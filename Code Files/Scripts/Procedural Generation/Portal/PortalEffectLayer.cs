using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalEffectLayer : MonoBehaviour
{
    public Color EffectColor = Color.white;
    private Material m_Material;    

    private void Awake()
    {
        m_Material = GetComponent<Renderer>().material;
        m_Material.SetColor("_EffectColor", EffectColor);
    }

    private void OnValidate()
    {
        if (m_Material != null)
            SetMaterialColor(EffectColor);
    }

    public void SetMaterialColor(Color color)
    {
        EffectColor = color;
        m_Material.SetColor("_EffectColor", EffectColor);
    }

    public void SetMateralTexture(Texture texture)
    {
        m_Material.SetTexture("_MainTex", texture);
    }
}
