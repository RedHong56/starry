using UnityEngine;
using DG.Tweening;

public class CameraIntroController : MonoBehaviour
{
    [Header("카메라 포인트")]
    public Transform skyPoint;      // 하늘 위치
    public Transform tablePoint;    // 테이블 위치

    [Header("설정")]
    public float moveDuration = 3f; // 카메라 이동 시간
    public Ease easeType = Ease.InOutSine;

    [Header("참조")]
    public GameObject startButton;     // Start 버튼
    public GameObject dialoguePanel;   // 말풍선

    void Start()
    {
        // 시작: 카메라를 하늘 위치로
        transform.position = skyPoint.position;
        transform.rotation = skyPoint.rotation;

        // 말풍선 숨기기
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    // Start 버튼에서 호출
    public void OnStartButtonClicked()
    {
        // 버튼 숨기기
        startButton.SetActive(false);

        // 카메라 이동 (DOTween)
        transform.DOMove(tablePoint.position, moveDuration)
            .SetEase(easeType);

        transform.DORotateQuaternion(tablePoint.rotation, moveDuration)
            .SetEase(easeType)
            .OnComplete(() =>
            {
                // 카메라 도착 후 말풍선 표시
                if (dialoguePanel != null)
                    dialoguePanel.SetActive(true);
            });
    }
}
