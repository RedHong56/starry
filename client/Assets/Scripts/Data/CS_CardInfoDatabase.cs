using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// cards_info.json 파싱 및 카드 정보 조회.
/// Resources/cards_info 에 TextAsset 배치 필요.
/// </summary>
public static class CardInfoDatabase
{
    private static Dictionary<int, CardInfo> _cards;
    private static bool _loaded;

    // ── 공개 API ──────────────────────────────────────────────────────────────

    /// <summary>id 로 카드 정보 조회. 없으면 null.</summary>
    public static CardInfo Get(int id)
    {
        EnsureLoaded();
        return _cards.TryGetValue(id, out var info) ? info : null;
    }

    // ── 로드 ──────────────────────────────────────────────────────────────────

    private static void EnsureLoaded()
    {
        if (_loaded) return;

        var asset = Resources.Load<TextAsset>("cards_info");
        if (asset == null)
        {
            Debug.LogError("[CardInfoDatabase] Resources/cards_info.json 을 찾을 수 없습니다.");
            _cards  = new Dictionary<int, CardInfo>();
            _loaded = true;
            return;
        }

        var root = JsonUtility.FromJson<CardInfoRoot>(asset.text);
        _cards = new Dictionary<int, CardInfo>(root.cards.Length);
        foreach (var c in root.cards)
            _cards[c.id] = c;

        _loaded = true;
    }

    // ── 표시용 유틸 ───────────────────────────────────────────────────────────

    private static readonly string[] RomanNumerals =
    {
        "0", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX",
        "X", "XI", "XII", "XIII", "XIV", "XV", "XVI", "XVII", "XVIII", "XIX",
        "XX", "XXI"
    };

    private static readonly Dictionary<string, string> SuitDisplayMap = new()
    {
        { "cup",      "Cup" },
        { "wand",     "Wand" },
        { "sword",    "Sword" },
        { "pentacle", "Pentacle" }
    };

    /// <summary>rank 문자열을 로마숫자 또는 대문자로 변환.</summary>
    public static string RankToDisplay(string rank)
    {
        if (int.TryParse(rank, out int num) && num >= 0 && num < RomanNumerals.Length)
            return RomanNumerals[num];

        // ace, page, knight, queen, king
        return rank switch
        {
            "ace"    => "Ace",
            "page"   => "Page",
            "knight" => "Knight",
            "queen"  => "Queen",
            "king"   => "King",
            _        => rank
        };
    }

    /// <summary>suit 문자열을 표시용 이름으로 변환.</summary>
    public static string SuitToDisplay(string suit)
    {
        if (suit == null) return string.Empty;
        return SuitDisplayMap.TryGetValue(suit, out var d) ? d : suit;
    }

    /// <summary>
    /// 카드 이름 포맷팅.
    /// Major → "0 The Fool"  (rank + name)
    /// Minor → "Wand II"     (suit + roman rank)
    /// </summary>
    public static string FormatCardName(CardInfo card)
    {
        if (card == null) return "Unknown";

        if (card.arcana == "major")
        {
            string displayRank = RankToDisplay(card.rank);
            return $"{displayRank} {card.name}";
        }
        else
        {
            string suit = SuitToDisplay(card.suit);
            string rank = RankToDisplay(card.rank);
            return $"{suit} {rank}";
        }
    }

    /// <summary>"Major" 또는 "Minor" 반환.</summary>
    public static string FormatArcana(CardInfo card)
    {
        if (card == null) return "Unknown";
        return card.arcana == "major" ? "Major" : "Minor";
    }

    /// <summary>
    /// Resources.Load 용 경로 반환.
    /// 파일명 패턴: "Cards/{id}_{name_snake_case}" (확장자 제외)
    /// 예: "Cards/0_the_fool", "Cards/22_ace_of_cups"
    /// </summary>
    public static string GetResourcePath(CardInfo card)
    {
        if (card == null) return null;
        string snakeName = card.name.ToLower().Replace(" ", "_");
        return $"Cards/{card.id}_{snakeName}";
    }
}

// ── JSON 매핑 클래스 ──────────────────────────────────────────────────────────

[Serializable]
public class CardInfoRoot
{
    public int version;
    public string description;
    public CardInfo[] cards;
}

[Serializable]
public class CardInfo
{
    public int    id;
    public string name;
    public string displayName;
    public string arcana;
    public string suit;
    public string rank;
    public CardKeywords keywords;
    public CardMeaning  meaning;
}

[Serializable]
public class CardKeywords
{
    public string[] upright;
    public string[] reversed;
}

[Serializable]
public class CardMeaning
{
    public string upright;
    public string reversed;
}
