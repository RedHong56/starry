using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SettingsController : MonoBehaviour
{
    [Header("BGM")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Toggle bgmToggle;

    [Header("SFX")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle sfxToggle;

    [Header("언어 세그먼트")]
    [SerializeField] private Button korButton;
    [SerializeField] private Button engButton;
    [SerializeField] private Color  selectedColor   = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color  unselectedColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    [Header("Button")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button logoutButton;

    [SerializeField] private GameObject settingsPanel;

    private void Awake()
    {
        settingsPanel.SetActive(false);

        closeButton.onClick.AddListener(Close);
        if (logoutButton != null) logoutButton.onClick.AddListener(OnLogoutClicked);

        bgmSlider.onValueChanged.AddListener(v => SoundManager.Instance?.SetBgmVolume(v));
        sfxSlider.onValueChanged.AddListener(v => SoundManager.Instance?.SetSfxVolume(v));

        bgmToggle.onValueChanged.AddListener(isOn => SoundManager.Instance?.SetBgmMute(!isOn));
        sfxToggle.onValueChanged.AddListener(isOn => SoundManager.Instance?.SetSfxMute(!isOn));

        if (korButton != null) korButton.onClick.AddListener(() => SelectLanguage(AppLanguage.Korean));
        if (engButton != null) engButton.onClick.AddListener(() => SelectLanguage(AppLanguage.English));
    }

    private void OnEnable()
    {
        bgmSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("bgmVolume", 1f));
        sfxSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("sfxVolume", 1f));
        bgmToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("bgmMute", 0) == 0);
        sfxToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("sfxMute", 0) == 0);

        RefreshLangButtons(LocalizationManager.Language);
    }

    private void SelectLanguage(AppLanguage lang)
    {
        SoundManager.Instance?.PlayBtn();
        LocalizationManager.SetLanguage(lang);
        RefreshLangButtons(lang);
    }

    private void RefreshLangButtons(AppLanguage lang)
    {
        if (korButton == null || engButton == null) return;
        bool isKor = lang == AppLanguage.Korean;
        SetButtonSelected(korButton, isKor);
        SetButtonSelected(engButton, !isKor);
    }

    private void SetButtonSelected(Button btn, bool selected)
    {
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = selected ? selectedColor : unselectedColor;

        var tmp = btn.GetComponentInChildren<TMP_Text>();
        if (tmp != null) tmp.color = selected ? selectedColor : unselectedColor;
    }

    public void Open()
    {
        SoundManager.Instance?.PlayBtn();
        settingsPanel.SetActive(true);
    }

    private void Close()
    {
        SoundManager.Instance?.PlayBtn();
        settingsPanel.SetActive(false);
    }

    private void OnLogoutClicked()
    {
        SoundManager.Instance?.PlayBtn();
        AuthManager.Instance.Logout();
        SceneManager.LoadScene("02_Login");
    }
}
