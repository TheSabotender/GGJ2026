using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public partial class TendrilManager : MonoBehaviour
{
    [SerializeField] private Transform tendrilParent = null;
    [SerializeField] private Rope ropePrefab;
    [SerializeField] private PlayerBrain playerBrain = null;
    [SerializeField] private LayerMask tendrilLayerMask;
    [SerializeField] private float eatDistance = 1f;
    [SerializeField] private VisualEffect eatVFXPrefab;

    private Camera mainCam;

    private void Awake()
    {
        mainCam = Camera.main;
        floorTendrils = new();
    }

    private void LateUpdate()
    {
        UpdateFloorTendrils();
    }

    bool GetTendrilHit(Vector3 targetWorld, float maxLength, out GameObject hitObject, out Vector3 hit)
    {
        Vector3 origin = tendrilParent.position;
        Vector3 dir = (targetWorld - origin);

        // Avoid NaNs if target == origin
        if (dir.sqrMagnitude < 0.0001f)
            dir = tendrilParent.forward;

        Ray ray = new Ray(origin, dir.normalized);

        if (Physics.Raycast(ray, out RaycastHit rayHit, maxLength, tendrilLayerMask, QueryTriggerInteraction.Ignore))
        {
            hitObject = rayHit.collider.gameObject;
            hit = rayHit.point;
            return true;
        }

        hitObject = null;
        hit = ray.GetPoint(maxLength);
        return false;
    }

    private IEnumerator Latch(Rope rope, Vector3 target, Vector2 tendrilSpeed, float tendrilStrength, float tendrilElasticity, GameObject hitObject, bool isSpider)
    {
        var playerBrain = GameManager.PlayerBrain;

        var hitRigidbody = hitObject != null ? hitObject.GetComponent<Rigidbody>() : null;
        var hitOffset = hitRigidbody != null ? hitRigidbody.position - target : Vector3.zero;

        float distance = Vector3.Distance(tendrilParent.position, target);
        float extendDuration = distance / tendrilSpeed.x;
        float retractDuration = distance / tendrilSpeed.y;
        rope.Elasticity = tendrilElasticity;

        float elapsed = 0f;
        while (elapsed < extendDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / extendDuration);
            rope.StartPoint = tendrilParent.position;
            rope.EndPoint = Vector3.Lerp(rope.StartPoint, target, t);
            rope.Elasticity = Mathf.Lerp(tendrilElasticity, 1, t);

            if(tendrilStrength > 0)
            {
                Vector3 pullDirection = (target - rope.StartPoint).normalized;
                //Vector3 pullDirection = (rope.NextPoint - rope.StartPoint).normalized;
                playerBrain.Rigidbody.AddForce(pullDirection * tendrilStrength, ForceMode.Acceleration);

                if (hitRigidbody != null)
                    hitRigidbody.AddForce(-pullDirection * tendrilStrength, ForceMode.Force);
            }
            yield return null;
        }

        while ((isSpider && isHoldingSpider) || (!isSpider && isHoldingTendril))
        {
            rope.StartPoint = tendrilParent.position;
            rope.EndPoint = target;

            if (tendrilStrength > 0)
            {
                Vector3 pullDirection = (target - rope.StartPoint).normalized;
                //Vector3 pullDirection = (rope.NextPoint - rope.StartPoint).normalized;
                playerBrain.Rigidbody.AddForce(pullDirection * tendrilStrength, ForceMode.Acceleration);
                playerBrain.transform.forward = (pullDirection.x > 0f) ? Vector3.forward : Vector3.back;

                if (hitRigidbody != null)
                {
                    hitRigidbody.AddForce(-pullDirection * tendrilStrength, ForceMode.Force);
                    target = hitRigidbody.position + hitOffset;
                    rope.EndPoint = target;
                }

                if (hitObject != null)
                {
                    var npc = hitObject.GetComponent<AIBrain>();
                    var distanceToPlayer = Vector3.Distance(playerBrain.transform.position, hitObject.transform.position);
                    if (npc != null && distanceToPlayer <= eatDistance)
                    {
                        var npcMask = npc.profile;

                        if (npc.IsAlive)
                            Util.SpawnOneShotVfx(eatVFXPrefab, target);
                        npc.Kill();

                        var isBeingObserved = playerBrain.ObservationManager.ObserverCount > 0;
                        if (isBeingObserved)
                        {
                            RegionManager.CurrentRegion.SetAlertState(GameManager.AlertState.Alert);
                        }

                        var maskState = new MaskState()
                        {
                            guid = npcMask.Guid,
                            status = isBeingObserved ? MaskStatus.Compromised : MaskStatus.Fresh
                        };

                        GameManager.PlayerBrain.AddMask(npcMask);
                        ReleaseTendril();
                    }
                }
            }
            yield return null;
        }

        rope.Elasticity = tendrilElasticity;

        elapsed = 0f;
        while (elapsed < retractDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / retractDuration);
            rope.StartPoint = tendrilParent.position;
            rope.EndPoint = Vector3.Lerp(target, rope.StartPoint, t);
            yield return null;
        }

        Destroy(rope.gameObject);
    }

    private IEnumerator Miss(Rope rope, Vector3 target, Vector2 tendrilSpeed, float tendrilElasticity)
    {
        rope.Elasticity = tendrilElasticity;

        float distance = Vector3.Distance(tendrilParent.position, target);
        float extendDuration = distance / tendrilSpeed.x;
        float retractDuration = distance / tendrilSpeed.y;

        float elapsed = 0f;
        while (elapsed < extendDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / extendDuration);
            rope.StartPoint = tendrilParent.position;
            rope.EndPoint = Vector3.Lerp(rope.StartPoint, target, t);

            yield return null;
        }

           elapsed = 0f;
        while (elapsed < retractDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / retractDuration);
            rope.StartPoint = tendrilParent.position;
            rope.EndPoint = Vector3.Lerp(target, rope.StartPoint, t);
            yield return null;
        }

        Destroy(rope.gameObject);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || mainCam == null)
            return;

        if (!Util.TryGetAimWorldPoint(mouseAction, out Vector3 aimWorld))
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(aimWorld, 0.5f);

        bool hit = GetTendrilHit(aimWorld, tendrilLength, out _, out Vector3 tendrilEnd);
        Gizmos.color = hit ? Color.green : Color.red;
        Gizmos.DrawLine(tendrilParent.position, tendrilEnd);
    }
}
