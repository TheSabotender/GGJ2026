using UnityEngine;

public class SimpleTransform : MonoBehaviour
{
    [System.Serializable]
    public class TransformData
    {
        public enum UpdateMode
        {
            Update,
            FixedUpdate,
            LateUpdate
        }

        public Transform transform;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        public Space space;
        public UpdateMode updateMode;
        public bool useDeltaTime;
    }

    public TransformData[] data;

    private void Update()
    {
        for (int i = 0; i < data.Length; i++)
        {
            var d = data[i];
            if(d.transform == null)
                d.transform = transform;
            if(d.updateMode == TransformData.UpdateMode.Update)
                UpdateTransform(d);
        }
    }

    private void LateUpdate()
    {
        for (int i = 0; i < data.Length; i++)
        {
            var d = data[i];
            if (d.transform == null)
                d.transform = transform;
            if (d.updateMode == TransformData.UpdateMode.LateUpdate)
                UpdateTransform(d);
        }
    }
    private void FixedUpdate()
    {
        for (int i = 0; i < data.Length; i++)
        {
            var d = data[i];
            if (d.transform == null)
                d.transform = transform;
            if (d.updateMode == TransformData.UpdateMode.FixedUpdate)
                UpdateTransform(d);
        }
    }

    static void UpdateTransform(TransformData d)
    {
        if(d.transform == null)
            return;

        var tMul = d.useDeltaTime ? Time.deltaTime : 1f;

        d.transform.Rotate(d.rotation * tMul, d.space);
        switch (d.space)
        {
            case Space.World:
                d.transform.position += d.position * tMul;
                d.transform.localScale += d.scale;
                break;
            case Space.Self:
                d.transform.localPosition += d.position;
                d.transform.localScale += d.scale;
                break;
        }
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < data.Length; i++)
        {
            var d = data[i];
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(d.position, 0.1f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(d.position, d.scale);
        }
    }
}
