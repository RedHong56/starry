using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    [SerializeField] private string authApiUrl = "https://your-backend.com/api/auth";

    private const string TokenKey = "jwt_token";

    public string Token     => PlayerPrefs.GetString(TokenKey, string.Empty);
    public bool   IsLoggedIn => !string.IsNullOrEmpty(Token);

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator AuthenticateRoutine(string provider, string socialToken, Action<bool> onResult)
    {
        var body = JsonUtility.ToJson(new AuthRequest { provider = provider, token = socialToken });

        using var req = new UnityWebRequest($"{authApiUrl}/{provider}", "POST")
        {
            uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)),
            downloadHandler = new DownloadHandlerBuffer()
        };
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var res = JsonUtility.FromJson<AuthResponse>(req.downloadHandler.text);
            PlayerPrefs.SetString(TokenKey, res.jwt);
            onResult?.Invoke(true);
        }
        else
        {
            Debug.LogWarning($"[AuthManager] {provider} auth failed: {req.error}");
            onResult?.Invoke(false);
        }
    }

    public void Logout()
    {
        PlayerPrefs.DeleteKey(TokenKey);
    }

    [Serializable] private class AuthRequest  { public string provider; public string token; }
    [Serializable] private class AuthResponse { public string jwt; }
}
