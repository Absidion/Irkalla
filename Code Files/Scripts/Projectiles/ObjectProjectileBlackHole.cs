using System.Collections.Generic;
using UnityEngine;
using TheNegative.AI;

//Author: James
//Last edited: Liam 3/25/2018
public class ObjectProjectileBlackHole : ObjectProjectile
{
    public float PullAmount = 0.5f;                                 //The amount of pull that the blackhole has
    public float m_TimeTillActive = 3.0f;                           //Determines how long the projectile travels before becoming active   
    public float m_RadiusOfPull = 20.0f;                            //The radius used for the projectile to check for targets                                       

    protected Vector3 m_Displacement = Vector3.zero;                //The displacement between the Player and the target being Pulled
    private float m_DeactivateTimer = 2.0f;                         //This timer is used to set m_IsActivated to false after some time. We use a timer so we don't just set this to true and then false right afte.r 
    private static float m_MaxTimeActive = 7.5f;
    private List<AI> m_PulledAI;                                    //The AI that are being pulled toward the blackhole center position

    private bool m_Activated = false;                               //Determines if the blackhole is active yet.    
    private float m_ActiveTimer = 0.0f;

    protected override void Awake()
    {
        base.Awake();
        m_PulledAI = new List<AI>();
    }

    protected void OnDisable()
    {
        //reenable all navmeshes of ai that are being pulled toward the center before clearing the list of values
        foreach(AI ai in m_PulledAI)
        {
            if (ai != null)
            {
                ai.SetNavMeshAgent(true);
                ai.SetIsKinematic(true);
            }
        }

        m_PulledAI.Clear();
        m_Activated = false;      
        
    }

    protected override void Update()
    {
        base.Update();

        //if the item is activated and this is the master client then allow the master client to pull enemies into the center
        if (m_Activated && PhotonNetwork.isMasterClient)
        {
            Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, m_RadiusOfPull, LayerMask.GetMask("Enemy"));
            foreach (Collider collider in hitColliders)
            {
                if (collider.tag == "Enemy")
                {
                    AI ai = collider.transform.root.GetComponent<AI>();
                    //if the navmeshagent isn't disabled then change it to be disabled
                    if (ai.Agent)
                        ai.SetNavMeshAgent(false);
                    //if the AI is still kinematic then it won't be dragged towards the center
                    if (ai.rigidbody.isKinematic)
                        ai.SetIsKinematic(false);
                    //if the list of AI being pulled doesn't contain the current iterated AI then add it to the list of currently pulled AI
                    if (!m_PulledAI.Contains(ai))
                        m_PulledAI.Add(ai);
                    


                    Vector3 m_DirectionToBlackHole = (transform.position - ai.transform.position).normalized;
                    //apply the force to the AI                     
                    ai.ApplyForceOverNetwork(m_DirectionToBlackHole * PullAmount, ForceMode.Impulse);
                    //remove later
                    Debug.DrawLine(transform.position, ai.transform.position);
                }
            }

            //check to see if the time is up and the object shouldn't move anymore
            if (m_DeactivateTimer > 0.0f)
            {
                m_DeactivateTimer -= Time.deltaTime;
                if (m_DeactivateTimer < 0.0f)
                {
                    m_DeactivateTimer = m_MaxTimeActive;
                    SetActive(false);
                }
            }
        }

        //if the object isn't activated yet then determine how long it has left to travel before activating
        else
        {
            m_ActiveTimer += Time.deltaTime;
            if (m_ActiveTimer >= m_TimeTillActive)
            {
                m_Activated = true;
                m_ActiveTimer = 0.0f;
                //nullify the velocity of this object
                rigidbody.velocity = Vector3.zero;
            }
        }
    }
}
