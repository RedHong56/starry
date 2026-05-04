using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PaymentChoiceController : MonoBehaviour
{
    [SerializeField] private GameObject choicePanel;

    [Header("별가루")]
    [SerializeField] private Button    starDustButton;
    [SerializeField] private TMP_Text  starDustCoinText;

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
        SetButtons(false);
        StartCoroutine(ConsumeRoutine());
    }

    private IEnumerator ConsumeRoutine()
    {
        yield return UserDataManager.Instance.ConsumeReadingRoutine(success =>
        {
            choicePanel.SetActive(false);
            if (success) _onConfirm?.Invoke();
            else         _onCancel?.Invoke();
        });
    }

    private void OnAdClicked()
    {
        SoundManager.Instance?.PlayBtn();
        SetButtons(false);

        AdRewardController.Instance.ShowRewardedAd(
            onSuccess: () =>
            {
                choicePanel.SetActive(false);
                _onConfirm?.Invoke();
            },
            onFail: () =>
            {
                SetButtons(true);
            }
        );
    }

    private void OnCancelClicked()
    {
        SoundManager.Instance?.PlayBtn();
        choicePanel.SetActive(false);
        _onCancel?.Invoke();
    }

    private void SetButtons(bool interactable)
    {
        starDustButton.interactable = interactable && (UserDataManager.Instance?.CanUseTarot() ?? false);
        adButton.interactable       = interactable;
        cancelButton.interactable   = interactable;
    }
}
