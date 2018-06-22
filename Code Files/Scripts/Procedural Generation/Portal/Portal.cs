using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Writer: Liam
//Laste Updated: 1/15/2017

[RequireComponent(typeof(RenderTarget), typeof(BoxCollider))]
public class Portal : MonoBehaviour
{
    public float PortalBrightness = 1.0f;

    [SerializeField]
    private Portal m_ConnectedPortal = null;    //the portal that this one is connected to  
    private IslandRoom m_ConnectedRoom = null;  //the room that this portal's connected portal is in
    private IslandRoom m_ParentRoom = null;     //the room that this portal is attached to 

    private int m_CollisionMask;                //the objects that can collide with the portals

    private float m_DisapearingTimer;           //the timer for how long the portal has been in the disapearing state attempting to disapear from the world

    private float m_CameraMinHeight;            //the minimum height that the camera can go to
    private float m_CameraMaxHeight;            //the maximum height that the camera can go to

    private bool m_IsActiveInRoom = true;       //whether or not the portal is active in the room it's in
    private bool m_IsInUse = false;             //whether or not the portal is in use or not

    private Renderer m_Renderer;                //the objects render
    private BoxCollider m_Collider;             //the box collider for the object
    private RenderTarget m_RenderTarget;        //the render target of the portal
    private Camera m_PortalCamera;              //the camera that is childed to the portal
    private PortalEffectLayer m_PortalEffect;   //The portal's effect layer
    private AudioSource m_PortalSFX;            //Audiosource for the Ambient Portal SoundEffect
    private void Awake()
    {
        m_CollisionMask = LayerMask.GetMask("Player");

        //initialize the renderer component
        m_Renderer = GetComponent<Renderer>();
        if (m_Renderer == null)
        {
            Debug.LogError("Please add a renderer to the portal object, ", this);
        }

        //initialize the collider component
        m_Collider = GetComponent<BoxCollider>();
        if (m_Collider == null)
        {
            Debug.LogError("Please add a boxcollider to the portal object, i don't know how you even removed it, ", this);
        }

        m_RenderTarget = GetComponentInChildren<RenderTarget>();
        m_PortalCamera = GetComponentInChildren<Camera>();
        m_PortalEffect = GetComponentInChildren<PortalEffectLayer>();

        m_CameraMinHeight = -m_Collider.bounds.extents.y;
        m_CameraMaxHeight = m_Collider.bounds.extents.y;
        m_Renderer.material.renderQueue = 4000;
    }

    private void Update()
    {
        //if the portal is in the middle of disapearing from the room then we need to update that function until the shader animation
        //is completed.
        if (IsActiveInRoom == false)
        {
            DeactivatePortal();
            Renderer.material.SetFloat("_BrightnessFilter", PortalBrightness);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //check to make sure the object isn't currently using the other portal and if they aren't we can send them
        //through to the other portal, otherwise do nothing. Also check to make sure that the portal isn't disapearing from the room at the
        //time of the collision
        if ((m_IsActiveInRoom == true) && collision.collider.tag == "Player")
        {
            //dot product to make sure that the player is moving into the portal from the correct side
            Vector3 portalToPlayer = collision.collider.transform.position - transform.position;
            float dot = Vector3.Dot(transform.forward, portalToPlayer);

            //calculate the reflected direction
            Vector3 reflectedDir = Vector3.Reflect(collision.collider.transform.forward, collision.contacts[0].normal);

            this.Room.TogglePortalFXLayer(false);

            if (dot > 0.0f)
            {
                ConnectedPortal.TeleportObject(collision.collider.gameObject, reflectedDir, transform.position - collision.gameObject.transform.position);

                if (!m_ConnectedPortal.m_ParentRoom.IsRoomCleared && collision.gameObject.GetComponent<Player>())
                {
                    if (PhotonNetwork.player.IsLocal)
                        SoundManager.PlayCombat();
                }
            }
        }
    }

    public void TeleportObject(GameObject objectToTeleport, Vector3 reflectedDir, Vector3 positionOffset)
    {
        Vector3 offset = ConnectedPortal.transform.InverseTransformDirection(positionOffset);
        offset = -transform.TransformDirection(offset);

        //transform the reflected direction to the portal being moved from and then the portal being moved to
        Vector3 transformedDir = ConnectedPortal.transform.InverseTransformDirection(reflectedDir);
        transformedDir = transform.TransformDirection(transformedDir);

        //calcualte the new reflected direction off of the portal that the player will be at
        Vector3 normalizedDir = ((transform.position + offset) - (transform.position + offset + transformedDir)).normalized;
        Vector3 newReflectedVector = Vector3.Reflect(normalizedDir, transform.forward);

        //assign the values to move the player
        objectToTeleport.transform.position = transform.position + offset + transform.forward * 0.4f;
        objectToTeleport.transform.forward = newReflectedVector;

        //debug drawz
        Debug.DrawLine(transform.position, transform.position + (newReflectedVector) * 3.0f, Color.magenta, 30.0f);
        Debug.DrawLine(transform.position, transform.position + (transformedDir * 3.0f), Color.red, 30.0f);
        Debug.DrawLine(transform.position, transform.position + (transform.forward * 3.0f), Color.blue, 30.0f);

        if (this.Room.IsRoomCleared)
            this.Room.TogglePortalFXLayer(true);
    }

    public void DeactivatePortal()
    {
        DisablePortalSound();
        //disable the collider right away so that the portal is no longer traversable
        m_Collider.enabled = false;
        m_Renderer.enabled = false;
        //check to make sure the timer is less then the aloted time of the disapearing effect
        if (m_DisapearingTimer < 2.0f)
        {
            //make it so the portal implodes

            //increment the timer then check to see if it has gone over its given time
            m_DisapearingTimer += Time.deltaTime;
            if (m_DisapearingTimer >= 2.0f)
            {
                SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "PortalVanish"); ////Play Portal deactivation sound effect
                //change the shader back to the normal one and turn off the renderer for the portal so it doesn't render
            }
        }
    }

