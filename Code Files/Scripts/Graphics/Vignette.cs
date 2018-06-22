using UnityEngine;

//Writer: Liam
//Last Updated: 1/7/2018

public class Vignette : MonoBehaviour
{
    [Range(0, 1.0f)]
    public float CenterSize = 0.5f;                         //this determines how big the center ring will be. (in clip space)
    [Range(0, 1.0f)]
    public float RemainVignetteFalloff = 0.5f;              //this determines how much of the remainning vignette will be fading out over a soild color/texture
    [Range(0.0f, 1.0f)]
    public float HeightOfVignette = 0.0f;                   //how height the vignette will be in clip pos

    public bool DontDisable = false;

    public Texture2D EffectTexture;                         //The effect texture that the viginette will use

    protected Material m_Material;                          //The render material
    protected Camera m_Camera;                              //The camera that willl be rendered to
    protected RenderTexture m_Texture;                      //The rendertexture of the camera
    protected string m_ShaderName = "Hidden/Vignette";      //The name of the shader that will be used

    protected string m_EffectName = string.Empty;           //The specific name of the effect

    protected float m_TimerLength;                          //How long the effect will last
    protected float m_Timer;                                //How long the effect has been up

    public string EffectName { get { return m_EffectName; } }
    public string ShaderName { get { return m_ShaderName; } }
    public float TimerLength { get { return m_TimerLength; } set { m_TimerLength = value; } }

    protected virtual void Awake()
    {
        var shader = Shader.Find(m_ShaderName);                         //Find the shader that we're looking for by name
        m_Material = new Material(shader);                              
        m_Camera = GetComponent<Camera>();
        //Create a new render texture
        m_Texture = new RenderTexture(new RenderTextureDescriptor(
            m_Camera.pixelWidth,
            m_Camera.pixelHeight,
            RenderTextureFormat.ARGBFloat,
            24)
            );

        //Set the main texture and the effect texture
        m_Material.SetTexture("_MainTex", m_Texture);
        m_Material.SetTexture("_EffectTex", EffectTexture);
    }

    protected void Update()
    {
        if (DontDisable)
            return;

        if(m_Timer > m_TimerLength)
        {
            enabled = false;            
        }
        m_Timer += Time.deltaTime;
    }

    public void SetEffect(float timerLength)
    {
        enabled = true;
        m_Timer = 0.0f;
        m_TimerLength = timerLength;
    }

    protected void OnPreRender()
    {
        m_Camera.targetTexture = m_Texture;
    }

    protected void OnPostRender()
    {
        m_Camera.targetTexture = null;
    }

    [ImageEffectOpaque]
    protected void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        var trt = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);

        //set up the uniforms
        SetUniforms();

        Graphics.Blit(source, destination, m_Material);

        RenderTexture.ReleaseTemporary(trt);
    }

    protected virtual void SetUniforms()
    {
        m_Material.SetFloat("_Intensity", CenterSize);
        m_Material.SetFloat("_Falloff", RemainVignetteFalloff);
        m_Material.SetFloat("_Height", HeightOfVignette);
    }
}
