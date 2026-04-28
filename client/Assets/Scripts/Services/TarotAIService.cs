using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class TarotAIService : MonoBehaviour
{
    [SerializeField] private string apiUrl = "https://your-backend.com/api/tarot/reading";

    public void GetTarotReading(int[] cardIds, string worry, Action<string> onComplete)
    {
        StartCoroutine(PostRoutine(cardIds, worry, onComplete));
    }

    private IEnumerator PostRoutine(int[] cardIds, string worry, Action<string> onComplete)
    {
        var body = JsonUtility.ToJson(new RequestBody { cardIds = cardIds, worry = worry });

        UnityWebRequest req = new UnityWebRequest(apiUrl, "POST")
        {
            uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)),
            downloadHandler = new DownloadHandlerBuffer()
        };
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string result = JsonUtility.FromJson<ResponseBody>(req.downloadHandler.text).result;
            onComplete?.Invoke(result);
        }
        else
        {
            Debug.LogError($"[TarotAIService] {req.error}");
            onComplete?.Invoke("별의 언어를 읽는 데 문제가 생겼다. 다시 시도해보게.");
        }

        req.Dispose();
    }

    [Serializable] private class RequestBody  { public int[] cardIds; public string worry; }
    [Serializable] private class ResponseBody { public string result; }
}
