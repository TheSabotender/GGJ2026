using UnityEngine;
using UnityEngine.InputSystem;

public partial class TendrilManager : MonoBehaviour
{
    [Header("Grapple Tendril")]
    [SerializeField] private InputActionReference mouseAction = null;
    [SerializeField] private float tendrilLength = 10f;
    [SerializeField] private Vector2 tendrilSpeed = new Vector2(20, 30);
    [SerializeField] private float tendrilStrength = 10f;
    [SerializeField][Range(0, 1)] private float tendrilElasticity = 0.8f;
    [SerializeField] private int tendrilCount = 5;
    [SerializeField] private float tendrilScatter = 1f;

    public static bool isHoldingTendril { get; private set; }

    public void LaunchTendril()
    {
        isHoldingTendril = true;

        if (!Util.TryGetAimWorldPoint(mouseAction, out Vector3 aimWorld))
            return;

        for (int i = 0; i < tendrilCount; i++)
        {
            var random = new Vector3(Random.Range(-tendrilScatter, tendrilScatter), Random.Range(-tendrilScatter, tendrilScatter), 0);
            var hit = GetTendrilHit(aimWorld + random, tendrilLength, out GameObject hitObject, out Vector3 tendrilEnd);
            var rope = Instantiate(ropePrefab, tendrilParent);

            if (hitObject != null)
            {
                var damageable = hitObject.GetComponent<Damageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage();
                    hit = false; // Tendril doesn't latch if it just damages
                }
            }

            if (hit)
                rope.StartCoroutine(Latch(rope, tendrilEnd, tendrilSpeed, tendrilStrength / tendrilCount, tendrilElasticity, hitObject, isSpider: false));
            else
                rope.StartCoroutine(Miss(rope, tendrilEnd, tendrilSpeed, tendrilElasticity));
        }
    }

    public void ReleaseTendril()
    {
        isHoldingTendril = false;
    }
}
