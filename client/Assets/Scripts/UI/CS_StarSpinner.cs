using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StarSpinner : MonoBehaviour
{
    [SerializeField] private Image[] stars;
    [SerializeField] private float radius = 40f;
    [SerializeField] private float duration = 1.2f;

    private void Start()
    {
        Hide();
        for (int i = 0; i < stars.Length; i++)
        {
            float angle = i * (360f / stars.Length) * Mathf.Deg2Rad;
            int index = i;

            DOTween.To(
                () => angle,
                a => {
                    angle = a;
                    stars[index].rectTransform.anchoredPosition =
                        new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
                },
                angle + Mathf.PI * 2,
                duration
            ).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);

            stars[i].transform
                .DOScale(0.5f, duration * 0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(i * (duration / stars.Length));
        }
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
