using System;
using UnityEngine;

public enum AppLanguage { Korean, English }

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }
    public static AppLanguage Language { get; private set; }
    public static event Action OnLanguageChanged;

    private const string PrefKey = "app_language";

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Language = (AppLanguage)PlayerPrefs.GetInt(PrefKey, 0);
    }

    public static void SetLanguage(AppLanguage lang)
    {
        if (Language == lang) return;
        Language = lang;
        PlayerPrefs.SetInt(PrefKey, (int)lang);
        OnLanguageChanged?.Invoke();
    }

    public static bool IsKorean => Language == AppLanguage.Korean;
    public static string Code   => IsKorean ? "ko" : "en";

    // ── 대화 텍스트 ───────────────────────────────────────────────────────────

    public static string Welcome      => IsKorean ? "어서 오게나..."       : "Welcome...";
    public static string AskWorry     => IsKorean ? "그래서 고민이 무엇이냐" : "What troubles you?";
    public static string Hmm          => IsKorean ? "흠…"                  : "Hmm...";
    public static string RevealResult => IsKorean ? "결과를 말해주겠다"     : "Let me reveal your fate";

    public static string[] PickDialogues => IsKorean
        ? new[] { "과거 카드를 고르게", "현재 카드를 고르게", "미래 카드를 고르게" }
        : new[] { "Choose the past card", "Choose the present card", "Choose the future card" };

    // ── UI 텍스트 ─────────────────────────────────────────────────────────────

    public static string LoadingHoroscope => IsKorean ? "오늘의 운세를 읽는 중..."        : "Reading your horoscope...";
    public static string TarotError       => IsKorean ? "별의 언어를 읽는 데 문제가 생겼다. 다시 시도해보게." : "The stars could not be read. Please try again.";
    public static string HoroscopeError   => IsKorean ? "오늘의 별자리 운세를 불러오지 못했습니다." : "Could not load today's horoscope.";
}
