using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GamePhase
{
    Intro,       // CAM_Begin → Cam_Walk (자동)
    Welcome,     // Cam_Walk → Cam_Seat, Beckoning, "어서 오게나..."
    Question,    // AskingQuestion, "그래서 고민이 무엇이냐", 입력 모달
    CardSelect,  // 카드 캐러셀, Writing, "흠…"
    Result       // Clapping, "결과를 말해주겠다", 카드 공개, AI 해설
}

// 전체 게임 흐름을 조율하는 FSM. 직접 UI/카메라/애니메이션을 건드리지 않고
// 각 전담 컨트롤러에 위임한다.
public class PhaseManager : MonoBehaviour
{
    public static PhaseManager Instance { get; private set; }

    [Header("Controllers")]
    [SerializeField] private CameraContorller         cameraController;
    [SerializeField] private TarotCharacterController characterController;
    [SerializeField] private UIController             uiController;
    [SerializeField] private CardDeckController       cardDeckController;
    [SerializeField] private CardResultController     cardResultController;
    [SerializeField] private StarFieldController      starField;
    [SerializeField] private TarotAIService           aiService;
    [Header("Buttons")]
    [SerializeField] private Button startButton;

    [Header("Object")]
    [SerializeField] private GameObject cardDeck;

    private GamePhase _currentPhase;
    private string    _userWorry;
    private int[]     _selectedCardIndices;
    private bool[]    _isReversed;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        startButton.onClick.AddListener(OnStartButtonClicked);
        EnterPhase(GamePhase.Intro);
    }

    private void EnterPhase(GamePhase phase)
    {
        _currentPhase = phase;
        switch (phase)
        {
            case GamePhase.Intro:      HandleIntro();      break;
            case GamePhase.Welcome:    HandleWelcome();    break;
            case GamePhase.Question:   HandleQuestion();   break;
            case GamePhase.CardSelect: HandleCardSelect(); break;
            case GamePhase.Result:     HandleResult();     break;
        }
    }

    // ── 단계별 처리 ────────────────────────────────────────────────────────────

    private void HandleIntro()
    {
        startButton.gameObject.SetActive(false);
        cameraController.GoToBegin();
        cameraController.GoToWalk(() => startButton.gameObject.SetActive(true));
    }

    private void OnStartButtonClicked()
    {
        if (_currentPhase != GamePhase.Intro) return;
        SoundManager.Instance?.PlayBtn();
        startButton.gameObject.SetActive(false);
        StartCoroutine(StartRoutine());
    }

    private IEnumerator StartRoutine()
    {
        var udm = UserDataManager.Instance;

        if (udm != null && udm.CanUseTarot())
        {
            yield return udm.ConsumeReadingRoutine(success =>
            {
                if (success) Proceed();
                else         RestoreStartButton();
            });
        }
        else
        {
            AdRewardController.Instance.ShowRewardedAd(
                onSuccess: () => StartCoroutine(AdProceedRoutine()),
                onFail:    RestoreStartButton
            );
        }
    }

    private IEnumerator AdProceedRoutine()
    {
        yield return UserDataManager.Instance.AdRewardRoutine(success =>
        {
            if (!success) { RestoreStartButton(); return; }
        });
        yield return UserDataManager.Instance.ConsumeReadingRoutine(success =>
        {
            if (success) Proceed();
            else         RestoreStartButton();
        });
    }

    private void Proceed()
    {
        characterController.PlayBeckoning();
        EnterPhase(GamePhase.Welcome);
    }

    private void RestoreStartButton()
    {
        startButton.gameObject.SetActive(true);
    }

    private void HandleWelcome()
    {
        cameraController.GoToSeat(() => { });
        SoundManager.Instance?.PlayDia(DiaType.Come);
        uiController.ShowDialogue(LocalizationManager.Welcome, () => EnterPhase(GamePhase.Question));
    }

    private void HandleQuestion()
    {
        SoundManager.Instance?.PlayDia(DiaType.Worry);
        uiController.ShowDialogue(LocalizationManager.AskWorry, () =>
        {
            uiController.HideDialogue();
            uiController.ShowInputModal(worry =>
            {
                _userWorry = worry;
                EnterPhase(GamePhase.CardSelect);
            });
        });
    }


    private void HandleCardSelect()
    {
        characterController.PlayWriting();
        cardDeck.gameObject.SetActive(false);

        var picks = LocalizationManager.PickDialogues;
        SoundManager.Instance?.PlayDia(DiaType.Past);
        uiController.ShowDialogue(picks[0], () =>
        {
            uiController.HideDialogue();
            cardDeckController.StartSelection(
                onComplete: (indices, isReversed) =>
                {
                    _selectedCardIndices = indices;
                    _isReversed = isReversed;
                    EnterPhase(GamePhase.Result);
                },
                onEachConfirm: confirmedIdx =>
                {
                    if (confirmedIdx + 1 < picks.Length)
                    {
                        DiaType dia = confirmedIdx == 0 ? DiaType.Present : DiaType.Future;
                        SoundManager.Instance?.PlayDia(dia);
                        uiController.ShowDialogue(picks[confirmedIdx + 1], () =>
                            uiController.HideDialogue());
                    }
                }
            );
        });
    }

    private void HandleResult()
    {
        // 흠… 제스처와 동시에 AI 요청 선발송 → 카드 공개 중 응답 대기
        bool   aiReady  = false;
        string aiResult = null;
        aiService.GetTarotReading(_selectedCardIndices, _userWorry, result =>
        {
            aiResult = result;
            aiReady  = true;
        });

        cardDeck.gameObject.SetActive(true);
        characterController.PlayClapping();
        SoundManager.Instance?.PlayDia(DiaType.Umm);
        uiController.ShowDialogue(LocalizationManager.Hmm, () =>
        {
            SoundManager.Instance?.PlayDia(DiaType.Result);
            uiController.ShowDialogue(LocalizationManager.RevealResult, () =>
            {
                uiController.HideDialogue();
                cardResultController.StartReveal(
                    _selectedCardIndices, _isReversed, _userWorry,
                    isAiReady:   () => aiReady,
                    getAiResult: () => aiResult,
                    beforeFlip:  i => cardDeckController.HideSelectedCard(i),
                    onComplete:  OnReadingComplete);
            });
        });
    }

    // AI 해설까지 완료 → "별자리 확인" 버튼 표시
    private void OnReadingComplete()
    {
        uiController.ShowViewConstellationButton(OnViewConstellationClicked);
    }

    // "별자리 확인" 버튼 눌림 → 카메라 하늘로 + 별자리 강조 (동시)
    private void OnViewConstellationClicked()
    {
        cardResultController.HideResultPanel();
        
        cardDeckController.HideSelectedCards();
        var data = starField.ShowConstellationResult(uiController.BirthMonth, uiController.BirthDay);
        string constellationName = data != null ? data.constellationName : string.Empty;
        string koreanName        = data != null ? data.koreanName        : string.Empty;

        // 카메라 이동 완료 후 패널 표시 + 운세 요청
        cameraController.GoToSky(() =>
        {
            uiController.ShowConstellationPanel(koreanName, onRestart: OnRestartClicked);
            aiService.GetHoroscope(constellationName, result =>
                uiController.UpdateConstellationDesc(result));
        });
    }

    // 재시작 버튼 → 씬 초기부터 다시
    private void OnRestartClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
