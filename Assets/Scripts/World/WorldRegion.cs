using UnityEngine;

public class WorldRegion : MonoBehaviour
{
    [SerializeField]
    private float range = 5f;

    [SerializeField]
    private MusicProfile musicProfile;

    public float Range => range;

    public MusicProfile MusicProfile => musicProfile;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
