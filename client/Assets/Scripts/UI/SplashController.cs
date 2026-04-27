// Scripts/Core/SplashController.cs

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SplashController : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup logoGroup;   // 로고 캔버스 그룹
    public Image logoImage;         // 로고 이미지

    [Header("설정")]
    public float fadeInDuration = 1f;
    public float holdDuration = 1.5f;
    public float fadeOutDuration = 0.8f;

    void Start()
    {
        // 처음엔 투명
        logoGroup.alpha = 0f;

        // 순서대로 실행
        Sequence seq = DOTween.Sequence();

        // 1. 페이드인
        seq.Append(logoGroup.DOFade(1f, fadeInDuration));

        // 2. 잠깐 유지
        seq.AppendInterval(holdDuration);

        // 3. 페이드아웃
        seq.Append(logoGroup.DOFade(0f, fadeOutDuration));

        // 4. S#2로 이동
        seq.OnComplete(() =>
        {
            SceneManager.LoadScene("02_Main");
        });
    }
}