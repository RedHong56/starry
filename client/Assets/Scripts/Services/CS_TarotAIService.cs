using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class TarotAIService : MonoBehaviour
{
    private readonly string apiUrl          = AppSecrets.BackendBaseUrl + "/api/tarot/reading";
    private readonly string horoscopeApiUrl = AppSecrets.BackendBaseUrl + "/api/horoscope";

    // 타로 해설 요청
    public void GetTarotReading(int[] cardIds, string worry, Action<string> onComplete)
    {
        StartCoroutine(TarotRoutine(cardIds, worry, onComplete));
    }

    // 별자리 일일 운세 요청 (constellationName: 영문 이름, ex. "Aries")
    public void GetHoroscope(string constellationName, Action<string> onComplete)
    {
        StartCoroutine(HoroscopeRoutine(constellationName, onComplete));
    }

    private IEnumerator TarotRoutine(int[] cardIds, string worry, Action<string> onComplete)
    {
        var body = JsonUtility.ToJson(new TarotRequest { cardIds = cardIds, worry = worry, language = LocalizationManager.Code });

        using var req = new UnityWebRequest(apiUrl, "POST")
        {
            uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)),
            downloadHandler = new DownloadHandlerBuffer()
        };
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            onComplete?.Invoke(JsonUtility.FromJson<TextResponse>(req.downloadHandler.text).result);
        else
        {
            Debug.LogError($"[TarotAIService] {req.error}");
            onComplete?.Invoke(LocalizationManager.TarotError);
        }
    }

    private IEnumerator HoroscopeRoutine(string constellationName, Action<string> onComplete)
    {
        var body = JsonUtility.ToJson(new HoroscopeRequest { constellation = constellationName, language = LocalizationManager.Code });

        using var req = new UnityWebRequest(horoscopeApiUrl, "POST")
        {
            uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)),
            downloadHandler = new DownloadHandlerBuffer()
        };
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            onComplete?.Invoke(JsonUtility.FromJson<TextResponse>(req.downloadHandler.text).result);
        else
        {
            Debug.LogError($"[TarotAIService] horoscope {req.error}");
            onComplete?.Invoke(LocalizationManager.HoroscopeError);
        }
    }

    [Serializable] private class TarotRequest     { public int[] cardIds; public string worry; public string language; }
    [Serializable] private class HoroscopeRequest { public string constellation; public string language; }
    [Serializable] private class TextResponse     { public string result; }
}
