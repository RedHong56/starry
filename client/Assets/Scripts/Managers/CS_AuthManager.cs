using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    [SerializeField] private string userApiUrl = "http://127.0.0.1:8000/api/user/me";

    private const string TokenKey = "jwt_token";

    public string Token      => PlayerPrefs.GetString(TokenKey, string.Empty);
    public bool   IsLoggedIn => !string.IsNullOrEmpty(Token);

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 딥링크에서 받은 JWT를 직접 저장
    public void StoreToken(string jwt)
    {
        PlayerPrefs.SetString(TokenKey, jwt);
    }

    // 저장된 토큰이 서버에서 유효한지 확인
    public IEnumerator ValidateTokenRoutine(Action<bool> onResult)
    {
        if (!IsLoggedIn) { onResult?.Invoke(false); yield break; }

        using var req = UnityWebRequest.Get(userApiUrl);
        req.SetRequestHeader("Authorization", $"Bearer {Token}");
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            onResult?.Invoke(true);
        else
        {
            Logout();
            onResult?.Invoke(false);
        }
    }

    // 구글/애플 액세스 토큰 방식
    public IEnumerator AuthenticateRoutine(string provider, string socialToken, Action<bool> onResult)
    {
        var body = JsonUtility.ToJson(new AuthTokenRequest { token = socialToken });

        using var req = new UnityWebRequest($"http://127.0.0.1:8000/api/auth/{provider}", "POST")
        {
            uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)),
            downloadHandler = new DownloadHandlerBuffer()
        };
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var res = JsonUtility.FromJson<AuthResponse>(req.downloadHandler.text);
            StoreToken(res.jwt);
            onResult?.Invoke(true);
        }
        else
        {
            Debug.LogWarning($"[AuthManager] auth failed: {req.error}");
            onResult?.Invoke(false);
        }
    }

    public void Logout()
    {
        PlayerPrefs.DeleteKey(TokenKey);
    }

    [Serializable] private class AuthTokenRequest { public string token; }
    [Serializable] private class AuthResponse     { public string jwt; }
}
