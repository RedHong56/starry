using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class StarFieldController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject        starPrefab;
    [SerializeField] private ConstellationData[] allConstellations;

    [Header("Line")]
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Color    lineColor = new Color(1f, 0.9f, 0.5f, 1f);

    [Header("Ambient Stars")]
    [SerializeField] private float fieldRadius      = 20f;
    [SerializeField] private int   ambientStarCount = 200;
    [SerializeField] private float starScale        = 0.1f;

    [Header("Constellation Display")]
    [SerializeField] private float displayScale     = 20f;   // 별자리 별 간격 배율
    [SerializeField] private float starAppearDelay  = 0.12f; // 별 하나씩 등장 간격(초)
    [SerializeField] private float starAppearDur    = 0.4f;  // 별 하나 등장 시간
    [SerializeField] private float lineAppearDelay  = 0.15f; // 선 하나씩 등장 간격(초)
    [SerializeField] private float lineAppearDur    = 0.5f;  // 선 페이드인 시간

    // 배경별
    private readonly List<Renderer> _ambientStars = new();

    // 현재 표시 중인 별자리 오브젝트 (재사용을 위해 별도 보관)
    private readonly List<GameObject> _constellationObjects = new();
    private Coroutine                 _revealCoroutine;

    private MaterialPropertyBlock _propBlock;

    private void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        SpawnAmbientStars();
    }

    // ── 배경별 ────────────────────────────────────────────────────────────────

    private void SpawnAmbientStars()
    {
        for (int i = 0; i < ambientStarCount; i++)
        {
            Vector3 pos = Random.onUnitSphere * fieldRadius;
            var go = Instantiate(starPrefab, transform);
            go.transform.localPosition = pos;
            go.transform.localScale    = Vector3.one * starScale;

            var r = go.GetComponent<Renderer>();
            r.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat("_TwinkleSpeed", Random.Range(1.5f, 3f));
            _propBlock.SetFloat("_TwinklePhase", Random.Range(0f, 6.28f));
            _propBlock.SetFloat("_Highlight",    0f);
            r.SetPropertyBlock(_propBlock);

            _ambientStars.Add(r);
        }
    }

    // ── 공개 API ──────────────────────────────────────────────────────────────

    /// 생년월일로 별자리 이름만 조회 (시각 연출 없음)
    public string GetConstellationName(int month, int day)
    {
        var data = FindByDate(month, day);
        return data != null ? data.constellationName : string.Empty;
    }

    /// 별자리 결과 연출: 해당 별자리만 중앙에 별 → 선 순서로 등장
    public ConstellationData ShowConstellationResult(int month, int day)
    {
        ConstellationData data = FindByDate(month, day);
        if (data == null) return null;

        ClearConstellation();

        if (_revealCoroutine != null) StopCoroutine(_revealCoroutine);
        _revealCoroutine = StartCoroutine(RevealConstellation(data));

        return data;
    }

    /// 현재 표시 중인 별자리를 제거하고 배경별만 남김
    public void ClearConstellation()
    {
        if (_revealCoroutine != null)
        {
            StopCoroutine(_revealCoroutine);
            _revealCoroutine = null;
        }

        foreach (var go in _constellationObjects)
            if (go != null) Destroy(go);
        _constellationObjects.Clear();
    }

    // ── 연출 코루틴 ───────────────────────────────────────────────────────────

    private IEnumerator RevealConstellation(ConstellationData data)
    {
        // 1) 별 생성 (scale = 0, 화면 중앙 배치)
        var starObjects = new List<GameObject>();
        foreach (var localPos in data.starPositions)
        {
            var go = Instantiate(starPrefab, transform);
            go.transform.localPosition = localPos * displayScale;
            go.transform.localScale    = Vector3.zero; // 처음엔 숨김

            var r = go.GetComponent<Renderer>();
            r.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat("_TwinkleSpeed", Random.Range(1f, 2f));
            _propBlock.SetFloat("_TwinklePhase", Random.Range(0f, 6.28f));
            _propBlock.SetFloat("_Highlight",    1f);
            r.SetPropertyBlock(_propBlock);

            starObjects.Add(go);
            _constellationObjects.Add(go);
        }

        // 2) 별 하나씩 등장
        foreach (var go in starObjects)
        {
            go.transform.DOScale(Vector3.one * starScale * 1.4f, starAppearDur)
              .SetEase(Ease.OutBack);
            yield return new WaitForSeconds(starAppearDelay);
        }

        // 별이 모두 나타날 때까지 대기
        yield return new WaitForSeconds(starAppearDur);

        // 3) 선 생성 후 하나씩 페이드인
        var lineRenderers = new List<LineRenderer>();
        for (int i = 0; data.lineIndices.Length > i + 1; i += 2)
        {
            int  idxA = data.lineIndices[i];
            int  idxB = data.lineIndices[i + 1];

            var go = new GameObject($"Line_{data.constellationName}_{i / 2}");
            go.transform.SetParent(transform, false);

            var lr = go.AddComponent<LineRenderer>();
            lr.material      = new Material(lineMaterial);
            lr.useWorldSpace = false;
            lr.startWidth    = lr.endWidth = 0.02f;
            lr.positionCount = 2;
            lr.SetPosition(0, data.starPositions[idxA] * displayScale);
            lr.SetPosition(1, data.starPositions[idxB] * displayScale);

            Color c = lineColor; c.a = 0f;
            lr.startColor = lr.endColor = c;

            lineRenderers.Add(lr);
            _constellationObjects.Add(go);
        }

        foreach (var lr in lineRenderers)
        {
            TweenLineAlpha(lr, 1f, lineAppearDur);
            yield return new WaitForSeconds(lineAppearDelay);
        }

        _revealCoroutine = null;
    }

    // ── 내부 유틸 ─────────────────────────────────────────────────────────────

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

    private ConstellationData FindByDate(int month, int day)
    {
        int dateNum = month * 100 + day;
        foreach (var c in allConstellations)
        {
            int  start   = c.startMonth * 100 + c.startDay;
            int  end     = c.endMonth   * 100 + c.endDay;
            bool inRange = start > end
                ? dateNum >= start || dateNum <= end
                : dateNum >= start && dateNum <= end;
            if (inRange) return c;
        }
        return null;
    }
}
