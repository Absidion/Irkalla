using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageNumber : MonoBehaviour
{
    public float    ScalingFactor = 250;         // A number to divite 
    public float    ConeAngle = 0.5f;            // The angle that the number will be shot out of  
    public float    NumberSpeed = 3.0f;          // The speed the number go by
    public float    DisplayTime    = 3.0f;       // The time the Number will stay displayed
    public float    UpdateTime = 0.01f;          // The time needed to add to the counter
    private Health  m_ActiveHealth;              // The Health compnent we are getting the damage from
    private Text    m_TextToDisplay;             // Text to Display
    private Player  m_LocalPlayerRef;            // Is the ref to the local player
    private float   m_DisplayTimer = 0.0f;       // The timer added to each fram
    private float   m_UpdateTimer = 0.0f;        // The timer for the updating of the Damage Number itsel
    private int     m_DamageBuildUp = 0;         // Cumulative counter of taken Damage
    private Vector3 m_NumberDir;                 // The direction the number is traveling
    private GameObject m_MainCameraRef;          // A Ref to the Main Character's Position

    public void Init()
    {
        //Get the Camera if its null
        if (m_MainCameraRef == null)
            m_MainCameraRef = Camera.main.gameObject;

        //Get the direction this Number will be travelling
        if (m_ActiveHealth != null)
        {
            m_NumberDir = GetRandomDirWithenCone();
            m_NumberDir.Normalize();
            //Hard set the Dir.y to 1
            m_NumberDir.y = 1;
            //Set the position to the ActiveHealth's postion
            transform.position = m_ActiveHealth.transform.position;
            m_TextToDisplay.font.material.renderQueue = 4999;
            if (m_MainCameraRef != null)
                return;
        }
    }

	// Update is called once per frame
	void Update ()
    {
        //If there is no player/health/camera ref initialize the component
        if (m_LocalPlayerRef == null || m_ActiveHealth == null || m_MainCameraRef == null)
        {
            Init();
        }
        //Once the timer reaches its stop adding to this counter but it will contunue to Display
        if(m_UpdateTimer > UpdateTime )
        {
            //Reset all the vars before disabling the Game object
            // The the m_ActiveHealth's Damage number to null because the Damage count manager uses this to tell what AI has an active Damage Number
            m_DamageBuildUp = 0;
            if(m_ActiveHealth != null)
                m_ActiveHealth.DamageNumber = null;
        }
        if(m_DisplayTimer > DisplayTime)
        {
            m_ActiveHealth = null;
            m_TextToDisplay.text = 0.0f.ToString();
            gameObject.SetActive(false);
        }

        // Look at the player so the text does not display at an angle
        transform.LookAt(m_LocalPlayerRef.transform);
        SetScaleBasedOnDistance();
        MoveDamageNumnber();

        
        //Set the local position to just above the AI


        //Increment the timers
        m_UpdateTimer += Time.deltaTime;
        m_DisplayTimer += Time.deltaTime;
    }


    private void SetScaleBasedOnDistance()
    {
        //Get the Scale as the distance between the player's camera and the The Number's Rect Transform
        float DyanmicSize = Vector3.Magnitude(m_MainCameraRef.transform.position - transform.position);
        // Divide by the ScalingFactor
        DyanmicSize /= ScalingFactor;
        //Set the scale of the Number (Reverse the X scale to make the Text Readable)
        transform.localScale = new Vector3(-DyanmicSize, DyanmicSize, DyanmicSize);
    }

    private void MoveDamageNumnber()
    {
        //If the Active Health is null set HealthObjPos to 0 so the Number will keep moving by itself
        Vector3 HealthObjPos;
        if(m_ActiveHealth != null)
        {
            HealthObjPos = m_ActiveHealth.gameObject.transform.position;
        }
        else
        {
            HealthObjPos = Vector3.zero;
        }

        //Move the Position based on the starting postion
        Vector3 FrameStep = (m_NumberDir * NumberSpeed) * Time.deltaTime;
        //Get the Numbers position and add the frame step
        Vector3 Objectpostion = transform.position + FrameStep;
        //Get the difrence of the ObjectPos - HeathObjPos to the HealthObjPos
        Vector3 Diffrence = Objectpostion - HealthObjPos;
        //Get the diffrence between the location
        Vector3 FinalPos = HealthObjPos + Diffrence;
        //Set the final pos
        transform.position = FinalPos;
    }

    private Vector3 GetRandomDirWithenCone()
    {
        //The Final dir the 
        Vector3 finaldir = Vector3.up;
        //Get a Random Local X as compared to the Object's Forward
        float ForwardX = transform.forward.x;
        float finalXDir = Random.Range(ForwardX - ConeAngle, ForwardX + ConeAngle);
        //Get a Random Local Z as compared to the Object's Forward but use a divied number to keep the numbers from flying in front of the Object too often
        float ForwardZ = transform.forward.z / 4;
        float finalZDir = Random.Range(ForwardZ - ConeAngle, ForwardZ + ConeAngle);
        //Add the final values to the Vector3 being returned
        finaldir.x = finalXDir;
        finaldir.z = finalZDir;
        return finaldir;
    }

    public void AddDamage(int damage, Health health, Player localPlayer, Color color)
    {
        // get the text component if its null
        if (m_TextToDisplay == null)
        {
            m_TextToDisplay = GetComponent<Text>();
        }
        m_LocalPlayerRef = localPlayer;
        //Set the active health to this and set the active Damage number in the health
        m_ActiveHealth = health;
        m_ActiveHealth.DamageNumber = this;
        //Add to the Damage build up and reset the timer
        m_DamageBuildUp += damage;
        m_TextToDisplay.text = m_DamageBuildUp.ToString();
        m_TextToDisplay.color = color;
        m_DisplayTimer = 0.0f;
    }
    public void AddDamage(int damage)
    {
        //Add to the Damage build up and reset the timer
        m_DamageBuildUp += damage;
        m_TextToDisplay.text = m_DamageBuildUp.ToString();
        m_DisplayTimer = 0.0f;
    }
}
