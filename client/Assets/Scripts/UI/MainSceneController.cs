// Scripts/Core/MainSceneController.cs

using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class MainSceneController : MonoBehaviour
{
    public enum Phase
    {
        Sky,        // 하늘
        Walking,    // 걷기
        Sitting,    // 앉기
        Input       // 입력
    }

    [Header("카메라 포인트")]
    public Transform skyPoint;
    public Transform walkStartPoint;
    public Transform walkEndPoint;
    public Transform sitPoint;

    [Header("UI")]
    public GameObject touchToStartUI;   // "터치하여 시작"
    public GameObject dialoguePanel;    // 말풍선
    public GameObject inputPanel;       // 고민 입력창

    [Header("캐릭터")]
    public Animator characterAnimator;

    [Header("설정")]
    public float walkDuration = 2.5f;
    public float sitDuration = 0.8f;

    private Phase currentPhase = Phase.Sky;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        // 시작: 하늘 위치
        mainCamera.transform.position = skyPoint.position;
        mainCamera.transform.rotation = skyPoint.rotation;

        // UI 초기화
        dialoguePanel.SetActive(false);
        inputPanel.SetActive(false);
        touchToStartUI.SetActive(true);
    }

    void Update()
    {
        // 터치/클릭 감지
        if (Input.GetMouseButtonDown(0) ||
            (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            OnScreenTouched();
        }
    }

    void OnScreenTouched()
    {
        if (currentPhase == Phase.Sky)
            StartWalking();
    }

    void StartWalking()
    {
        currentPhase = Phase.Walking;
        touchToStartUI.SetActive(false);

        // 카메라 걸어들어가는 연출
        Sequence walkSeq = DOTween.Sequence();

        // 1. 하늘에서 입구로 이동
        walkSeq.Append(
            mainCamera.transform.DOMove(walkStartPoint.position, 1f)
                .SetEase(Ease.InOutSine)
        );
        walkSeq.Join(
            mainCamera.transform.DORotateQuaternion(walkStartPoint.rotation, 1f)
                .SetEase(Ease.InOutSine)
        );

        // 2. 걸어들어가기 (Bob 효과 포함)
        walkSeq.Append(
            mainCamera.transform.DOMove(walkEndPoint.position, walkDuration)
                .SetEase(Ease.Linear)
        );
        walkSeq.Join(
            mainCamera.transform.DORotateQuaternion(walkEndPoint.rotation, walkDuration)
                .SetEase(Ease.InOutSine)
        );

        // 3. 앉기
        walkSeq.OnComplete(() => StartSitting());
    }

    void StartSitting()
    {
        currentPhase = Phase.Sitting;

        // 카메라가 살짝 아래로 (앉는 효과)
        mainCamera.transform
            .DOMove(sitPoint.position, sitDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => OnSitComplete());
    }

    void OnSitComplete()
    {
        currentPhase = Phase.Input;

        // 캐릭터 대사 등장
        dialoguePanel.SetActive(true);
        dialoguePanel.transform.localScale = Vector3.zero;
        dialoguePanel.transform
            .DOScale(1f, 0.4f)
            .SetEase(Ease.OutBack);

        // 캐릭터 Talking 애니메이션
        characterAnimator.SetTrigger("Talk");

        // 1.5초 후 입력창 표시
        DOVirtual.DelayedCall(1.5f, () =>
        {
            inputPanel.SetActive(true);
            inputPanel.transform.localScale = Vector3.zero;
            inputPanel.transform
                .DOScale(1f, 0.4f)
                .SetEase(Ease.OutBack);
        });
    }

    // 입력 완료 버튼에서 호출
    public void OnInputComplete()
    {
        // TODO: 입력값 저장 후 S#3으로
        SceneManager.LoadScene("03_Reading");
    }
}