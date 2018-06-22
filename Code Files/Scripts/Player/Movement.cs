using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//author: Liam
//last edited: Josue 11/08/2017

public class Movement : SyncBehaviour
{
    public float Speed;                     //the Speed value of anything with a movement script   
    public float MaxSpeed;                  //the maximum movement speed of any gameobject with this script attached to them
    public float JumpHeight;                //the jump height of any gameobject with this script attached
    public float TimeUntilNextJump = 0.2f;  //time until the gameobject can jump again
    public float JumpTimer = 2f;
    public GameObject FootLocationObj;      //the location of the objects foot so it can use the StepUp code
    public Animator AnimatorRef;            //reference to the animator class of the gameobject
    public PhysicMaterial JumpMaterial;     //The material that gets added to the object when it's jumping

    [SerializeField]
    private float m_JumpSlope = 0.4f;                   //value which is used to reset jump. This value represents the angle at which something that has this script can jump at off of other surfaces
    private Vector3 m_JumpVector = new Vector3(0, 5, 0);
    private bool m_IsJumping = false;                   //whether or not this object is jumping.
    private bool m_IsLanding = false;                   //whether this object is landing after falling or not
    private bool m_IsInAir = false;                     //whether this object is currently in the air
    private float m_TimeInAir = 0.0f;                   //amount of time this object has spent in the air
    private bool m_IsSprinting = false;                 //whether this object is sprinting or not
    private List<bool> m_Jumps;                         //number of availible jumps
    public Rigidbody m_RigidBody;                       //reference to the object's rigidbody
    private int m_GroundCheckMask;                      //a layer mask used to in the stepup code to move an object up staris
    private Vector3 m_GroundNormal;                     //the normal of the ground we're colliding on
    private float m_PercentageAmountSlowed = 0.0f;      //percentage amount the speed will be slowed by
    private float m_OldPercentageAmountSlowed = 0.0f;   //every time amount slowed changes, it gets compared to this to notify that the speed needs to be changed
    private float m_SpeedRemoved = 0.0f;                //stores the flat amount of speed removed from the player so we know the original speed before being slowed

    private Collider m_ObjectCollider = null;       //The collider for the object
    private AudioSource m_WalkingSFX;               //Audiosource for the walking SoundEffect
    private AudioSource m_RunningSFX;               //Audiosource for the Running SoundEffect
    private bool isMoving = false;                  //This bool is used so we don't keep trying to reobtain a audiosource every frame to play the walking sound.
    public float AmountSlowed { get { return m_PercentageAmountSlowed; } set { m_PercentageAmountSlowed = value; } }
    public bool IsSprinting { get { return m_IsSprinting; } set { m_IsSprinting = value; } }

    public void Init()
    {
        m_Jumps = new List<bool>();
        m_Jumps.Add(true);

        //if the speed is higher then maxspeed then set spedd to maxspeed
        if (Speed > MaxSpeed)
        {
            Speed = MaxSpeed;
        }

        //set up the rigidbody component
        m_RigidBody = GetComponent<Rigidbody>();
        if (m_RigidBody == null)
        {
            Debug.LogError("Movement component failed to get rigidbody of gameobject it's attached to. Please add a rigidbody to this object. ", this);
        }

        m_GroundCheckMask = LayerMask.GetMask("Stairs");

        m_ObjectCollider = GetComponent<Collider>();
    }

    public void UpdatePosition(Vector3 moveDir, bool jumpKeyActive, bool sprintKeyActive)
    {
        UpdateSlowAmount();

        if (moveDir == Vector3.zero && isMoving)
        {
            DisableWalkingSound();
        }
        else if (!isMoving && (moveDir != Vector3.zero))
        {
            EnableWalkingSound();
        }

        //affect movement speed based on if player pressed/released sprint
        if (sprintKeyActive)
        {
            DisableWalkingSound();
            if (!m_IsSprinting)
            {
                EnableRunningSound();
                m_IsSprinting = true;
            }
        }
        else
        {
            if (m_IsSprinting)
            {
                DisableRunningSound();
                m_IsSprinting = false;
            }
        }

        float speedPlusSprint = Speed;
        if (m_IsSprinting)
            speedPlusSprint *= 1.75f;

        Vector3 TargetVelocity = new Vector3(moveDir.x, 0, moveDir.z);
        TargetVelocity = transform.TransformDirection(TargetVelocity);
        TargetVelocity *= speedPlusSprint;
        TargetVelocity.y = 0;

        Vector3 velocity = m_RigidBody.velocity;
        Vector3 VelocityChange = (TargetVelocity - velocity);
        VelocityChange.x = Mathf.Clamp(VelocityChange.x, -speedPlusSprint, speedPlusSprint);
        VelocityChange.z = Mathf.Clamp(VelocityChange.z, -speedPlusSprint, speedPlusSprint);
        VelocityChange.y = 0;

        m_RigidBody.AddForce(VelocityChange, ForceMode.VelocityChange);

        if (jumpKeyActive)
        {
            UpdateJumps(velocity);
        }

        if (m_RigidBody.velocity.y > 3 || m_RigidBody.velocity.y < -0.15)
        {
            Vector3 vel = m_RigidBody.velocity;
            vel.y -= (JumpHeight * Time.deltaTime) * 2;
            m_RigidBody.velocity = vel;
        }

        if (AnimatorRef != null)
            //update the movement animations after all the calculations have been done
            UpdateMovementAnimations(moveDir);
    }

