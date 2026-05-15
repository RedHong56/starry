using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

[Serializable]
public class CoinProduct
{
    public string productId;
    public int    coins;
    public string priceLabel;
}

public class PaymentController : MonoBehaviour, IDetailedStoreListener
{
    public static PaymentController Instance { get; private set; }

    private readonly string purchaseApiUrl = AppSecrets.BackendBaseUrl + "/api/payment/purchase";

    public CoinProduct[] products = new CoinProduct[]
    {
        new CoinProduct { productId = "stardust_10", coins = 10, priceLabel = "₩1,200" },
        new CoinProduct { productId = "stardust_30", coins = 30, priceLabel = "₩3,300" },
        new CoinProduct { productId = "stardust_70", coins = 70, priceLabel = "₩6,600" },
    };

    private IStoreController _storeController;
    private Action _onSuccess;
    private Action _onFail;
    private Product _pendingProduct;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        foreach (var p in products)
            builder.AddProduct(p.productId, ProductType.Consumable);
        UnityPurchasing.Initialize(this, builder);
    }

    // ── IDetailedStoreListener ────────────────────────────────────────────────

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        _storeController = controller;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogWarning($"[PaymentController] IAP init failed: {error}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogWarning($"[PaymentController] IAP init failed: {error} - {message}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        _pendingProduct = args.purchasedProduct;
        string productId = args.purchasedProduct.definition.id;
        string receipt   = args.purchasedProduct.receipt;
        StartCoroutine(ValidatePurchaseRoutine(productId, receipt, _onSuccess, _onFail));
        // 서버 검증 완료 후 ConfirmPendingPurchase 호출 → Pending 반환
        return PurchaseProcessingResult.Pending;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogWarning($"[PaymentController] Purchase failed: {failureReason}");
        _onFail?.Invoke();
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.LogWarning($"[PaymentController] Purchase failed: {failureDescription.message}");
        _onFail?.Invoke();
    }

    // ── 구매 시작 ─────────────────────────────────────────────────────────────

    public void Purchase(string productId, Action onSuccess, Action onFail)
    {
        if (_storeController == null)
        {
            Debug.LogWarning("[PaymentController] IAP가 아직 초기화되지 않았습니다.");
            onFail?.Invoke();
            return;
        }

        _onSuccess = onSuccess;
        _onFail    = onFail;
        _storeController.InitiatePurchase(productId);
    }

    // ── 백엔드 영수증 검증 ────────────────────────────────────────────────────

    private IEnumerator ValidatePurchaseRoutine(string productId, string receipt, Action onSuccess, Action onFail)
    {
        var body = JsonUtility.ToJson(new PurchaseRequest { productId = productId, receipt = receipt });

        using var req = new UnityWebRequest(purchaseApiUrl, "POST")
        {
            uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)),
            downloadHandler = new DownloadHandlerBuffer()
        };
        req.SetRequestHeader("Content-Type", "application/json");

        string token = AuthManager.Instance?.Token;
        if (!string.IsNullOrEmpty(token))
            req.SetRequestHeader("Authorization", $"Bearer {token}");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            // 서버 검증 완료 → 구매 확정
            if (_pendingProduct != null)
            {
                _storeController.ConfirmPendingPurchase(_pendingProduct);
                _pendingProduct = null;
            }
            onSuccess?.Invoke();
        }
        else
        {
            Debug.LogWarning($"[PaymentController] purchase failed: {req.error}");
            onFail?.Invoke();
        }
    }

    [Serializable] private class PurchaseRequest { public string productId; public string receipt; }
}
