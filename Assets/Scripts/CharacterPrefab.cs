using UnityEngine;

public class CharacterPrefab : MonoBehaviour
{
    private Material material;
    public Material Material
    {
        get
        {
            if (material == null)
            {
                var renderers = GetComponentsInChildren<Renderer>();
                if (renderers?.Length > 0)
                    material = new Material(renderers[0].material);
                else
                    material = new Material(Shader.Find("CharacterShader"));

                foreach (var renderer in renderers)
                    renderer.material = (material);
            }

            return material;
        }
    }
}
