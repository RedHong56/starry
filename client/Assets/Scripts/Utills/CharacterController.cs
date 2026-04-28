using UnityEngine;

// Unity의 내장 CharacterController와 이름 충돌 방지
public class TarotCharacterController : MonoBehaviour
{
    private static readonly int HashBeckoning     = Animator.StringToHash("Beckoning");
    private static readonly int HashAskingQuestion = Animator.StringToHash("AskingQuestion");
    private static readonly int HashWriting        = Animator.StringToHash("Writing");
    private static readonly int HashClapping       = Animator.StringToHash("Clapping");

    [SerializeField] private Animator animator;

    public void PlayBeckoning()      => animator.SetTrigger(HashBeckoning);
    public void PlayAskingQuestion() => animator.SetTrigger(HashAskingQuestion);
    public void PlayWriting()        => animator.SetTrigger(HashWriting);
    public void PlayClapping()       => animator.SetTrigger(HashClapping);
}
