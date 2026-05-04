using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

// 코인 상품 정의
[Serializable]
public class CoinProduct
{
    public string productId;
    public int    coins;
    public string priceLabel;
}

public class PaymentController : MonoBehaviour
{
    public static PaymentController Instance { get; private set; }

    [SerializeField] private string purchaseApiUrl = "https://your-backend.com/api/payment/purchase";

    // TODO: Unity IAP 상품 ID 설정
    public CoinProduct[] products = new CoinProduct[]
    {
        new CoinProduct { productId = "coins_10", coins = 10, priceLabel = "₩1,200" },
        new CoinProduct { productId = "coins_30", coins = 30, priceLabel = "₩3,300" },
        new CoinProduct { productId = "coins_60", coins = 60, priceLabel = "₩5,900" },
    };

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // TODO: Unity IAP 초기화
        // var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        // foreach (var p in products)
        //     builder.AddProduct(p.productId, ProductType.Consumable);
        // UnityPurchasing.Initialize(this, builder);
    }

    public void Purchase(string productId, Action onSuccess, Action onFail)
    {
        // TODO: Unity IAP SDK 연결 후 아래 주석 해제
        // m_Controller.InitiatePurchase(productId);
        // 구매 완료는 ProcessPurchase 콜백에서 처리

        // 임시: SDK 없을 때 바로 성공 처리
        StartCoroutine(ValidatePurchaseRoutine(productId, "dummy_receipt", onSuccess, onFail));
    }

    // TODO: IStoreListener.ProcessPurchase 구현 시 receipt를 실제 영수증으로 교체
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
            yield return UserDataManager.Instance.FetchUserDataRoutine();
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
