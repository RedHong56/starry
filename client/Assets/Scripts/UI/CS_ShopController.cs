using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopController : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button     closeButton;

    [Header("상품 버튼 (coins_10 / coins_30 / coins_60 순)")]
    [SerializeField] private Button[]   productButtons;
    [SerializeField] private TMP_Text[] productLabels;

    [Header("현재 보유량 표시")]
    [SerializeField] private TMP_Text currentCoinsText;

    [Header("로딩")]
    [SerializeField] private StarSpinner loadingSpinner;

    private static readonly string[] ProductIds    = { "coins_10", "coins_30", "coins_60" };
    private static readonly int[]    ProductCoins  = { 10, 30, 60 };
    private static readonly string[] ProductPrices = { "₩1,200", "₩3,300", "₩5,900" };

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

    public void Open()
    {
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
        FindObjectOfType<UserStatusHUD>()?.Refresh();
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
