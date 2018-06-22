using UnityEngine;

public class WaterBody : MonoBehaviour
{
    public float WaterSpeed = 1;                        //Speed at which the water travels
    [Range(-1.0f, 1.0f)]
    public float WaterDirectionX = 0;                   //The direction that the water will travel on the X axis
    [Range(-1.0f, 1.0f)]
    public float WaterDirectionY = 0;                   //The direction that the water will travel on the Y (Z-Axis visually) axis

    private Material m_StandardWaterMat;                //The standard water material that just moves the texture of the water

	void Awake ()
    {
        Renderer renderer = GetComponent<Renderer>();
        m_StandardWaterMat = renderer.material;
	}	
	
	void Update ()
    {
        m_StandardWaterMat.SetFloat("_WaterSpeed", WaterSpeed);
        m_StandardWaterMat.SetVector("_WaterDirection", new Vector4(WaterDirectionX, WaterDirectionY, 0, 0));
	}
}