    public void UpdateJumps(Vector3 velocity)
    {
        bool canJump = false;
        for (int i = 0; i < m_Jumps.Count; i++)
        {
            if (m_Jumps[i])
            {
                canJump = true;
                m_Jumps[i] = false;
                SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "Generic_Jump", transform.position);
                break;
            }
        }

        if (canJump)
        {
            m_ObjectCollider.material = JumpMaterial;
            m_IsInAir = true;
            m_IsJumping = true; //set to true to play jump animation
            m_IsLanding = false;
            AnimatorRef.SetBool("IsJumping", m_IsJumping);
            AnimatorRef.SetBool("InAir", m_IsInAir);
            AnimatorRef.SetBool("IsLanding", m_IsLanding);
            //m_RigidBody.velocity = new Vector3(velocity.x, JumpHeight, velocity.z);
            m_RigidBody.AddForce(new Vector3(velocity.x, JumpHeight, velocity.z), ForceMode.VelocityChange);
        }

    }

    private void DisableWalkingSound()
    {
        if (m_WalkingSFX != null)
        {
            m_WalkingSFX.spatialBlend = 1f;
            SoundManager.StopSFXLooped(m_WalkingSFX);
            m_WalkingSFX = null;
        }
        isMoving = false;
    }
    private void EnableWalkingSound()
    {
        m_WalkingSFX = SoundManager.PlaySFXLooped("Generic_Walk");
        if (m_WalkingSFX != null)
            m_WalkingSFX.spatialBlend = 0f;
        isMoving = true;
    }

    private void DisableRunningSound()
    {
        if (m_RunningSFX != null)
        {
            m_RunningSFX.spatialBlend = 1f;
            SoundManager.StopSFXLooped(m_RunningSFX);
            m_RunningSFX = null;
        }

    }
    private void EnableRunningSound()
    {
        m_RunningSFX = SoundManager.PlaySFXLooped("GenericRun");
        if (m_RunningSFX != null)
            m_RunningSFX.spatialBlend = 0f;

    }

    public void UpdateRotation(Quaternion rotation)
    {
        rotation.eulerAngles += m_RigidBody.rotation.eulerAngles;       //add on to the rotation's eulerangles the rigidbody's rotation eulerangles        
        m_RigidBody.MoveRotation(rotation);                             //apply the rotation to the rigidbody
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (photonView.isMine)
        {
            //loop through all of the contact points inside of the collision contacts
            foreach (ContactPoint contact in collision.contacts)
            {
                //check to see if the contact normal is  greater then jump slope
                if (contact.normal.y > m_JumpSlope)
                {
                    if (m_Jumps != null)
                    {
                        for (int i = 0; i < m_Jumps.Count; i++)
                        {
                            m_Jumps[i] = true;
                        }
                    }

                    //reset in air variables
                    m_IsJumping = false;
                    AnimatorRef.SetBool("IsJumping", m_IsJumping);

                    //start landing animation
                    m_IsLanding = true;
                    AnimatorRef.SetBool("IsLanding", m_IsLanding);

                    //reset physics material
                    m_ObjectCollider.material = null;
                }
            }
        }
    }

    //Update the speed whenever the slow amount has been changed
    private void UpdateSlowAmount()
    {
        //if the slow amount has changed
        if (m_PercentageAmountSlowed != m_OldPercentageAmountSlowed)
        {
            if (m_PercentageAmountSlowed > m_OldPercentageAmountSlowed)
            {
                float slowPercentage = m_PercentageAmountSlowed - m_OldPercentageAmountSlowed;

                float slowAmount = 0.0f;
                if (m_SpeedRemoved == 0.0f)
                {
                    slowAmount = Speed * slowPercentage;
                    m_SpeedRemoved = slowAmount;
                }
                else
                {
                    slowAmount = (m_SpeedRemoved + Speed) * slowPercentage;
                    m_SpeedRemoved = slowAmount;
                }

                Speed -= slowAmount;                                      //if the slow amount has increased, decrease speed more by the difference
                m_OldPercentageAmountSlowed = m_PercentageAmountSlowed;   //update the old slow amount
            }
            else if (m_PercentageAmountSlowed < m_OldPercentageAmountSlowed)
            {
                float slowPercentage = m_OldPercentageAmountSlowed - m_PercentageAmountSlowed;
                float slowAmount = (m_SpeedRemoved + Speed) * slowPercentage;

                Speed += slowAmount;                                        //if the slow amount has decreased, increase speed more by the difference
                m_SpeedRemoved -= slowAmount;
                m_OldPercentageAmountSlowed = m_PercentageAmountSlowed;     //update the old slow amount
            }
        }
    }

    protected void StepUp()
    {
        float CheckForGroundRadius = 1.0f;
        float GroundCheckStartOffsetY = 0.5f;
        //float GroundResolutionOverlap = 0.05f;
        float m_CenterHeight = transform.position.y;
        //Check for the ground below the player
        float footHeight = FootLocationObj.transform.position.y;
        float halfCapsuleHeight = m_CenterHeight - footHeight;
        // get start of raycast
        Vector3 rayStart = transform.position;
        rayStart.y += GroundCheckStartOffsetY;
        // get direction and distance of raycast
        Vector3 rayDir = Vector3.down;
        float rayDist = halfCapsuleHeight + GroundCheckStartOffsetY - CheckForGroundRadius;
        RaycastHit groundHitInfo;
        //Physics.SphereCast(rayStart, CheckForGroundRadius, rayDir, out groundHitInfo, rayDist);
        //string tag = groundHitInfo.collider.tag;
        //int layer = groundHitInfo.collider.gameObject.layer;
        if (Physics.SphereCast(rayStart, CheckForGroundRadius, rayDir, out groundHitInfo, rayDist, m_GroundCheckMask))
        {
            //step-up
            Vector3 bottomAtHitPoint = MathFunc.ProjectToBottomOfCapsule(groundHitInfo.point, transform.position, halfCapsuleHeight * 2.0f, CheckForGroundRadius);
            float stepUpAmount = groundHitInfo.point.y - bottomAtHitPoint.y;
            m_CenterHeight += stepUpAmount; //- GroundResolutionOverlap;
            Vector3 playerCenter = transform.position;
            playerCenter.y = m_CenterHeight;
            transform.position = playerCenter;
        }

        m_GroundNormal = groundHitInfo.normal;
    }

    public void AddJump()
    {
        m_Jumps.Add(true);
    }

    //Update and set all animation properties related to movement
    private void UpdateMovementAnimations(Vector3 moveDir)
    {
        //lerp movement floats so blend tree is smooth
        float currentFDirection = AnimatorRef.GetFloat("ForwardDirection");
        float currentRDirection = AnimatorRef.GetFloat("RightDirection");

        //forward direction
        if (MathFunc.AlmostEquals(currentFDirection, moveDir.z))
            currentFDirection = moveDir.z;
        else if (moveDir.z < currentFDirection)
            currentFDirection -= Time.deltaTime * 2.0f;
        else if (moveDir.z > currentFDirection)
            currentFDirection += Time.deltaTime * 2.0f;

        //left/right direction
        if (MathFunc.AlmostEquals(currentRDirection, moveDir.x))
            currentRDirection = moveDir.x;
        else if (moveDir.x < currentRDirection)
            currentRDirection -= Time.deltaTime * 2.0f;
        else if (moveDir.x > currentRDirection)
            currentRDirection += Time.deltaTime * 2.0f;

        if (m_IsSprinting)
        {
            AnimatorRef.SetFloat("ForwardDirection", currentFDirection);
        }
        else
        {
            AnimatorRef.SetFloat("ForwardDirection", Mathf.Clamp(currentFDirection, -1.0f, 0.5f));
        }

        AnimatorRef.SetFloat("RightDirection", currentRDirection);

        AnimatorRef.SetBool("InAir", m_IsInAir);

        //if true, set animation for jumping off the ground
        AnimatorRef.SetBool("IsJumping", m_IsJumping);

        //if true, set animation for landing on the ground
        AnimatorRef.SetBool("IsLanding", m_IsLanding);
    }

    public void DisableMovementFX()
    {
        AnimatorRef.SetFloat("ForwardDirection", 0.0f);
        AnimatorRef.SetFloat("RightDirection", 0.0f);

        DisableRunningSound();
        DisableWalkingSound();
    }

    //Function gets called at the end of landing animation
    public void EventFinishedLanding()
    {
        m_IsLanding = false;

        if (m_IsJumping == false)
            m_IsInAir = false;

        SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "GenericLand", transform.position);
    }
}
