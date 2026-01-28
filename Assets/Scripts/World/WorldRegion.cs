using UnityEngine;

public class WorldRegion : MonoBehaviour
{
    [SerializeField]
    private MusicProfile musicProfile;

    [SerializeField]
    private Light[] standardLights;

    [SerializeField]
    private Light[] securityLights;

    public GameManager.AlertState AlertState { get; private set; }

    public MusicProfile MusicProfile => musicProfile;

    public Bounds[] bounds;

    void Awake()
    {
        SetAlertState(GameManager.AlertState.Normal);
    }

    public void SetAlertState(GameManager.AlertState newAlert)
    {
        AlertState = newAlert;

        var color = newAlert switch
        {
            GameManager.AlertState.Caution => Color.yellow,
            GameManager.AlertState.Alert => Color.red,
            _ => Color.white
        };

        foreach (var light in standardLights)
            light.enabled = newAlert != GameManager.AlertState.Alert;

        foreach (var light in securityLights)
        {
            light.enabled = newAlert != GameManager.AlertState.Normal;
            light.color = color;
        }
    }

    public bool IsWithinRegion(Vector3 position)
    {
        foreach (var b in bounds)
        {
            if (b.Contains(position - transform.position))
                return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        foreach (var b in bounds)
        {
            Gizmos.DrawWireCube(b.center + transform.position, b.size);
        }
    }
}
