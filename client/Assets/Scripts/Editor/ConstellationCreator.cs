using UnityEngine;
using UnityEditor;
using System.IO;

// Unity 메뉴: Tools → Starry → Create All Constellations
public static class ConstellationCreator
{
    private struct ConstellationDef
    {
        public string    name;
        public string    koreanName;
        public int       startMonth, startDay, endMonth, endDay;
        public Vector3[] stars;
        public int[]     lines;
    }

    [MenuItem("Tools/Starry/Create All Constellations")]
    private static void CreateAll()
    {
        string folder = "Assets/Resources/Constellations";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            Directory.CreateDirectory(Application.dataPath + "/Resources/Constellations");
            AssetDatabase.Refresh();
        }

        foreach (var def in Definitions)
        {
            string path  = $"{folder}/{def.name}.asset";

            // 이미 있으면 덮어쓰기
            var asset = AssetDatabase.LoadAssetAtPath<ConstellationData>(path)
                        ?? ScriptableObject.CreateInstance<ConstellationData>();

            asset.constellationName = def.name;
            asset.koreanName        = def.koreanName;
            asset.startMonth        = def.startMonth;
            asset.startDay          = def.startDay;
            asset.endMonth          = def.endMonth;
            asset.endDay            = def.endDay;
            asset.starPositions     = def.stars;
            asset.lineIndices       = def.lines;

            if (!File.Exists(Application.dataPath + "/Resources/Constellations/" + def.name + ".asset"))
                AssetDatabase.CreateAsset(asset, path);
            else
                EditorUtility.SetDirty(asset);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[ConstellationCreator] 12개 별자리 에셋 생성 완료 → " + folder);
    }

    // ── 12개 별자리 좌표 정의 ───────────────────────────────────────────────
    // starPositions: 별자리 중심 기준 로컬 좌표 (단위: 유니티 씬 단위)
    // lineIndices:   stars 배열 인덱스 쌍 (0-1 연결, 2-3 연결, ...)

