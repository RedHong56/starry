using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginController : MonoBehaviour
{
    [SerializeField] private Button kakaoButton;
    [SerializeField] private Button googleButton;
    [SerializeField] private Button appleButton;
    [SerializeField] private StarSpinner loadingSpinner;

    // 앱 콜드스타트 딥링크는 한 번만 처리 (로그아웃 후 씬 재로드 시 재실행 방지)
    private static bool   _coldStartUrlConsumed = false;
    // 동일 JWT 재처리 방지 (Android에서 deepLinkActivated 이중 호출 대응)
    private static string _lastHandledJwt = null;

    private static readonly string KakaoRestApiKey  = AppSecrets.KakaoRestApiKey;
    private static readonly string KakaoRedirectUri = AppSecrets.KakaoRedirectUri;

    private void Awake()
    {
        loadingSpinner.Hide();
        appleButton.gameObject.SetActive(Application.platform == RuntimePlatform.IPhonePlayer);
        kakaoButton.onClick.AddListener(OnKakaoClicked);
        googleButton.onClick.AddListener(OnGoogleClicked);
        appleButton.onClick.AddListener(OnAppleClicked);

        Application.deepLinkActivated += OnDeepLinkActivated;

        // 앱이 딥링크로 콜드 스타트된 경우 (씬 재로드 시에는 무시)
        if (!_coldStartUrlConsumed && !string.IsNullOrEmpty(Application.absoluteURL))
        {
            _coldStartUrlConsumed = true;
            HandleDeepLink(Application.absoluteURL);
        }

        StartCoroutine(AutoLoginRoutine());
    }

    private void OnDestroy()
    {
        Application.deepLinkActivated -= OnDeepLinkActivated;
    }

    private IEnumerator AutoLoginRoutine()
    {
        if (AuthManager.Instance == null || !AuthManager.Instance.IsLoggedIn) yield break;

        SetLoading(true);
        bool valid = false;
        yield return AuthManager.Instance.ValidateTokenRoutine(result => valid = result);

        if (valid)
        {
            yield return UserDataManager.Instance.FetchUserDataRoutine();
            SceneManager.LoadScene("03_Main");
        }
        else
        {
            SetLoading(false);
        }
    }

    private void OnKakaoClicked()
    {
        // TODO: Mock 해제 후 아래 웹 OAuth 블록으로 교체
        // StartCoroutine(TokenLoginRoutine("kakao", "kakao_dummy_token"));

        // 실제 카카오 웹 OAuth (APK 빌드 후 딥링크 테스트 시 사용)
        SetLoading(true);
        var encodedRedirect = Uri.EscapeDataString(KakaoRedirectUri);
        Application.OpenURL(
            $"https://kauth.kakao.com/oauth/authorize" +
            $"?client_id={KakaoRestApiKey}" +
            $"&redirect_uri={encodedRedirect}" +
            $"&response_type=code"
        );
    }

    private void OnGoogleClicked()
    {
#if UNITY_EDITOR
        StartCoroutine(TokenLoginRoutine("google", "google_dummy_token"));
#else
        SetLoading(true);
        Application.OpenURL($"{AppSecrets.BackendBaseUrl}/api/auth/google");
#endif
    }

    private void OnAppleClicked()
    {
        // TODO: Apple Sign-In SDK 연결 후 교체
        StartCoroutine(TokenLoginRoutine("apple", "apple_dummy_token"));
    }

    private void OnDeepLinkActivated(string url) => HandleDeepLink(url);

    private void HandleDeepLink(string url)
    {
        // 예상 URL: starry://auth?jwt=XXXXX  또는  starry://auth?error=...
        if (!url.StartsWith("starry://auth")) return;

        var jwt   = ExtractQueryParam(url, "jwt");
        var error = ExtractQueryParam(url, "error");

        if (!string.IsNullOrEmpty(jwt))
            StartCoroutine(FinishLoginWithJwt(jwt));
        else
        {
            Debug.LogWarning("[LoginController] 카카오 로그인 실패: " + error);
            SetLoading(false);
        }
    }

    private IEnumerator FinishLoginWithJwt(string jwt)
    {
        if (jwt == _lastHandledJwt) { SetLoading(false); yield break; }
        _lastHandledJwt = jwt;
        AuthManager.Instance.StoreToken(jwt);
        yield return UserDataManager.Instance.FetchUserDataRoutine();
        SetLoading(false);
        SceneManager.LoadScene("03_Main");
    }

    private IEnumerator TokenLoginRoutine(string provider, string token)
    {
        SetLoading(true);
        bool success = false;
        yield return AuthManager.Instance.AuthenticateRoutine(provider, token, r => success = r);

        if (!success)
        {
            SetLoading(false);
            Debug.LogWarning("[LoginController] 인증 실패");
            yield break;
        }
        yield return UserDataManager.Instance.FetchUserDataRoutine();
        SetLoading(false);
        SceneManager.LoadScene("03_Main");
    }

    private void SetLoading(bool on)
    {
        if (on) loadingSpinner.Show(); else loadingSpinner.Hide();
        kakaoButton.interactable  = !on;
        googleButton.interactable = !on;
        appleButton.interactable  = !on;
    }

    private static string ExtractQueryParam(string url, string key)
    {
        var query = url.Contains("?") ? url.Substring(url.IndexOf('?') + 1) : string.Empty;
        foreach (var pair in query.Split('&'))
        {
            var kv = pair.Split('=');
            if (kv.Length == 2 && kv[0] == key)
                return Uri.UnescapeDataString(kv[1]);
        }
        return null;
    }
}
