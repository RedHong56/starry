using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class UserDataManager : MonoBehaviour
{
    public static UserDataManager Instance { get; private set; }

    public string UserId   { get; private set; }
    public int    Coins    { get; private set; }
    public bool   HasFreeCoupon { get; private set; }

    [SerializeField] private string userDataApiUrl = "https://your-backend.com/api/user/me";

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator FetchUserDataRoutine()
    {
        UnityWebRequest req = UnityWebRequest.Get(userDataApiUrl);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var data = JsonUtility.FromJson<UserDataResponse>(req.downloadHandler.text);
            UserId       = data.userId;
            Coins        = data.coins;
            HasFreeCoupon = data.hasFreeCoupon;
        }
        else
        {
            Debug.LogWarning($"[UserDataManager] fetch failed: {req.error}. Using defaults.");
            UserId       = "guest";
            Coins        = 0;
            HasFreeCoupon = false;
        }

        req.Dispose();
    }

    public bool CanUseTarot() => HasFreeCoupon || Coins > 0;

    public void ConsumeReading()
    {
        if (HasFreeCoupon)
            HasFreeCoupon = false;
        else
            Coins = Mathf.Max(0, Coins - 1);
    }

    [Serializable]
    private class UserDataResponse
    {
        public string userId;
        public int    coins;
        public bool   hasFreeCoupon;
    }
}