    private static readonly ConstellationDef[] Definitions =
    {
        // ──────────────────────────────────────────────────────
        // 0. 양자리 Aries  3/21–4/19
        new()
        {
            name="Aries", koreanName="양자리",
            startMonth=3, startDay=21, endMonth=4, endDay=19,
            stars = new[]
            {
                new Vector3(-0.8f,  0.2f, 0),   // 0 Mesarthim
                new Vector3(-0.3f,  0.5f, 0),   // 1 Sheratan
                new Vector3( 0.4f,  0.7f, 0),   // 2 Hamal
                new Vector3( 1.2f,  0.3f, 0),   // 3 Botein
            },
            lines = new[] { 0,1, 1,2, 2,3 }
        },

        // ──────────────────────────────────────────────────────
        // 1. 황소자리 Taurus  4/20–5/20
        new()
        {
            name="Taurus", koreanName="황소자리",
            startMonth=4, startDay=20, endMonth=5, endDay=20,
            stars = new[]
            {
                new Vector3(-0.5f, -0.5f, 0),   // 0 Ain
                new Vector3( 0.0f, -0.2f, 0),   // 1 Aldebaran (중심)
                new Vector3( 0.5f, -0.5f, 0),   // 2 Theta
                new Vector3(-0.3f,  0.3f, 0),   // 3 뿔 1
                new Vector3( 0.3f,  0.3f, 0),   // 4 뿔 2
                new Vector3( 0.0f,  1.0f, 0),   // 5 Elnath (뿔 끝)
            },
            lines = new[] { 0,1, 1,2, 1,3, 1,4, 4,5 }
        },

        // ──────────────────────────────────────────────────────
        // 2. 쌍둥이자리 Gemini  5/21–6/21
        new()
        {
            name="Gemini", koreanName="쌍둥이자리",
            startMonth=5, startDay=21, endMonth=6, endDay=21,
            stars = new[]
            {
                new Vector3(-0.6f,  1.0f, 0),   // 0 Castor (머리)
                new Vector3(-0.6f,  0.4f, 0),   // 1 몸통
                new Vector3(-0.6f, -0.2f, 0),   // 2 다리 위
                new Vector3(-0.6f, -0.8f, 0),   // 3 발

                new Vector3( 0.6f,  1.0f, 0),   // 4 Pollux (머리)
                new Vector3( 0.6f,  0.4f, 0),   // 5 몸통
                new Vector3( 0.6f, -0.2f, 0),   // 6 다리 위
                new Vector3( 0.6f, -0.8f, 0),   // 7 발
            },
            lines = new[] { 0,1, 1,2, 2,3,  4,5, 5,6, 6,7,  1,5 }
        },

        // ──────────────────────────────────────────────────────
        // 3. 게자리 Cancer  6/22–7/22
        new()
        {
            name="Cancer", koreanName="게자리",
            startMonth=6, startDay=22, endMonth=7, endDay=22,
            stars = new[]
            {
                new Vector3( 0.0f,  0.8f, 0),   // 0 Acubens
                new Vector3(-0.5f,  0.0f, 0),   // 1 Asellus Borealis
                new Vector3( 0.0f,  0.0f, 0),   // 2 Praesepe (중심)
                new Vector3( 0.5f,  0.0f, 0),   // 3 Asellus Australis
                new Vector3( 0.0f, -0.8f, 0),   // 4 Al Tarf
            },
            lines = new[] { 0,2, 1,2, 2,3, 2,4 }
        },

        // ──────────────────────────────────────────────────────
        // 4. 사자자리 Leo  7/23–8/22
        new()
        {
            name="Leo", koreanName="사자자리",
            startMonth=7, startDay=23, endMonth=8, endDay=22,
            stars = new[]
            {
                new Vector3(-0.8f,  0.2f, 0),   // 0 Epsilon (꼬리)
                new Vector3( 0.0f,  0.0f, 0),   // 1 Regulus (몸통)
                new Vector3( 0.4f,  0.6f, 0),   // 2 Eta
                new Vector3( 0.8f,  0.9f, 0),   // 3 Gamma (갈기)
                new Vector3( 1.0f,  0.5f, 0),   // 4 Zeta
                new Vector3( 0.9f,  0.0f, 0),   // 5 Mu (머리 아래)
                new Vector3( 0.7f, -0.4f, 0),   // 6 Epsilon (앞발)
            },
            lines = new[] { 0,1, 1,2, 2,3, 3,4, 4,5, 5,6, 5,1 }
        },

        // ──────────────────────────────────────────────────────
        // 5. 처녀자리 Virgo  8/23–9/22
        new()
        {
            name="Virgo", koreanName="처녀자리",
            startMonth=8, startDay=23, endMonth=9, endDay=22,
            stars = new[]
            {
                new Vector3( 0.0f,  1.0f, 0),   // 0 Vindemiatrix
                new Vector3(-0.4f,  0.4f, 0),   // 1 왼쪽
                new Vector3( 0.0f,  0.0f, 0),   // 2 Spica (중심)
                new Vector3( 0.4f,  0.4f, 0),   // 3 오른쪽
                new Vector3(-0.7f, -0.3f, 0),   // 4 왼쪽 아래
                new Vector3( 0.7f, -0.3f, 0),   // 5 오른쪽 아래
                new Vector3( 0.0f, -0.8f, 0),   // 6 발
            },
            lines = new[] { 0,2, 1,2, 2,3, 1,4, 3,5, 2,6 }
        },

        // ──────────────────────────────────────────────────────
        // 6. 천칭자리 Libra  9/23–10/22
        new()
        {
            name="Libra", koreanName="천칭자리",
            startMonth=9, startDay=23, endMonth=10, endDay=22,
            stars = new[]
            {
                new Vector3( 0.0f,  0.8f, 0),   // 0 Zubenelgenubi (꼭대기)
                new Vector3(-0.7f, -0.2f, 0),   // 1 왼쪽 접시
                new Vector3( 0.7f, -0.2f, 0),   // 2 오른쪽 접시
                new Vector3( 0.0f, -0.7f, 0),   // 3 Brachium (받침)
            },
            lines = new[] { 0,1, 0,2, 1,3, 2,3 }
        },

        // ──────────────────────────────────────────────────────
        // 7. 전갈자리 Scorpius  10/23–11/21
        new()
        {
            name="Scorpius", koreanName="전갈자리",
            startMonth=10, startDay=23, endMonth=11, endDay=21,
            stars = new[]
            {
                new Vector3(-0.5f,  0.8f, 0),   // 0 머리
                new Vector3(-0.2f,  0.4f, 0),   // 1
                new Vector3( 0.0f,  0.0f, 0),   // 2 Antares (중심)
                new Vector3( 0.2f, -0.4f, 0),   // 3
                new Vector3( 0.4f, -0.8f, 0),   // 4
                new Vector3( 0.6f, -1.0f, 0),   // 5
                new Vector3( 0.5f, -1.3f, 0),   // 6 꼬리 끝
                new Vector3( 0.3f, -1.5f, 0),   // 7 침
            },
            lines = new[] { 0,1, 1,2, 2,3, 3,4, 4,5, 5,6, 6,7 }
        },

        // ──────────────────────────────────────────────────────
        // 8. 궁수자리 Sagittarius  11/22–12/21
        new()
        {
            name="Sagittarius", koreanName="궁수자리",
            startMonth=11, startDay=22, endMonth=12, endDay=21,
            stars = new[]
            {
                new Vector3(-0.7f,  0.0f, 0),   // 0 Kaus Borealis
                new Vector3(-0.4f, -0.5f, 0),   // 1 Kaus Media
                new Vector3( 0.0f, -0.7f, 0),   // 2 Kaus Australis
                new Vector3( 0.5f, -0.4f, 0),   // 3 Nunki
                new Vector3( 0.8f,  0.2f, 0),   // 4 주전자 주둥이
                new Vector3( 0.3f,  0.6f, 0),   // 5 뚜껑
                new Vector3(-0.3f,  0.5f, 0),   // 6
            },
            lines = new[] { 0,1, 1,2, 2,3, 3,4, 4,5, 5,6, 6,0, 0,5 }
        },

        // ──────────────────────────────────────────────────────
        // 9. 염소자리 Capricorn  12/22–1/19  (연도 걸침)
        new()
        {
            name="Capricorn", koreanName="염소자리",
            startMonth=12, startDay=22, endMonth=1, endDay=19,
            stars = new[]
            {
                new Vector3(-0.8f,  0.4f, 0),   // 0 Algedi
                new Vector3(-0.3f,  0.7f, 0),   // 1 Dabih
                new Vector3( 0.4f,  0.5f, 0),   // 2
                new Vector3( 0.8f,  0.0f, 0),   // 3 Nashira
                new Vector3( 0.3f, -0.6f, 0),   // 4 Deneb Algedi
                new Vector3(-0.4f, -0.4f, 0),   // 5
            },
            lines = new[] { 0,1, 1,2, 2,3, 3,4, 4,5, 5,0 }
        },

        // ──────────────────────────────────────────────────────
        // 10. 물병자리 Aquarius  1/20–2/18
        new()
        {
            name="Aquarius", koreanName="물병자리",
            startMonth=1, startDay=20, endMonth=2, endDay=18,
            stars = new[]
            {
                new Vector3( 0.0f,  0.8f, 0),   // 0 Sadalsuud (항아리)
                new Vector3(-0.3f,  0.2f, 0),   // 1 Sadalmelik
                new Vector3( 0.3f,  0.2f, 0),   // 2
                new Vector3(-0.6f, -0.3f, 0),   // 3 물줄기 왼
                new Vector3( 0.0f, -0.3f, 0),   // 4
                new Vector3( 0.6f, -0.3f, 0),   // 5 물줄기 오른
                new Vector3( 0.0f, -0.9f, 0),   // 6 끝
            },
            lines = new[] { 0,1, 0,2, 1,3, 2,5, 3,4, 4,5, 4,6 }
        },

        // ──────────────────────────────────────────────────────
        // 11. 물고기자리 Pisces  2/19–3/20
        new()
        {
            name="Pisces", koreanName="물고기자리",
            startMonth=2, startDay=19, endMonth=3, endDay=20,
            stars = new[]
            {
                new Vector3(-1.0f,  0.5f, 0),   // 0 물고기 1 머리
                new Vector3(-0.7f,  0.0f, 0),   // 1
                new Vector3(-0.5f, -0.5f, 0),   // 2 꼬리 연결
                new Vector3( 0.5f, -0.5f, 0),   // 3 꼬리 연결
                new Vector3( 0.7f,  0.0f, 0),   // 4
                new Vector3( 1.0f,  0.5f, 0),   // 5 물고기 2 머리
            },
            lines = new[] { 0,1, 1,2, 2,3, 3,4, 4,5 }
        },
    };
}
