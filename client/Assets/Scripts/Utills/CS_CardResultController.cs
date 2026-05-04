using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 결과 씬: 카드 순차 공개 → 누적 스크롤 → AI 해설 표시
public class CardResultController : MonoBehaviour
{
    private static readonly string[] SlotLabels = { "과거", "현재", "미래" };

    [Header("Card Slots (0=과거, 1=현재, 2=미래)")]
    [SerializeField] private GameObject SlotPannel;
    [SerializeField] private CardSlotView[] cardSlots;   // 슬롯마다 FlipView + arcana + name + image
    [SerializeField] private float          pauseBetweenCards = 2f;

    [Header("AI Result Panel (누적 스크롤)")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text   resultText;
    [SerializeField] private ScrollRect resultScroll;

    [Header("Dependencies")]
    [SerializeField] private TarotAIService   aiService;
    [SerializeField] private TypewriterEffect typewriter;

    private void Awake()
    {
        foreach (var slot in cardSlots)
            slot.Clear();

        resultPanel.SetActive(false);
    }

    /// <summary>
    /// 카드 공개 시작.
    /// cardIndices: 선택된 카드 id 배열 (0-77)
    /// isReversed: 각 카드의 역방향 여부
    /// </summary>
    // PhaseManager에서 AI를 미리 요청한 경우 — isAiReady/getAiResult 로 결과 수령
    public void StartReveal(int[] cardIndices, bool[] isReversed, string userWorry,
                             Func<bool> isAiReady, Func<string> getAiResult,
                             Action<int> beforeFlip = null, Action onComplete = null)
    {
        StartCoroutine(RevealRoutine(cardIndices, isReversed, userWorry,
                                     isAiReady, getAiResult, beforeFlip, onComplete));
    }

    public void StartReveal(int[] cardIndices, bool[] isReversed, string userWorry,
                             Action<int> beforeFlip = null, Action onComplete = null)
    {
        StartCoroutine(RevealRoutine(cardIndices, isReversed, userWorry,
                                     null, null, beforeFlip, onComplete));
    }

    // 기존 호환용 오버로드 (전부 정방향)
    public void StartReveal(int[] cardIndices, string userWorry)
    {
        bool[] upright = new bool[cardIndices.Length];
        StartReveal(cardIndices, upright, userWorry);
    }

    private IEnumerator RevealRoutine(int[] cardIndices, bool[] isReversed, string userWorry,
                                       Func<bool> isAiReady, Func<string> getAiResult,
                                       Action<int> beforeFlip, Action onComplete = null)
    {
        // 결과 패널 활성화 + 텍스트 초기화
        resultPanel.SetActive(true);
        resultText.text = string.Empty;

        for (int i = 0; i < cardIndices.Length; i++)
        {
            int id = cardIndices[i];
            bool reversed = isReversed != null && i < isReversed.Length && isReversed[i];

            // JSON 데이터에서 카드 정보 조회
            CardInfo info = CardInfoDatabase.Get(id);

            // Resources/Cards 경로에서 스프라이트 직접 로드
            string spritePath = CardInfoDatabase.GetResourcePath(info);
            Sprite frontSprite = spritePath != null ? Resources.Load<Sprite>(spritePath) : null;

            // ── 선택된 덱 카드 hide 후 플립 ──
            beforeFlip?.Invoke(i);
            yield return StartCoroutine(cardSlots[i].FlipView.FlipRoutine(frontSprite));

            // ── flip 후 카드 정보 UI 업데이트 ──
            cardSlots[i].ShowCardInfo(info, id);

            // ── 누적 스크롤에 설명 추가 ──
            string meaning = GetMeaning(info, reversed);
            string section = $"[{SlotLabels[i]}]\n\n{meaning}\n\n";
            resultText.text += section;
            ScrollToBottom();

            yield return new WaitForSeconds(pauseBetweenCards);
        }

        // ── AI 해설 대기 (PhaseManager에서 미리 요청한 결과를 수령) ──
        resultText.text += "[해설]\n\n";
        resultText.text += "점괘를 읽는 중...";
        ScrollToBottom();

        // 미리 요청한 결과가 없으면 직접 요청 (폴백)
        if (isAiReady == null)
        {
            bool done = false;
            string fallbackResult = null;
            aiService.GetTarotReading(cardIndices, userWorry, r => { fallbackResult = r; done = true; });
            yield return new WaitUntil(() => done);
            isAiReady   = () => true;
            getAiResult = () => fallbackResult;
        }

        yield return new WaitUntil(isAiReady);
        string aiResult = getAiResult();

        // "점괘를 읽는 중..." 제거 후 AI 결과 삽입
        resultText.text = resultText.text.Replace("점괘를 읽는 중...", aiResult);
        ScrollToBottom();

        onComplete?.Invoke();
    }

    public void HideResultPanel()
    {
        resultPanel.SetActive(false);
        SlotPannel.SetActive(false);
    }

    // ── 유틸 ──────────────────────────────────────────────────────────────────

    private string GetMeaning(CardInfo info, bool reversed)
    {
        if (info == null) return "카드 정보를 찾을 수 없습니다.";
        return reversed ? info.meaning.reversed : info.meaning.upright;
    }

    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        resultScroll.normalizedPosition = new Vector2(0f, 0f);
    }
}
