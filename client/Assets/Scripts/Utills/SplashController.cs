using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SplashController : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup logoGroup;
    public Image       logoImage;

    [Header("Settings")]
    public float fadeInDuration  = 1f;
    public float holdDuration    = 1.5f;
    public float fadeOutDuration = 0.8f;

    private bool _logoAnimDone  = false;
    private bool _dataFetchDone = false;

    private void Start()
    {
        EnsureUserDataManager();
        StartCoroutine(FetchDataRoutine());
        PlayLogoAnimation();
    }

    private void EnsureUserDataManager()
    {
        if (UserDataManager.Instance == null)
            new GameObject("UserDataManager").AddComponent<UserDataManager>();
    }

    private IEnumerator FetchDataRoutine()
    {
        yield return UserDataManager.Instance.FetchUserDataRoutine();
        _dataFetchDone = true;
        TryLoadMain();
    }

    private void PlayLogoAnimation()
    {
        logoGroup.alpha = 0f;
        DOTween.Sequence()
            .Append(logoGroup.DOFade(1f, fadeInDuration))
            .AppendInterval(holdDuration)
            .Append(logoGroup.DOFade(0f, fadeOutDuration))
            .OnComplete(() => { _logoAnimDone = true; TryLoadMain(); });
    }

    private void TryLoadMain()
    {
        if (_logoAnimDone && _dataFetchDone)
            SceneManager.LoadScene("02_Main");
    }
}
