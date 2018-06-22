using UnityEngine;

//Writer: Liam
//Edited: 12/21/2017

public class RenderTarget : MonoBehaviour
{
    private Shader m_ReflectingShader;                  //the reflective shader that will take the camera's image and display it on the reflecting shader
    private Camera m_Camera;                            //the camera that is a child of this object/the thing who's view we are grabbing to render the image
    private Renderer m_Renderer;                        //the objects renderer that we are going to be rendering to
    private Material m_Material;                        //the material that will be used for the render texture
    private RenderTexture m_RenderTexture;              //the render texture of the camera

    private void Awake()
    {
        m_ReflectingShader = Shader.Find("Unlit/PortalShader");
        m_Material = new Material(m_ReflectingShader);

        //check to see if the camera texture is the child of the object we are attached to 
        m_Camera = GetComponentInChildren<Camera>();

        //if the camera is still null it means that this component is attached to the camera itself
        if (m_Camera == null)
        {
            //understand how this script works please
            Debug.Assert(true, "This script needs to be attached to an object that has a camera as it's child");
        }

        m_RenderTexture = new RenderTexture(new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.ARGBFloat, 24));
        m_Material.SetTexture("_MainTex", m_RenderTexture);

        m_Camera.targetTexture = m_RenderTexture;

        //get the renderer to set up the material 
        m_Renderer = GetComponent<Renderer>();
        m_Renderer.GetComponent<Renderer>().material = m_Material;
    }

    public Material Material { get { return m_Material; } set { m_Material = value; } }
    public RenderTexture RenderTexture { get { return m_RenderTexture; } }
}
