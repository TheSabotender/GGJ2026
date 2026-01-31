using UnityEngine;

public partial class TendrilManager : MonoBehaviour
{
    [Header("Spider Tendrils")]
    public int spiderTendrilCount = 8;

    private bool isHoldingSpider;

    public void SpreadTendrils()
    {
        isHoldingSpider = true;

        for (int i = 0; i < spiderTendrilCount; i++)
        {
            var angle = i * (360f / tendrilCount);
            var direction = Quaternion.Euler(0, 0, angle) * Vector3.right;
            var aimWorld = tendrilParent.position + direction * tendrilLength;

            var random = new Vector3(Random.Range(-tendrilScatter, tendrilScatter), Random.Range(-tendrilScatter, tendrilScatter), 0);
            var hit = GetTendrilHit(aimWorld + random, tendrilLength, out _, out Vector3 tendrilEnd);
            var rope = Instantiate(ropePrefab, tendrilParent);

            if (hit)
                rope.StartCoroutine(Latch(rope, tendrilEnd, tendrilSpeed, tendrilStrength / tendrilCount, tendrilElasticity, null, isSpider: true));
            else
                rope.StartCoroutine(Miss(rope, tendrilEnd, tendrilSpeed, tendrilElasticity));
        }
    }

    public void ReleaseSpread()
    {
        isHoldingSpider = false;
    }
}
