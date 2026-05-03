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
        // TODO: Kakao Unity SDK 연결
        // KakaoGame.Login(onSuccess: token => StartCoroutine(LoginRoutine(token, "kakao")));
        StartCoroutine(LoginRoutine("kakao_dummy_token", "kakao"));
    }

    private void OnGoogleClicked()
    {
        // TODO: Google Sign-In SDK 연결
        // GoogleSignIn.Login(onSuccess: token => StartCoroutine(LoginRoutine(token, "google")));
        StartCoroutine(LoginRoutine("google_dummy_token", "google"));
    }

    private void OnAppleClicked()
    {
        // TODO: Sign in with Apple SDK 연결
        // AppleAuthManager.Login(onSuccess: token => StartCoroutine(LoginRoutine(token, "apple")));
        StartCoroutine(LoginRoutine("apple_dummy_token", "apple"));
    }

    private IEnumerator LoginRoutine(string token, string provider)
    {
        SetLoading(true);

        // TODO: 백엔드에 토큰 전달 → JWT 발급 → UserDataManager에 저장
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
