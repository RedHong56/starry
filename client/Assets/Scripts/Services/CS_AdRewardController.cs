using System;
using GoogleMobileAds.Api;
using UnityEngine;

public class AdRewardController : MonoBehaviour
{
    public static AdRewardController Instance { get; private set; }

    private const string AndroidAdUnitId = "ca-app-pub-5297356763431131/9112153007";
    private const string IosAdUnitId     = "ca-app-pub-3940256099942544/1712485313"; // TODO: iOS 단위 ID 교체

    // 테스트 광고 단위 (항상 광고 채워짐)
    private const string TestAndroidAdUnitId = "ca-app-pub-3940256099942544/5224354917";
    private const string TestIosAdUnitId     = "ca-app-pub-3940256099942544/1712485313";

    [SerializeField] private bool testMode = false;

    private RewardedAd _rewardedAd;
    private string     _adUnitId;
    private Action     _pendingOnFail;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

#if UNITY_IOS
        _adUnitId = testMode ? TestIosAdUnitId : IosAdUnitId;
#else
        _adUnitId = testMode ? TestAndroidAdUnitId : AndroidAdUnitId;
#endif

        MobileAds.Initialize(_ =>
        {
            Debug.Log("[AdRewardController] AdMob 초기화 완료");
            LoadAd();
        });
    }

    private void LoadAd()
    {
        _rewardedAd?.Destroy();
        RewardedAd.Load(_adUnitId, new AdRequest(), OnAdLoaded);
    }

    private void OnAdLoaded(RewardedAd ad, LoadAdError error)
    {
        if (error != null)
        {
            Debug.LogWarning($"[AdRewardController] 광고 로드 실패: {error.GetMessage()}. 5초 후 재시도.");
            Invoke(nameof(LoadAd), 5f);
            return;
        }

        _rewardedAd = ad;
        _rewardedAd.OnAdFullScreenContentFailed += e =>
        {
            Debug.LogWarning($"[AdRewardController] 광고 표시 실패: {e.GetMessage()}");
            var fail = _pendingOnFail;
            _pendingOnFail = null;
            fail?.Invoke();
            LoadAd();
        };
        _rewardedAd.OnAdFullScreenContentClosed += LoadAd;

        Debug.Log("[AdRewardController] 광고 로드 완료");
    }

    public void ShowRewardedAd(Action onSuccess, Action onFail)
    {
        if (_rewardedAd == null || !_rewardedAd.CanShowAd())
        {
            Debug.LogWarning("[AdRewardController] 광고 준비 안됨. 로드 재시도.");
            onFail?.Invoke();
            LoadAd();
            return;
        }

        _pendingOnFail = onFail;
        _rewardedAd.Show(_ =>
        {
            _pendingOnFail = null;
            onSuccess?.Invoke();
        });
    }
}
