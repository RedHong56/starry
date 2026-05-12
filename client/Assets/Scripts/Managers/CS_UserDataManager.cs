using System;
using System.Collections;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class UserDataManager : MonoBehaviour
{
    public static UserDataManager Instance { get; private set; }

    public string   UserId             { get; private set; }
    public int      Coins              { get; private set; }
    public bool     HasFreeCoupon      { get; private set; }
    public DateTime? FreeCouponRefreshAt { get; private set; }

    private readonly string userDataApiUrl = AppSecrets.BackendBaseUrl + "/api/user/me";
    private readonly string consumeApiUrl  = AppSecrets.BackendBaseUrl + "/api/user/consume";
    private readonly string adRewardApiUrl = AppSecrets.BackendBaseUrl + "/api/user/ad-reward";

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInit()
    {
        if (Instance != null) return;
        var go = new GameObject("[UserDataManager]");
        go.AddComponent<UserDataManager>();
    }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator FetchUserDataRoutine()
    {
        using var req = UnityWebRequest.Get(userDataApiUrl);
        req.timeout = 15;
        AddAuthHeader(req);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var data   = JsonUtility.FromJson<UserDataResponse>(req.downloadHandler.text);
            UserId             = data.userId;
            Coins              = data.coins;
            HasFreeCoupon      = data.hasFreeCoupon;
            FreeCouponRefreshAt = string.IsNullOrEmpty(data.freeCouponRefreshAt)
                ? (DateTime?)null
                : DateTime.Parse(data.freeCouponRefreshAt, null, DateTimeStyles.RoundtripKind).ToUniversalTime();
        }
        else
        {
            Debug.LogWarning($"[UserDataManager] fetch failed: {req.error}. Using defaults.");
            UserId              = "guest";
            Coins               = 0;
            HasFreeCoupon       = false;
            FreeCouponRefreshAt = null;
        }
    }

    public IEnumerator ConsumeReadingRoutine(Action<bool> onResult)
    {
        using var req = new UnityWebRequest(consumeApiUrl, "POST")
        {
            uploadHandler   = new UploadHandlerRaw(Array.Empty<byte>()),
            downloadHandler = new DownloadHandlerBuffer(),
            timeout         = 15,
        };
        req.SetRequestHeader("Content-Type", "application/json");
        AddAuthHeader(req);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            if (HasFreeCoupon) HasFreeCoupon = false;
            else Coins = Mathf.Max(0, Coins - 1);
            onResult?.Invoke(true);
        }
        else
        {
            Debug.LogWarning($"[UserDataManager] consume failed: {req.error}");
            onResult?.Invoke(false);
        }
    }

    public IEnumerator AdRewardRoutine(Action<bool> onResult)
    {
        using var req = new UnityWebRequest(adRewardApiUrl, "POST")
        {
            uploadHandler   = new UploadHandlerRaw(Array.Empty<byte>()),
            downloadHandler = new DownloadHandlerBuffer(),
            timeout         = 15,
        };
        req.SetRequestHeader("Content-Type", "application/json");
        AddAuthHeader(req);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("[UserDataManager] ad reward success");
            onResult?.Invoke(true);
        }
        else
        {
            Debug.LogWarning($"[UserDataManager] ad reward failed: {req.error}");
            onResult?.Invoke(false);
        }
    }

    public bool CanUseTarot() => HasFreeCoupon || Coins > 0;

    private void AddAuthHeader(UnityWebRequest req)
    {
        string token = AuthManager.Instance?.Token;
        if (!string.IsNullOrEmpty(token))
            req.SetRequestHeader("Authorization", $"Bearer {token}");
    }

    [Serializable]
    private class UserDataResponse
    {
        public string userId;
        public int    coins;
        public bool   hasFreeCoupon;
        public string freeCouponRefreshAt;
    }
}
