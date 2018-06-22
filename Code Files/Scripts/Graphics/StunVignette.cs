using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunVignette : Vignette
{

    public float TimeScale = 1.0f;              //Time scale for the shader to help determine how quickly object's and texture speeds will move
    [Range(0.1f, 10.0f)]
    public float AlphaMultiplier = 5.0f;        //A multiplier for the Alpha channel which is based on distance
    [Range(0.0f, 1.0f)]
    public float Frequency = 0.5f;              //Frequency for the Noise algorithm
    [Range(0.0f, 1.0f)]
    public float Amplitude = 0.5f;              //Amplitude for the Noise algorithm
    [Range(1, 5)]
    public int Octaves = 2;                     //Octaves for the Noise algorithm
    public float Lacunarity = 0.724f;           //Lacunarity for the Noise algorithm
    public float Gain = 3.0f;                   //Gain of each loop of the Noise algorithm
    protected override void Awake()
    {
        m_EffectName = Status.Stun.ToString();          //burn damage effect so it gets the name of the burn effect
        m_ShaderName = "Hidden/StunVignette";
        base.Awake();
    }

    protected override void SetUniforms()
    {
        base.SetUniforms();

        m_Material.SetFloat("_TimeScale", TimeScale);
        m_Material.SetFloat("_Alpha", AlphaMultiplier);
        m_Material.SetFloat("_Frequency", Frequency);
        m_Material.SetFloat("_Amplitude", Amplitude);
        m_Material.SetInt("_Octaves", Octaves);
        m_Material.SetFloat("_Lacunarity", Lacunarity);
        m_Material.SetFloat("_Gain", Gain);
    }
}
