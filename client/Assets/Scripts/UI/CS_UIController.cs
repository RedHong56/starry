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
    [SerializeField] private TMP_Dropdown  monthInput;
    [SerializeField] private TMP_Dropdown  dayInput;
    [SerializeField] private Button        submitButton;

    [Header("별자리 이동 버튼 (Result 패널 위)")]
    [SerializeField] private Button viewConstellationButton;

    [Header("별자리 패널 (양피지)")]
    [SerializeField] private GameObject constellationPanel;
    [SerializeField] private TMP_Text   constellationNameText;
    [SerializeField] private TMP_Text   constellationDescText;
    [SerializeField] private Button     restartButton;

    public int BirthMonth { get; private set; }
    public int BirthDay   { get; private set; }

    private Action<string> _onSubmit;

    private void Awake()
    {
        dialoguePanel.SetActive(false);
        inputModal.SetActive(false);
        viewConstellationButton.gameObject.SetActive(false);
        constellationPanel.SetActive(false);
        submitButton.onClick.AddListener(OnSubmitClicked);
    }

    // ── 대화 ─────────────────────────────────────────────────────────────────

    public void ShowDialogue(string text, Action onComplete)
    {
        SoundManager.Instance?.PlayPannel();
        dialoguePanel.SetActive(true);
        typewriter.Play(text, onComplete);
    }

    public void HideDialogue()
    {
        typewriter.Stop();
        dialoguePanel.SetActive(false);
    }

    // ── 고민 입력 모달 ────────────────────────────────────────────────────────

    public void ShowInputModal(Action<string> onSubmit)
    {
        SoundManager.Instance?.PlayPannel();
        _onSubmit = onSubmit;
        worryInput.text = string.Empty;
        inputModal.SetActive(true);
        worryInput.Select();
    }

    private void OnSubmitClicked()
    {
        string text = worryInput.text.Trim();
        if (string.IsNullOrEmpty(text)) return;
        SoundManager.Instance?.PlayBtn();

        BirthMonth = monthInput.value + 1;
        BirthDay   = dayInput.value + 1;

        inputModal.SetActive(false);
        _onSubmit?.Invoke(text);
        _onSubmit = null;
    }

    // ── 별자리 이동 버튼 ──────────────────────────────────────────────────────

    /// Result 패널 위에 "별자리 확인" 버튼을 표시. 누르면 버튼이 사라지고 onClicked 호출.
    public void ShowViewConstellationButton(Action onClicked)
    {
        viewConstellationButton.gameObject.SetActive(true);
        viewConstellationButton.onClick.RemoveAllListeners();
        viewConstellationButton.onClick.AddListener(() =>
        {
            SoundManager.Instance?.PlayBtn();
            viewConstellationButton.gameObject.SetActive(false);
            onClicked?.Invoke();
        });
    }

    // ── 별자리 패널 (양피지) ──────────────────────────────────────────────────

    /// 별자리 이름을 양피지 패널에 표시. 설명은 로딩 중 placeholder를 먼저 보여줌.
    public void ShowConstellationPanel(string koreanName, Action onRestart)
    {
        constellationNameText.text = koreanName ?? string.Empty;
        constellationDescText.text = "오늘의 운세를 읽는 중...";

        restartButton.onClick.RemoveAllListeners();
        restartButton.onClick.AddListener(() =>
        {
            SoundManager.Instance?.PlayBtn();
            onRestart?.Invoke();
        });

        SoundManager.Instance?.PlayPannel();
        constellationPanel.SetActive(true);
    }

    /// 백엔드 응답이 오면 설명 텍스트를 교체.
    public void UpdateConstellationDesc(string description)
    {
        constellationDescText.text = description ?? string.Empty;
    }

    public void HideConstellationPanel()
    {
        constellationPanel.SetActive(false);
    }
}
