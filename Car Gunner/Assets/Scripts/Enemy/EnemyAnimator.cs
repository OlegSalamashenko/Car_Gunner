using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    private Animator _animator;

    private static readonly int IsRunning = Animator.StringToHash("isRunning");
    private static readonly int IsPunching = Animator.StringToHash("isPunching");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void SetRunning(bool isRunning)
    {
        _animator.SetBool(IsRunning, isRunning);
    }

    public void PlayPunch()
    {
        _animator.SetTrigger(IsPunching);
    }
}