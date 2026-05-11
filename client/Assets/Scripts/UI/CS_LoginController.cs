using System;
using System.Collections;
using Google;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginController : MonoBehaviour
{
    [SerializeField] private Button kakaoButton;
    [SerializeField] private Button googleButton;
    [SerializeField] private Button appleButton;
    [SerializeField] private StarSpinner loadingSpinner;

    private static readonly string KakaoRestApiKey  = AppSecrets.KakaoRestApiKey;
    private static readonly string KakaoRedirectUri = AppSecrets.KakaoRedirectUri;

    private void Awake()
    {
        loadingSpinner.Hide();
        kakaoButton.onClick.AddListener(OnKakaoClicked);
        googleButton.onClick.AddListener(OnGoogleClicked);
        appleButton.onClick.AddListener(OnAppleClicked);

        Application.deepLinkActivated += OnDeepLinkActivated;

        // 앱이 딥링크로 콜드 스타트된 경우
        if (!string.IsNullOrEmpty(Application.absoluteURL))
            HandleDeepLink(Application.absoluteURL);

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
        StartCoroutine(TokenLoginRoutine("kakao", "kakao_dummy_token"));

        // 실제 카카오 웹 OAuth (APK 빌드 후 딥링크 테스트 시 사용)
        // SetLoading(true);
        // var encodedRedirect = Uri.EscapeDataString(KakaoRedirectUri);
        // Application.OpenURL(
        //     $"https://kauth.kakao.com/oauth/authorize" +
        //     $"?client_id={KakaoRestApiKey}" +
        //     $"&redirect_uri={encodedRedirect}" +
        //     $"&response_type=code"
        // );
    }

    private void OnGoogleClicked()
    {
        GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            WebClientId    = AppSecrets.GoogleWebClientId,
            RequestIdToken = true,
            UseGameSignIn  = false,
        };
        SetLoading(true);
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogWarning("[LoginController] Google 로그인 실패: " + task.Exception);
                SetLoading(false);
                return;
            }
            var idToken = task.Result.IdToken;
            StartCoroutine(TokenLoginRoutine("google", idToken));
        });
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
