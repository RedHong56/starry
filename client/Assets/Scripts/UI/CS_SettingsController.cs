using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [Header("BGM")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Toggle bgmToggle;

    [Header("SFX")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle sfxToggle;

    [Header("Button")]
    [SerializeField] private Button closeButton;

    [SerializeField] private GameObject settingsPanel;

    private void Awake()
    {
        settingsPanel.SetActive(false);

        closeButton.onClick.AddListener(Close);

        bgmSlider.onValueChanged.AddListener(v => SoundManager.Instance?.SetBgmVolume(v));
        sfxSlider.onValueChanged.AddListener(v => SoundManager.Instance?.SetSfxVolume(v));

        bgmToggle.onValueChanged.AddListener(isOn => SoundManager.Instance?.SetBgmMute(!isOn));
        sfxToggle.onValueChanged.AddListener(isOn => SoundManager.Instance?.SetSfxMute(!isOn));
    }

    private void OnEnable()
    {
        bgmSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("bgmVolume", 1f));
        sfxSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("sfxVolume", 1f));
        bgmToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("bgmMute", 0) == 0);
        sfxToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("sfxMute", 0) == 0);
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
}
