using TMPro;
using UnityEngine;

public class CS_Worry_limit : MonoBehaviour
{
    [Header("적는 글자")]
    [SerializeField] private TMP_InputField WriteInput;
    [Header("남은 글자")]
    [SerializeField] private TMP_Text CountInput;

    private readonly int maxCount = 150;

    private void Start()
    {
        // 1. 최대 글자 수 150자 제한 설정
        WriteInput.characterLimit = maxCount;

        // 2. 텍스트가 변경될 때마다 UpdateCount 함수 실행
        WriteInput.onValueChanged.AddListener(UpdateCount);

        // 3. 시작할 때 초기 텍스트 반영
        UpdateCount(WriteInput.text);
    }

    private void UpdateCount(string currentText)
    {
        // (현재글자수/150) 형태로 표시
        CountInput.text = $"({currentText.Length}/{maxCount})";
    }
}