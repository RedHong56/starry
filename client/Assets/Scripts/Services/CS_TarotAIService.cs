using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class TarotAIService : MonoBehaviour
{
    [SerializeField] private string apiUrl          = "https://your-backend.com/api/tarot/reading";
    [SerializeField] private string horoscopeApiUrl = "https://your-backend.com/api/horoscope";

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
        var body = JsonUtility.ToJson(new TarotRequest { cardIds = cardIds, worry = worry });

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
            onComplete?.Invoke("별의 언어를 읽는 데 문제가 생겼다. 다시 시도해보게.");
        }
    }

    private IEnumerator HoroscopeRoutine(string constellationName, Action<string> onComplete)
    {
        var body = JsonUtility.ToJson(new HoroscopeRequest { constellation = constellationName });

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
            onComplete?.Invoke("오늘의 별자리 운세를 불러오지 못했습니다.");
        }
    }

    [Serializable] private class TarotRequest     { public int[] cardIds; public string worry; }
    [Serializable] private class HoroscopeRequest { public string constellation; }
    [Serializable] private class TextResponse     { public string result; }
}