    public void ReactivatePortal()
    {
        //TEMPORARY VALUE UNTIL PORTAL DISAPPEARING IS FINALIZED
        m_Collider.enabled = true;
        m_Renderer.enabled = true;
        //check to make sure the timer is less then the aloted time of the reappearing effect
        if (m_DisapearingTimer < 2.0f)
        {
            //make it so the portal goes back to its normal form

            m_DisapearingTimer += Time.deltaTime;

            if (m_DisapearingTimer >= 2.0f)
            {
                //reactivate the collider so players may move through the portal again
                m_Collider.enabled = true;
                SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "PortalAppear", transform.position); //Play Portal Activation sound effect
                EnablePortalSound();
            }
        }
    }

    public void SetPortalEffectLayer()
    {
        if (m_PortalEffect != null && ConnectedPortal != null && RoomManager.Instance != null)
        {
            m_PortalEffect.SetMateralTexture(ConnectedPortal.RenderTarget.RenderTexture);
            m_PortalEffect.SetMaterialColor(RoomManager.Instance.GetColorFromRoomType(ConnectedRoom.TypeOfRoom));
        }
    }

    private void DisablePortalSound()
    {
        if (m_PortalSFX != null)
        {
            m_PortalSFX.spatialBlend = 1f;
            SoundManager.StopSFXLooped(m_PortalSFX);
            m_PortalSFX = null;
        }

    }
    private void EnablePortalSound()
    {

        m_PortalSFX = SoundManager.PlaySFXLooped("PortalAmbient");
        m_PortalSFX.gameObject.transform.position = transform.position;

        if (m_PortalSFX != null)
            m_PortalSFX.spatialBlend = 1.0f;

    }

    public Portal ConnectedPortal { get { return m_ConnectedPortal; } set { m_ConnectedPortal = value; } }
    public bool IsActiveInRoom { get { return m_IsActiveInRoom; } set { m_IsActiveInRoom = value; } }
    public bool IsInUse { get { return m_IsInUse; } set { m_IsInUse = value; } }
    public RenderTarget RenderTarget { get { return m_RenderTarget; } set { m_RenderTarget = value; } }
    public Renderer Renderer { get { return m_Renderer; } set { m_Renderer = value; } }
    public IslandRoom Room { get { return m_ParentRoom; } set { m_ParentRoom = value; } }
    public Camera PortalCamera { get { return m_PortalCamera; } }
    public IslandRoom ConnectedRoom { get { return m_ConnectedRoom; } set { m_ConnectedRoom = value; } }
    public PortalEffectLayer PortalEffect { get { return m_PortalEffect; } }
    public float CameraMinHeight { get { return m_CameraMinHeight; } }
    public float CameraMaxHeight { get { return m_CameraMaxHeight; } }
}
