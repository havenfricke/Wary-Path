using UnityEngine;
using System.Collections;

public class DamageColliderEventHandler : MonoBehaviour
{
    private EquippedWeapon weapon;
    private Animator animator; 

    private string attackAnimationStateName = "Continuous_Attack";

    private void Awake()
    {
        weapon = GetComponentInChildren<EquippedWeapon>();
        animator = GetComponent<Animator>(); 
    }

    // called via an animation event 
    public void OpenDamageCollider()
    {
        weapon.OpenColldier();
        
        StartCoroutine(WaitForAnimationToEnd());
    }

    public void CloseDamageCollider()
    {
        weapon.CloseColldier();
    }

    // Coroutine to wait until the attack animation has finished.
    private IEnumerator WaitForAnimationToEnd()
    {
        if (animator == null)
        {
            Debug.LogWarning("No Animator found, cannot wait for animation to end.");
            yield break;
        }

        // Wait until the attack animation is playing.
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(attackAnimationStateName));

        // Wait until the attack animation is finished.
        yield return new WaitUntil(() =>
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            // Check if normalized time is at least 1 (animation finished) and not transitioning.
            return stateInfo.normalizedTime >= 1f && !animator.IsInTransition(0);
        });

        // When finished, close the damage collider.
        CloseDamageCollider();
    }
}

