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
        characterController.PlayBeckoning();
        EnterPhase(GamePhase.Welcome);
    }

    private void HandleWelcome()
    {
        cameraController.GoToSeat(() =>
        {
            SoundManager.Instance?.PlayDia(DiaType.Come);
            uiController.ShowDialogue("어서 오게나...", () => EnterPhase(GamePhase.Question));
        });
    }

    private void HandleQuestion()
    {
        SoundManager.Instance?.PlayDia(DiaType.Worry);
        uiController.ShowDialogue("그래서 고민이 무엇이냐", () =>
        {
            uiController.HideDialogue();
            uiController.ShowInputModal(worry =>
            {
                _userWorry = worry;
                EnterPhase(GamePhase.CardSelect);
            });
        });
    }

    private static readonly string[] PickDialogues = { "과거 카드를 고르게", "현재 카드를 고르게", "미래 카드를 고르게" };

    private void HandleCardSelect()
    {
        characterController.PlayWriting();
        cardDeck.gameObject.SetActive(false);

        SoundManager.Instance?.PlayDia(DiaType.Past);
        uiController.ShowDialogue(PickDialogues[0], () =>
        {
            uiController.HideDialogue();
            cardDeckController.StartSelection(
                onComplete: (indices, isReversed) =>
                {
                    _selectedCardIndices = indices;
                    _isReversed          = isReversed;
                    EnterPhase(GamePhase.Result);
                },
                onEachConfirm: confirmedIdx =>
                {
                    if (confirmedIdx + 1 < PickDialogues.Length)
                    {
                        DiaType dia = confirmedIdx == 0 ? DiaType.Present : DiaType.Future;
                        SoundManager.Instance?.PlayDia(dia);
                        uiController.ShowDialogue(PickDialogues[confirmedIdx + 1], () =>
                            uiController.HideDialogue());
                    }
                }
            );
        });
    }

    private void HandleResult()
    {
        cardDeck.gameObject.SetActive(true);
        characterController.PlayClapping();
        SoundManager.Instance?.PlayDia(DiaType.Umm);
        uiController.ShowDialogue("흠…", () =>
        {
            SoundManager.Instance?.PlayDia(DiaType.Result);
            uiController.ShowDialogue("결과를 말해주겠다", () =>
            {
                uiController.HideDialogue();
                cardResultController.StartReveal(
                    _selectedCardIndices, _isReversed, _userWorry,
                    beforeFlip: i => cardDeckController.HideSelectedCard(i),
                    onComplete: OnReadingComplete);
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
        var data = starField.HighlightConstellation(uiController.BirthMonth, uiController.BirthDay);
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
