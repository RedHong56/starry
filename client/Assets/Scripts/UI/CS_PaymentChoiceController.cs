using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PaymentChoiceController : MonoBehaviour
{
    [SerializeField] private GameObject choicePanel;

    [Header("별가루")]
    [SerializeField] private Button starDustButton;
    [SerializeField] private TMP_Text starDustCoinText;

    [Header("광고")]
    [SerializeField] private Button adButton;

    [Header("취소")]
    [SerializeField] private Button cancelButton;

    private Action _onConfirm;
    private Action _onCancel;

    private void Awake()
    {
        choicePanel.SetActive(false);

        starDustButton.onClick.AddListener(OnStarDustClicked);
        adButton.onClick.AddListener(OnAdClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
    }

    public void Open(Action onConfirm, Action onCancel)
    {
        _onConfirm = onConfirm;
        _onCancel  = onCancel;

        bool canUse = UserDataManager.Instance != null && UserDataManager.Instance.CanUseTarot();
        starDustButton.interactable = canUse;

        if (starDustCoinText != null && UserDataManager.Instance != null)
            starDustCoinText.text = $"별가루 {UserDataManager.Instance.Coins}개 보유";

        SoundManager.Instance?.PlayPannel();
        choicePanel.SetActive(true);
    }

    private void OnStarDustClicked()
    {
        SoundManager.Instance?.PlayBtn();
        UserDataManager.Instance?.ConsumeReading();
        choicePanel.SetActive(false);
        _onConfirm?.Invoke();
    }

    private void OnAdClicked()
    {
        SoundManager.Instance?.PlayBtn();
        // TODO: 광고 SDK 연결 후 콜백에서 호출
        choicePanel.SetActive(false);
        _onConfirm?.Invoke();
    }

    private void OnCancelClicked()
    {
        SoundManager.Instance?.PlayBtn();
        choicePanel.SetActive(false);
        _onCancel?.Invoke();
    }
}
