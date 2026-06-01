using TMPro;
using UnityEngine;

public class CS_Worry_limit : MonoBehaviour
{
    [Header("적는 글자")]
    [SerializeField] private TMP_InputField WriteInput;
    [Header("남은 글자")]
    [SerializeField] private TMP_Text CountInput;
    [Header("개인정보 안내")]
    [SerializeField] private TMP_Text privacyNoteText;

    private readonly int maxCount = 150;

    private void Start()
    {
        WriteInput.characterLimit = maxCount;
        WriteInput.onValueChanged.AddListener(UpdateCount);
        UpdateCount(WriteInput.text);
        RefreshPrivacyNote();
        LocalizationManager.OnLanguageChanged += RefreshPrivacyNote;
    }

    private void OnDestroy()
    {
        LocalizationManager.OnLanguageChanged -= RefreshPrivacyNote;
    }

    private void UpdateCount(string currentText)
    {
        CountInput.text = $"({currentText.Length}/{maxCount})";
    }

    private void RefreshPrivacyNote()
    {
        if (privacyNoteText != null)
            privacyNoteText.text = LocalizationManager.WorryPrivacyNote;
    }
}