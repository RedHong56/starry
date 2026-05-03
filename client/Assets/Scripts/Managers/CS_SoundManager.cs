using UnityEngine;

public enum DiaType { Come, Worry, Sub, Past, Present, Future, Umm, Result }
public enum CardSFX { Animation, Select, Flip, Swipe }

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource diaSource;

    [Header("BGM")]
    [SerializeField] private AudioClip bgmClip;

    [Header("SFX")]
    [SerializeField] private AudioClip chairClip;
    [SerializeField] private AudioClip btnClip;
    [SerializeField] private AudioClip pannelClip;

    [Header("DIA")]
    [SerializeField] private AudioClip diaComeClip;
    [SerializeField] private AudioClip diaWorryClip;
    [SerializeField] private AudioClip diaSubClip;
    [SerializeField] private AudioClip diaPastClip;
    [SerializeField] private AudioClip diaPresentClip;
    [SerializeField] private AudioClip diaFutureClip;
    [SerializeField] private AudioClip diaUmmClip;
    [SerializeField] private AudioClip diaResultClip;

    [Header("Card")]
    [SerializeField] private AudioClip cardAnimationClip;
    [SerializeField] private AudioClip cardSelectClip;
    [SerializeField] private AudioClip cardFlipClip;
    [SerializeField] private AudioClip cardSwipeClip;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        bgmSource.clip = bgmClip;
        bgmSource.loop = true;
        bgmSource.Play();

        bgmSource.volume  = PlayerPrefs.GetFloat("bgmVolume", 1f);
        sfxSource.volume  = PlayerPrefs.GetFloat("sfxVolume", 1f);
        diaSource.volume  = PlayerPrefs.GetFloat("sfxVolume", 1f);
        bgmSource.mute    = PlayerPrefs.GetInt("bgmMute", 0) == 1;
        sfxSource.mute    = PlayerPrefs.GetInt("sfxMute", 0) == 1;
        diaSource.mute    = PlayerPrefs.GetInt("sfxMute", 0) == 1;
    }

    public void SetBgmVolume(float value)
    {
        bgmSource.volume = value;
        PlayerPrefs.SetFloat("bgmVolume", value);
    }

    public void SetBgmMute(bool mute)
    {
        bgmSource.mute = mute;
        PlayerPrefs.SetInt("bgmMute", mute ? 1 : 0);
    }

    public void SetSfxVolume(float value)
    {
        sfxSource.volume = value;
        diaSource.volume = value;
        PlayerPrefs.SetFloat("sfxVolume", value);
    }

    public void SetSfxMute(bool mute)
    {
        sfxSource.mute = mute;
        diaSource.mute = mute;
        PlayerPrefs.SetInt("sfxMute", mute ? 1 : 0);
    }

    public void PlayChair()  => sfxSource.PlayOneShot(chairClip);
    public void PlayBtn()    => sfxSource.PlayOneShot(btnClip);
    public void PlayPannel() => sfxSource.PlayOneShot(pannelClip);

    public void PlayDia(DiaType type)
    {
        AudioClip clip = type switch
        {
            DiaType.Come    => diaComeClip,
            DiaType.Worry   => diaWorryClip,
            DiaType.Sub     => diaSubClip,
            DiaType.Past    => diaPastClip,
            DiaType.Present => diaPresentClip,
            DiaType.Future  => diaFutureClip,
            DiaType.Umm     => diaUmmClip,
            DiaType.Result  => diaResultClip,
            _               => null
        };
        if (clip != null) diaSource.PlayOneShot(clip);
    }

    public void PlayCard(CardSFX sfx)
    {
        AudioClip clip = sfx switch
        {
            CardSFX.Animation => cardAnimationClip,
            CardSFX.Select    => cardSelectClip,
            CardSFX.Flip      => cardFlipClip,
            CardSFX.Swipe     => cardSwipeClip,
            _                 => null
        };
        if (clip != null) sfxSource.PlayOneShot(clip);
    }
}
