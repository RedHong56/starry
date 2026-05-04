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

    private void Start()
    {
        StartCoroutine(SplashRoutine());
    }

    private IEnumerator SplashRoutine()
    {
        if (logoGroup != null)
        {
            logoGroup.alpha = 0f;
            DOTween.Sequence()
                .Append(logoGroup.DOFade(1f, fadeInDuration))
                .AppendInterval(holdDuration)
                .Append(logoGroup.DOFade(0f, fadeOutDuration));
        }

        yield return new WaitForSeconds(fadeInDuration + holdDuration + fadeOutDuration);
        SceneManager.LoadScene("02_Login");
    }
}
