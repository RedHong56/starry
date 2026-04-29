using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

// 카드 선택 캐러셀 - 도넛(원형) 배치, 드래그로 회전
public class CardDeckController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private const int TotalCards      = 78;
    private const int RequiredSelects = 3;

    [Header("Card Layout")]
    [SerializeField] private GameObject    cardPrefab;
    [SerializeField] private RectTransform cardContainer;
    [SerializeField] private float         radius          = 300f;
    [SerializeField] private float         cardScale       = 1f;    // 카드 크기 (밀도 조절)
    [SerializeField] private float         elevationOffset = 30f; // 선택중인 카드 높이
    [SerializeField] private float         snapDuration    = 0.25f; //자동 정렬(snap)되는 애니메이션 시간
    [SerializeField] private float         dragSensitivity = 0.05f;

    [Header("Shuffle Animation")]
    [SerializeField] private float shuffleDuration = 0.6f;
    [SerializeField] private float shuffleStagger  = 0.008f; // 카드 한 장당 딜레이

    [Header("Selected Slots (0=과거, 1=현재, 2=미래)")]
    [SerializeField] private Transform[] selectedSlots;

    [Header("UI")]
    [SerializeField] private GameObject deckPanel;
    [SerializeField] private Button     confirmButton;

    private RectTransform[]    _cards;
    private readonly HashSet<int> _usedIndices = new();
    private int                _currentIndex;
    private int                _confirmedCount;
    private int[]              _selectedIndices;
    private Action<int[]>      _onComplete;

    private float _dragStartX;
    private float _containerStartAngle;
    private float _currentAngle;
    private int   _highlightedIndex = -1;

    private float AngleStep => 360f / TotalCards;

    private void Awake()
    {
        confirmButton.onClick.AddListener(OnConfirm);
        deckPanel.SetActive(false);
    }

    public void StartSelection(Action<int[]> onComplete)
    {
        _onComplete      = onComplete;
        _confirmedCount  = 0;
        _currentIndex    = 0;
        _currentAngle    = 0f;
        _highlightedIndex = -1;
        _selectedIndices = new int[RequiredSelects];
        _usedIndices.Clear();

        deckPanel.SetActive(true);
        confirmButton.interactable = false;
        SpawnCards();
        PlayShuffleAnimation();
    }

    // ── 카드 생성 ─────────────────────────────────────────────────────────────

    private void SpawnCards()
    {
        foreach (Transform child in cardContainer)
            Destroy(child.gameObject);

        _cards = new RectTransform[TotalCards];
        for (int i = 0; i < TotalCards; i++)
        {
            GameObject go = Instantiate(cardPrefab, cardContainer);
            _cards[i] = go.GetComponent<RectTransform>();
            _cards[i].localScale = Vector3.one * cardScale;

            float angle = i * AngleStep;
            _cards[i].localEulerAngles = new Vector3(0f, 0f, -angle);
            _cards[i].anchoredPosition = Vector2.zero; // 셔플 시작점

            var outline = go.AddComponent<Outline>();
            outline.effectColor    = Color.yellow;
            outline.effectDistance = new Vector2(4f, -4f);
            outline.enabled        = false;
        }
    }

    // ── 셔플 애니메이션 ───────────────────────────────────────────────────────

    private void PlayShuffleAnimation()
    {
        cardContainer.localEulerAngles = Vector3.zero;

        for (int i = 0; i < TotalCards; i++)
        {
            float angle = i * AngleStep;
            float rad   = angle * Mathf.Deg2Rad;
            Vector2 target = new Vector2(Mathf.Sin(rad) * radius, Mathf.Cos(rad) * radius);

            // 랜덤 초기 회전으로 섞이는 느낌
            _cards[i].localEulerAngles = new(0f, 0f, UnityEngine.Random.Range(-180f, 180f));

            float delay = i * shuffleStagger;
            _cards[i].DOAnchorPos(target, shuffleDuration)
                .SetEase(Ease.OutQuart)
                .SetDelay(delay);
            _cards[i].DOLocalRotate(new Vector3(0f, 0f, -angle), shuffleDuration)
                .SetEase(Ease.OutQuart)
                .SetDelay(delay);
        }

        float totalTime = shuffleDuration + (TotalCards - 1) * shuffleStagger;
        StartCoroutine(EnableConfirmAfter(totalTime));
    }

    private IEnumerator EnableConfirmAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        confirmButton.interactable = true;
        HighlightCurrent(animated: true);
    }

    // ── Drag 처리 ────────────────────────────────────────────────────────────

    public void OnBeginDrag(PointerEventData e)
    {
        _dragStartX          = e.position.x;
        _containerStartAngle = _currentAngle;
    }

    public void OnDrag(PointerEventData e)
    {
        float delta = e.position.x - _dragStartX;
        _currentAngle = _containerStartAngle + delta * dragSensitivity;
        cardContainer.localEulerAngles = new Vector3(0f, 0f, _currentAngle);
        UpdateCurrentIndex();
        HighlightCurrent(animated: false);
    }

    public void OnEndDrag(PointerEventData e)
    {
        SnapToCurrent();
    }

    // ── 인덱스 계산 ──────────────────────────────────────────────────────────

    private void UpdateCurrentIndex()
    {
        float normalized = ((_currentAngle % 360f) + 360f) % 360f;
        int   index      = Mathf.RoundToInt(normalized / AngleStep) % TotalCards;

        int next = index;
        while (_usedIndices.Contains(next))
            next = (next + 1) % TotalCards;
        _currentIndex = next;
    }

    private void SnapToCurrent()
    {
        float targetAngle = _currentIndex * AngleStep;
        float diff        = Mathf.DeltaAngle(_currentAngle, targetAngle);
        _currentAngle    += diff;

        cardContainer.DOLocalRotate(new Vector3(0f, 0f, _currentAngle), snapDuration)
            .SetEase(Ease.OutQuad);
        HighlightCurrent(animated: true);
    }

    // ── 위치 갱신 ─────────────────────────────────────────────────────────────

    private void HighlightCurrent(bool animated)
    {
        if (_highlightedIndex >= 0 && _highlightedIndex < _cards.Length)
        {
            _cards[_highlightedIndex].GetComponent<Outline>().enabled = false;
            SetCardLocalPos(_highlightedIndex, animated);
        }

        if (!_usedIndices.Contains(_currentIndex))
        {
            _cards[_currentIndex].GetComponent<Outline>().enabled = true;
            _highlightedIndex = _currentIndex;
        }

        SetCardLocalPos(_currentIndex, animated);
    }

    private void SetCardLocalPos(int i, bool animated)
    {
        if (_usedIndices.Contains(i)) return;

        float angle = i * AngleStep;
        float rad   = angle * Mathf.Deg2Rad;
        float r     = (i == _currentIndex) ? radius + elevationOffset : radius;
        Vector2 pos = new(Mathf.Sin(rad) * r, Mathf.Cos(rad) * r);

        if (animated)
            _cards[i].DOAnchorPos(pos, snapDuration).SetEase(Ease.OutQuad);
        else
            _cards[i].anchoredPosition = pos;
    }

    // ── 카드 확정 ─────────────────────────────────────────────────────────────

    private void OnConfirm()
    {
        if (_confirmedCount >= RequiredSelects) return;
        if (_usedIndices.Contains(_currentIndex)) return;

        _usedIndices.Add(_currentIndex);
        _selectedIndices[_confirmedCount] = _currentIndex;

        RectTransform card = _cards[_currentIndex];
        Transform     slot = selectedSlots[_confirmedCount];

        card.GetComponent<Outline>().enabled = false;
        _highlightedIndex = -1;

        card.SetParent(slot, worldPositionStays: true);
        card.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutQuad);
        card.DOLocalRotate(Vector3.zero, 0.5f).SetEase(Ease.OutQuad);

        _confirmedCount++;

        if (_confirmedCount >= RequiredSelects)
        {
            confirmButton.interactable = false;
            DOVirtual.DelayedCall(0.6f, () =>
            {
                deckPanel.SetActive(false);
                _onComplete?.Invoke(_selectedIndices);
            });
        }
        else
        {
            int next = (_currentIndex + 1) % TotalCards;
            while (_usedIndices.Contains(next))
                next = (next + 1) % TotalCards;
            _currentIndex = next;
            SnapToCurrent();
        }
    }
}
