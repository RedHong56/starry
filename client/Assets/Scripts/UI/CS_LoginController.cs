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

    private void Awake()
    {
        loadingSpinner.Hide();
        kakaoButton.onClick.AddListener(OnKakaoClicked);
        googleButton.onClick.AddListener(OnGoogleClicked);
        appleButton.onClick.AddListener(OnAppleClicked);
    }

    private void OnKakaoClicked()
    {
        // TODO: Kakao Unity SDK 연결 후 실제 토큰으로 교체
        // KakaoGame.Login(onSuccess: token => StartCoroutine(LoginRoutine("kakao", token)));
        StartCoroutine(LoginRoutine("kakao", "kakao_dummy_token"));
    }

    private void OnGoogleClicked()
    {
        // TODO: Google Sign-In SDK 연결 후 실제 토큰으로 교체
        // GoogleSignIn.Login(onSuccess: token => StartCoroutine(LoginRoutine("google", token)));
        StartCoroutine(LoginRoutine("google", "google_dummy_token"));
    }

    private void OnAppleClicked()
    {
        // TODO: Sign in with Apple SDK 연결 후 실제 토큰으로 교체
        // AppleAuthManager.Login(onSuccess: token => StartCoroutine(LoginRoutine("apple", token)));
        StartCoroutine(LoginRoutine("apple", "apple_dummy_token"));
    }

    private IEnumerator LoginRoutine(string provider, string socialToken)
    {
        SetLoading(true);

        bool authSuccess = false;
        yield return AuthManager.Instance.AuthenticateRoutine(provider, socialToken, result => authSuccess = result);

        if (!authSuccess)
        {
            SetLoading(false);
            Debug.LogWarning("[LoginController] 인증 실패");
            yield break;
        }

        yield return UserDataManager.Instance.FetchUserDataRoutine();

        SetLoading(false);
        SceneManager.LoadScene("Main");
    }

    private void SetLoading(bool on)
    {
        if (on) loadingSpinner.Show(); else loadingSpinner.Hide();
        kakaoButton.interactable = !on;
        googleButton.interactable = !on;
        appleButton.interactable  = !on;
    }
}
