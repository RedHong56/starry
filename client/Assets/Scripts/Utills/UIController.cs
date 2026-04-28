using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private GameObject  dialoguePanel;
    [SerializeField] private TMP_Text    dialogueText;
    [SerializeField] private float       typewriterSpeed = 0.05f;
    [SerializeField] private float       postTypePause   = 1.2f;

    [Header("Input Modal")]
    [SerializeField] private GameObject    inputModal;
    [SerializeField] private TMP_InputField worryInput;
    [SerializeField] private Button         submitButton;

    private Coroutine        _typewriterCoroutine;
    private Action<string>   _onSubmit;

    private void Awake()
    {
        dialoguePanel.SetActive(false);
        inputModal.SetActive(false);
        submitButton.onClick.AddListener(OnSubmitClicked);
    }

    // 대사 표시 + 타이프라이터 효과. onComplete는 일시정지 후 호출(null 허용)
    public void ShowDialogue(string text, Action onComplete)
    {
        dialoguePanel.SetActive(true);

        if (_typewriterCoroutine != null)
            StopCoroutine(_typewriterCoroutine);

        _typewriterCoroutine = StartCoroutine(TypewriterRoutine(text, onComplete));
    }

    public void HideDialogue()
    {
        if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
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

        inputModal.SetActive(false);
        _onSubmit?.Invoke(text);
        _onSubmit = null;
    }

    private IEnumerator TypewriterRoutine(string text, Action onComplete)
    {
        dialogueText.text = string.Empty;
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        if (onComplete != null)
        {
            yield return new WaitForSeconds(postTypePause);
            onComplete.Invoke();
        }
    }
}
