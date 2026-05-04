using System;
using System.Collections;
using UnityEngine;

public class AdRewardController : MonoBehaviour
{
    public static AdRewardController Instance { get; private set; }

    // TODO: Unity Ads 설정
    // [SerializeField] private string gameId = "your-unity-game-id";
    // [SerializeField] private string adUnitId = "Rewarded_Android";

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // TODO: Unity Ads 초기화
        // Advertisement.Initialize(gameId, testMode: false);
    }

    public void ShowRewardedAd(Action onSuccess, Action onFail)
    {
        // TODO: Unity Ads SDK 연결 후 아래 주석 해제
        // if (!Advertisement.IsReady(adUnitId)) { onFail?.Invoke(); return; }
        // var options = new ShowOptions { resultCallback = result =>
        // {
        //     if (result == ShowResult.Finished)
        //         StartCoroutine(RewardRoutine(onSuccess, onFail));
        //     else
        //         onFail?.Invoke();
        // }};
        // Advertisement.Show(adUnitId, options);

        // 임시: SDK 없을 때 바로 성공 처리
        StartCoroutine(RewardRoutine(onSuccess, onFail));
    }

    private IEnumerator RewardRoutine(Action onSuccess, Action onFail)
    {
        yield return UserDataManager.Instance.AdRewardRoutine(success =>
        {
            if (success) onSuccess?.Invoke();
            else         onFail?.Invoke();
        });
    }
}
