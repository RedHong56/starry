using System;
using UnityEngine;
using DG.Tweening;

public class CameraContorller : MonoBehaviour
{
    [Header("Camera Transform to animate")]
    [SerializeField] private Transform cameraTransform;

    [Header("Target Anchors (empty GameObjects in scene)")]
    [SerializeField] private Transform beginTarget;
    [SerializeField] private Transform walkTarget;
    [SerializeField] private Transform seatTarget;

    [Header("Durations")]
    [SerializeField] private float walkDuration     = 2f;
    [SerializeField] private float seatZDuration    = 1.5f;
    [SerializeField] private float seatRotDuration  = 1f;

    public void GoToBegin()
    {
        cameraTransform.SetPositionAndRotation(beginTarget.position, beginTarget.rotation);
    }

    public void GoToWalk(Action onComplete)
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(cameraTransform.DOMove(walkTarget.position, walkDuration).SetEase(Ease.InOutSine));
        seq.Join(cameraTransform.DORotateQuaternion(walkTarget.rotation, walkDuration).SetEase(Ease.InOutSine));
        seq.OnComplete(() => onComplete?.Invoke());
    }

    // Z축 이동 먼저 → 이후 최종 위치 + X 회전
    public void GoToSeat(Action onComplete)
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(cameraTransform.DOMoveZ(seatTarget.position.z, seatZDuration).SetEase(Ease.InOutSine));
        seq.Append(cameraTransform.DOMove(seatTarget.position, seatRotDuration).SetEase(Ease.InOutSine));
        seq.Join(cameraTransform.DORotateQuaternion(seatTarget.rotation, seatRotDuration).SetEase(Ease.InOutSine));
        seq.OnComplete(() => onComplete?.Invoke());
    }
}
