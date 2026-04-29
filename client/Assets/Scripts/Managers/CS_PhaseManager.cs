using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private CameraContorller        cameraController;
    [SerializeField] private TarotCharacterController characterController;
    [SerializeField] private UIController            uiController;
    [SerializeField] private CardDeckController      cardDeckController;
    [SerializeField] private CardResultController    cardResultController;

    [Header("Start Button")]
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
        startButton.gameObject.SetActive(false);
        characterController.PlayBeckoning();
        EnterPhase(GamePhase.Welcome);
    }

    private void HandleWelcome()
    {
        cameraController.GoToSeat(() =>
        {
            uiController.ShowDialogue("어서 오게나...", () => EnterPhase(GamePhase.Question));
        });
    }

    private void HandleQuestion()
    {
        uiController.ShowDialogue("그래서 고민이 무엇이냐", () =>
        {
            uiController.HideDialogue();
            uiController.ShowInputModal(worry =>
            {
                cardDeck.gameObject.SetActive(false); // 오브젝트 숨기기
                _userWorry = worry;
                EnterPhase(GamePhase.CardSelect);
            });
        });
    }

    private void HandleCardSelect() //callback when entering card select phase
    {
        cardDeck.SetActive(true);
        characterController.PlayWriting();
        cardDeckController.StartSelection((indices, isReversed) =>
        {
            _selectedCardIndices = indices;
            _isReversed          = isReversed;
            EnterPhase(GamePhase.Result);
        });
    }

    private void HandleResult()
    {
        uiController.ShowDialogue("흠…", null);
        
        cardDeck.gameObject.SetActive(true); // 오브젝트 표시
        uiController.HideDialogue();
        characterController.PlayClapping();
        uiController.ShowDialogue("결과를 말해주겠다", () =>
        {
            cardResultController.StartReveal(_selectedCardIndices, _isReversed, _userWorry);
        });
    }
}
