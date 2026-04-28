using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

// 카드 선택 캐러셀 (과거/현재/미래 3장 선택)
public class CardDeckController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private const int TotalCards      = 78;
    private const int RequiredSelects = 3;

    [Header("Card Layout")]
    [SerializeField] private GameObject  cardPrefab;
    [SerializeField] private Transform   cardContainer;
    [SerializeField] private float       cardSpacing    = 180f;
    [SerializeField] private float       elevationY     = 45f;
    [SerializeField] private float       snapDuration   = 0.25f;

    [Header("Selected Slots (0=과거, 1=현재, 2=미래)")]
    [SerializeField] private Transform[] selectedSlots;

    [Header("UI")]
    [SerializeField] private GameObject deckPanel;
    [SerializeField] private Button     confirmButton;

    private RectTransform[]  _cards;
    private HashSet<int>     _usedIndices = new HashSet<int>();
    private int              _currentIndex;
    private int              _confirmedCount;
    private int[]            _selectedIndices;
    private Action<int[]>    _onComplete;

    private float _dragStartX;
    private float _containerStartX;

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
        _selectedIndices = new int[RequiredSelects];
        _usedIndices.Clear();

        deckPanel.SetActive(true);
        confirmButton.interactable = true;
        SpawnCards();
        RefreshPositions(animated: false);
    }

    private void SpawnCards()
    {
        foreach (Transform child in cardContainer)
            Destroy(child.gameObject);

        _cards = new RectTransform[TotalCards];
        for (int i = 0; i < TotalCards; i++)
        {
            GameObject go = Instantiate(cardPrefab, cardContainer);
            _cards[i] = go.GetComponent<RectTransform>();
        }
    }

    // ── Drag 처리 ─────────────────────────────────────────────────────────────

    public void OnBeginDrag(PointerEventData e)
    {
        _dragStartX     = e.position.x;
        _containerStartX = cardContainer.localPosition.x;
    }

    public void OnDrag(PointerEventData e)
    {
        float delta = e.position.x - _dragStartX;
        cardContainer.localPosition = new Vector3(_containerStartX + delta, 0f, 0f);
    }

    public void OnEndDrag(PointerEventData e)
    {
        float delta      = e.position.x - _dragStartX;
        int   indexDelta = Mathf.RoundToInt(-delta / cardSpacing);
        _currentIndex    = Mathf.Clamp(_currentIndex + indexDelta, 0, TotalCards - 1);

        // 이미 선택된 카드면 옆으로 건너뜀
        while (_usedIndices.Contains(_currentIndex) && _currentIndex < TotalCards - 1)
            _currentIndex++;

        RefreshPositions(animated: true);
    }

    // ── 위치 갱신 ──────────────────────────────────────────────────────────────

    private void RefreshPositions(bool animated)
    {
        float targetContainerX = -_currentIndex * cardSpacing;

        if (animated)
            cardContainer.DOLocalMoveX(targetContainerX, snapDuration).SetEase(Ease.OutQuad);
        else
            cardContainer.localPosition = new Vector3(targetContainerX, 0f, 0f);

        for (int i = 0; i < _cards.Length; i++)
        {
            float targetY = (i == _currentIndex) ? elevationY : 0f;
            Vector2 pos   = new Vector2(i * cardSpacing, targetY);

            if (animated)
                _cards[i].DOAnchorPos(pos, snapDuration).SetEase(Ease.OutQuad);
            else
                _cards[i].anchoredPosition = pos;
        }
    }

    // ── 카드 확정 ──────────────────────────────────────────────────────────────

    private void OnConfirm()
    {
        if (_confirmedCount >= RequiredSelects) return;
        if (_usedIndices.Contains(_currentIndex)) return;

        _usedIndices.Add(_currentIndex);
        _selectedIndices[_confirmedCount] = _currentIndex;

        RectTransform card = _cards[_currentIndex];
        Transform     slot = selectedSlots[_confirmedCount];

        // 카드를 슬롯으로 이동
        card.DOMove(slot.position, 0.5f).SetEase(Ease.InOutQuad);
        card.DOScale(Vector3.one * 0.75f, 0.5f);

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
    }
}
