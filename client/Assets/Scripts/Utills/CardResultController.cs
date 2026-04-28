using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 결과 씬: 카드 순차 공개 → AI 해설 표시
public class CardResultController : MonoBehaviour
{
    private static readonly string[] SlotLabels = { "과거", "현재", "미래" };

    [Header("Card Flip Views (0=과거, 1=현재, 2=미래)")]
    [SerializeField] private CardFlipView[] cardSlots;
    [SerializeField] private float          pauseBetweenCards = 2f;

    [Header("Card Description Popup")]
    [SerializeField] private GameObject descPanel;
    [SerializeField] private TMP_Text   descText;

    [Header("AI Result Panel")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text   resultText;
    [SerializeField] private ScrollRect resultScroll;

    [Header("Dependencies")]
    [SerializeField] private TarotAIService aiService;

    private TarotCardData[] _cardDatabase;

    private void Awake()
    {
        descPanel.SetActive(false);
        resultPanel.SetActive(false);

        // Resources/TarotCards/ 폴더에 TarotCardData 에셋을 배치해야 함
        _cardDatabase = Resources.LoadAll<TarotCardData>("TarotCards");
    }

    public void StartReveal(int[] cardIndices, string userWorry)
    {
        StartCoroutine(RevealRoutine(cardIndices, userWorry));
    }

    private IEnumerator RevealRoutine(int[] cardIndices, string userWorry)
    {
        for (int i = 0; i < cardIndices.Length; i++)
        {
            TarotCardData data = FindCard(cardIndices[i]);

            // 카드 설명 팝업 표시
            descText.text = $"[{SlotLabels[i]}] {data?.cardName ?? "Unknown"}\n{data?.description}";
            descPanel.SetActive(true);

            // 카드 뒤집기
            yield return StartCoroutine(cardSlots[i].FlipRoutine(data?.frontSprite));

            yield return new WaitForSeconds(pauseBetweenCards);
            descPanel.SetActive(false);
        }

        // 전체 해설 요청
        resultPanel.SetActive(true);
        resultText.text = "점괘를 읽는 중...";

        bool done = false;
        string aiResult = null;

        aiService.GetTarotReading(cardIndices, userWorry, result =>
        {
            aiResult = result;
            done     = true;
        });

        yield return new WaitUntil(() => done);

        resultText.text = aiResult;
        resultScroll.normalizedPosition = new Vector2(0f, 1f); // 스크롤 상단
    }

    private TarotCardData FindCard(int id)
    {
        foreach (var card in _cardDatabase)
            if (card.id == id) return card;
        return null;
    }
}
