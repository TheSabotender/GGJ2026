using UnityEngine;
using static CharacterProfile;

public class Door : MonoBehaviour
{
    public const string ANIM_BOOL_OPEN = "Open";
    public const string ANIM_BOOL_LOCKED = "Locked";

    public Animator Animator;
    public float openDistance = 3f;
    public WorldRegion region;

    public JobTag fieldsAllowed;
    public Clearance minimumClearance;

    private void Update()
    {
        var playerPos = GameManager.PlayerBrain.transform.position;
        if (Vector3.Distance(transform.position, playerPos) <= openDistance)
        {
            var hasAccess = HasAccess(GameManager.CurrentGameSave.CurrentProfile);
            Animator.SetBool(ANIM_BOOL_OPEN, hasAccess);
            Animator.SetBool(ANIM_BOOL_LOCKED, !hasAccess);
        }
        else
        {
            Animator.SetBool(ANIM_BOOL_OPEN, false);
            Animator.SetBool(ANIM_BOOL_LOCKED, false);
        }    
    }

    public bool HasAccess(CharacterProfile profile)
    {
        return HasAccess(profile.field, profile.securityClearance);
    }

    public bool HasAccess(JobTag field, Clearance clearance)
    {
        var hasJob = (fieldsAllowed & field) != 0;
        var clear = clearance >= minimumClearance;
        return hasJob && clear;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, openDistance);
    }
}
