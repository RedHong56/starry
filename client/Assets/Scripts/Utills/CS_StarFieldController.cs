using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class StarFieldController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject      starPrefab;
    [SerializeField] private ConstellationData[] allConstellations;

    [Header("Line")]
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Color    lineColor = new Color(1f, 0.9f, 0.5f, 1f);

    [Header("Tuning")]
    [SerializeField] private float starScale        = 0.1f;
    [SerializeField] private float fieldRadius      = 20f;
    [SerializeField] private int   ambientStarCount = 200;

    private struct StarInstance
    {
        public Renderer renderer;
        public string   belongsTo; // null = 배경 별
    }

    private readonly List<StarInstance>                     _stars = new();
    private readonly List<(LineRenderer lr, string name)>  _lines = new();
    private readonly MaterialPropertyBlock                  _propBlock = new();

    private void Start()
    {
        SpawnAmbientStars();
        SpawnConstellationStars();
    }

    // ── 스폰 ─────────────────────────────────────────────────────────────────

    private void SpawnAmbientStars()
    {
        for (int i = 0; i < ambientStarCount; i++)
        {
            Vector3 pos = UnityEngine.Random.onUnitSphere * fieldRadius;
            SpawnStar(pos, null,
                      UnityEngine.Random.Range(1.5f, 3f),
                      UnityEngine.Random.Range(0f, 6.28f));
        }
    }

    private void SpawnConstellationStars()
    {
        foreach (var data in allConstellations)
        {
            Vector3    anchor = ConstellationAnchor(data);
            Quaternion rot    = Quaternion.LookRotation(anchor.normalized);

            foreach (var localPos in data.starPositions)
                SpawnStar(anchor + rot * localPos, data.constellationName,
                          UnityEngine.Random.Range(1f, 2f),
                          UnityEngine.Random.Range(0f, 6.28f));

            SpawnLines(data, anchor, rot);
        }
    }

    private void SpawnStar(Vector3 pos, string belongsTo, float speed, float phase)
    {
        var go = Instantiate(starPrefab, pos, Quaternion.identity, transform);
        go.transform.localScale = Vector3.one * starScale;

        var r = go.GetComponent<Renderer>();
        r.GetPropertyBlock(_propBlock);
        _propBlock.SetFloat("_TwinkleSpeed", speed);
        _propBlock.SetFloat("_TwinklePhase", phase);
        _propBlock.SetFloat("_Highlight",    0f);
        r.SetPropertyBlock(_propBlock);

        _stars.Add(new StarInstance { renderer = r, belongsTo = belongsTo });
    }

    private void SpawnLines(ConstellationData data, Vector3 anchor, Quaternion rot)
    {
        for (int i = 0; i < data.lineIndices.Length; i += 2)
        {
            var go = new GameObject($"Line_{data.constellationName}_{i / 2}");
            go.transform.SetParent(transform);

            var lr = go.AddComponent<LineRenderer>();
            lr.material          = new Material(lineMaterial);
            lr.startWidth        = lr.endWidth = 0.02f;
            lr.positionCount     = 2;
            lr.SetPosition(0, anchor + rot * data.starPositions[data.lineIndices[i]]);
            lr.SetPosition(1, anchor + rot * data.starPositions[data.lineIndices[i + 1]]);

            Color c = lineColor; c.a = 0f;
            lr.startColor = lr.endColor = c;

            _lines.Add((lr, data.constellationName));
        }
    }

    // ── 공개 API ──────────────────────────────────────────────────────────────

    /// 모든 별 일반 반짝임 (강조 없음)
    public void EnterAmbientMode()
    {
        foreach (var star in _stars)
            TweenHighlight(star.renderer, 0f, 1.5f);

        foreach (var (lr, _) in _lines)
            TweenLineAlpha(lr, 0f, 1.5f);
    }

    /// 생일 기준으로 해당 별자리를 강조하고 ConstellationData를 반환
    public ConstellationData HighlightConstellation(int month, int day)
    {
        ConstellationData target     = FindByDate(month, day);
        string            targetName = target?.constellationName;

        foreach (var star in _stars)
        {
            float hl = (!string.IsNullOrEmpty(star.belongsTo) && star.belongsTo == targetName) ? 1f : 0f;
            TweenHighlight(star.renderer, hl, 2f);
        }

        foreach (var (lr, name) in _lines)
            TweenLineAlpha(lr, name == targetName ? 1f : 0f, 2f);

        return target;
    }

    // ── 내부 유틸 ─────────────────────────────────────────────────────────────

    private void TweenHighlight(Renderer r, float target, float duration)
    {
        // 별마다 독립적인 block 인스턴스를 캡처해 트윈 간 간섭 방지
        var block = new MaterialPropertyBlock();
        r.GetPropertyBlock(block);
        float current = block.GetFloat("_Highlight");

        DOTween.To(
            () => current,
            v =>
            {
                current = v;
                r.GetPropertyBlock(block);
                block.SetFloat("_Highlight", v);
                r.SetPropertyBlock(block);
            },
            target, duration
        );
    }

    private void TweenLineAlpha(LineRenderer lr, float targetAlpha, float duration)
    {
        Color baseColor = lineColor;
        DOTween.To(
            () => lr.startColor.a,
            a =>
            {
                Color c = baseColor; c.a = a;
                lr.startColor = lr.endColor = c;
            },
            targetAlpha, duration
        );
    }

    private Vector3 ConstellationAnchor(ConstellationData c)
    {
        // 양자리(3월)부터 30도씩 시계방향 배치
        float angle = ((c.startMonth - 3 + 12) % 12) * 30f * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(angle), 0.3f, Mathf.Cos(angle)).normalized * fieldRadius;
    }

    private ConstellationData FindByDate(int month, int day)
    {
        int dateNum = month * 100 + day;
        foreach (var c in allConstellations)
        {
            int  start   = c.startMonth * 100 + c.startDay;
            int  end     = c.endMonth   * 100 + c.endDay;
            bool inRange = start > end                          // 염소자리처럼 연도를 걸치는 경우
                ? dateNum >= start || dateNum <= end
                : dateNum >= start && dateNum <= end;
            if (inRange) return c;
        }
        return null;
    }
}
