using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private GameObject      dialoguePanel;
    [SerializeField] private TypewriterEffect typewriter;

    [Header("Input Modal")]
    [SerializeField] private GameObject    inputModal;
    [SerializeField] private TMP_InputField worryInput;
    
    [SerializeField] private TMP_Dropdown monthInput;
    [SerializeField] private TMP_Dropdown dayInput;
    [SerializeField] private Button         submitButton;

    
    public int BirthMonth { get; private set; }
    public int BirthDay   { get; private set; }

    private Action<string>   _onSubmit;

    private void Awake()
    {
        dialoguePanel.SetActive(false);
        inputModal.SetActive(false);
        submitButton.onClick.AddListener(OnSubmitClicked);
    }

    public void ShowDialogue(string text, Action onComplete)
    {
        dialoguePanel.SetActive(true);
        typewriter.Play(text, onComplete);
    }

    public void HideDialogue()
    {
        typewriter.Stop();
        dialoguePanel.SetActive(false);
    }

    // 고민 입력 모달. 제출 시 onSubmit(입력값) 호출
    public void ShowInputModal(Action<string> onSubmit)
    {
        _onSubmit = onSubmit;
        worryInput.text = string.Empty;
        inputModal.SetActive(true);
        worryInput.Select();
    }

    private void OnSubmitClicked()
    {
        string text = worryInput.text.Trim();
        if (string.IsNullOrEmpty(text)) return;

        
        BirthMonth = monthInput.value + 1;
        BirthDay   = dayInput.value + 1;

        inputModal.SetActive(false);
        _onSubmit?.Invoke(text);
        _onSubmit = null;
    }
}