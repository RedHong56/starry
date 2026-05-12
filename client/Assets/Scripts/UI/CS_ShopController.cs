using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopController : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button     closeButton;

    [Header("상품 버튼 (stardust_10 / stardust_30 / stardust_70 순)")]
    [SerializeField] private Button[]   productButtons;
    [SerializeField] private TMP_Text[] productLabels;

    [Header("현재 보유량 표시")]
    [SerializeField] private TMP_Text currentCoinsText;

    [Header("로딩")]
    [SerializeField] private StarSpinner loadingSpinner;

    private static readonly string[] ProductIds    = { "stardust_10", "stardust_30", "stardust_70" };
    private static readonly int[]    ProductCoins  = { 10, 30, 70 };
    private static readonly string[] ProductPrices = { "₩1,200", "₩3,300", "₩6,600" };

    private Action _onClose;

    private void Awake()
    {
        shopPanel.SetActive(false);
        closeButton.onClick.AddListener(Close);

        for (int i = 0; i < productButtons.Length && i < ProductIds.Length; i++)
        {
            int idx = i;
            productButtons[i].onClick.AddListener(() => OnProductClicked(idx));
            if (productLabels != null && i < productLabels.Length)
                productLabels[i].text = $"별가루 {ProductCoins[i]}개\n{ProductPrices[i]}";
        }
    }

    public void Open(Action onClose = null)
    {
        _onClose = onClose;
        RefreshCoinsDisplay();
        SetButtons(true);
        loadingSpinner?.Hide();
        SoundManager.Instance?.PlayPannel();
        shopPanel.SetActive(true);
    }

    public void Close()
    {
        SoundManager.Instance?.PlayBtn();
        shopPanel.SetActive(false);
        _onClose?.Invoke();
        _onClose = null;
    }

    private void OnProductClicked(int idx)
    {
        SoundManager.Instance?.PlayBtn();
        SetButtons(false);
        loadingSpinner?.Show();

        PaymentController.Instance.Purchase(
            ProductIds[idx],
            onSuccess: () => StartCoroutine(OnPurchaseSuccess()),
            onFail:    () => StartCoroutine(OnPurchaseFail())
        );
    }

    private IEnumerator OnPurchaseSuccess()
    {
        yield return UserDataManager.Instance.FetchUserDataRoutine();
        RefreshCoinsDisplay();
        loadingSpinner?.Hide();
        SetButtons(true);
    }

    private IEnumerator OnPurchaseFail()
    {
        loadingSpinner?.Hide();
        SetButtons(true);
        yield break;
    }

    private void RefreshCoinsDisplay()
    {
        if (currentCoinsText != null && UserDataManager.Instance != null)
            currentCoinsText.text = $"보유 별가루: {UserDataManager.Instance.Coins}개";
    }

    private void SetButtons(bool interactable)
    {
        foreach (var btn in productButtons)
            if (btn != null) btn.interactable = interactable;
        closeButton.interactable = interactable;
    }
}
