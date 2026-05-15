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

    [Header("상점")]
    [SerializeField] private ShopController shopController;

    [Header("로딩")]
    [SerializeField] private StarSpinner loadingSpinner;

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

        RefreshStarDustButton();

        SoundManager.Instance?.PlayPannel();
        choicePanel.SetActive(true);
    }

    private void RefreshStarDustButton()
    {
        int coins = UserDataManager.Instance != null ? UserDataManager.Instance.Coins : 0;
        if (starDustCoinText != null)
            starDustCoinText.text = coins > 0 ? $"{coins}개 보유" : "구매하기";
    }

    private void OnStarDustClicked()
    {
        SoundManager.Instance?.PlayBtn();
        int coins = UserDataManager.Instance != null ? UserDataManager.Instance.Coins : 0;
        if (coins > 0)
        {
            SetButtons(false);
            StartCoroutine(ConsumeRoutine());
        }
        else
        {
            shopController.Open(onClose: RefreshStarDustButton);
        }
    }

    private IEnumerator ConsumeRoutine()
    {
        if (loadingSpinner != null) loadingSpinner.Show();
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
