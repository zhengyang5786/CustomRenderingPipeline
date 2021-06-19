using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour
{
    public Color baseColor = new Color();
    [Range(0f, 1f)]
    public float metallic = 0f;
    [Range(0f, 1f)]
    public float smoothness = 0.5f;

    private static int baseColorId = Shader.PropertyToID("_BaseColor");
    private static int metallicId = Shader.PropertyToID("_Metallic");
    private static int smoothnessId = Shader.PropertyToID("_Smoothness");
    private static MaterialPropertyBlock block;

    private void OnValidate()
    {
        if (block == null)
        {
            block = new MaterialPropertyBlock();
        }
        block.SetFloat(metallicId, metallic);
        block.SetFloat(smoothnessId, smoothness);
        block.SetColor(baseColorId, baseColor);
        GetComponent<Renderer>().SetPropertyBlock(block);
    }

    private void Awake()
    {
        OnValidate();
    }
}
