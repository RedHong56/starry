using UnityEngine;

[CreateAssetMenu(fileName = "Constellation", menuName = "Starry/Constellation")]
public class ConstellationData : ScriptableObject
{
    public string constellationName;
    public string koreanName;

    [Header("날짜 범위 (월-일)")]
    public int startMonth;
    public int startDay;
    public int endMonth;
    public int endDay;

    [Header("별 좌표 (로컬 좌표)")]
    public Vector3[] starPositions;

    [Header("선 인덱스 쌍 (짝수 길이)")]
    public int[] lineIndices;
}
