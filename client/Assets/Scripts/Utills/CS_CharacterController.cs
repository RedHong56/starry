using UnityEngine;

// Unity의 내장 CharacterController와 이름 충돌 방지
public class TarotCharacterController : MonoBehaviour
{
    private static readonly int HashTalk   = Animator.StringToHash("Talk");
    private static readonly int HashBeckon = Animator.StringToHash("Beckon");
    private static readonly int HashClap   = Animator.StringToHash("Clap");

    private static readonly int HashWrite  = Animator.StringToHash("Write");

    [SerializeField] private Animator animator;

    public void PlayBeckoning()      => animator.SetBool(HashBeckon, true);

    public void PlayWriting()        => animator.SetBool(HashWrite,   true);
    public void StopWriting()        => animator.SetBool(HashWrite,   false);
    public void PlayClapping()       => animator.SetBool(HashClap,    true);
    public void PlayTalking()        => animator.SetBool(HashTalk,    true);
}
