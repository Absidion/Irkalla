using System;
using UnityEngine;

namespace TheNegative.AI.Node
{
    public class DigNode : Node
    {
        private float m_AttackCooldown = 10.0f;                        //cooldown of this attack
        private float m_DigSpeed = 2.0f;                               //the speed at which the nerg digs
        private int m_Damage = 10;                                     //damage when the nerg pops out of the ground
        private float m_TimeSpentUnderground = 2.0f;                   //the time that the entity will spend underground

        private CurrentStep m_CurrentStep = CurrentStep.Step1MoveUnderGround;

        private float m_AttackTimer = 0.0f;                         //the timer for how long since the last time this attack was used
        private float m_UnderGroundTimer = 0.0f;                    //the timer for how long the AI has been underground
        private float m_WaitTimer = 0.0f;                           //how long the entity will wait after reamerging before reentering the ground

        private Vector3 m_MoveLocation = Vector3.zero;              //the location the entity is moving to

        private bool m_IsAttacking = false;                         //determines whether or not the enemy is in the actual damage state of the attack
        private bool m_IsInitialized = false;                       //whether or not the node is setup

        private int m_DigLayerMask;   //layer mask so the raycast doesn't hit own hitbox

        private const float m_ExtraDepth = 3.0f;
        private const float m_WaitTime = 3.0f;

        public DigNode(AI reference) : base(reference)
        {
            m_DigLayerMask = ~(LayerMask.GetMask("Enemy") | LayerMask.GetMask("Room"));
        }

        public override void Init()
        {
            m_AIReference.Agent.enabled = false;
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            if (m_AttackTimer > m_AttackCooldown)
            {
                if (!m_IsInitialized)
                {
                    SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "NergDig", m_AIReference.transform.position);
                    //get the closest ground position 
                    m_MoveLocation = MathFunc.CalculateClosestGroundPosition(m_AIReference.transform.position);
                    //make sure the location to move to is a little bit lower so the nerg goes completely underground
                    m_MoveLocation.y -= m_ExtraDepth;

                    m_IsInitialized = true;
                }

                return RunDigLogic();
            }

            return BehaviourState.Failed;
        }

        public override void LateUpdate()
        {
            m_AttackTimer += Time.deltaTime;
        }

        public BehaviourState RunDigLogic()
        {
            switch (m_CurrentStep)
            {
                //move the nerg underground
                case CurrentStep.Step1MoveUnderGround:

                    MoveToLocation();
                    break;

                //move underneith the player's position
                case CurrentStep.Step2MoveToPlayerPos:

                    if (m_UnderGroundTimer > m_TimeSpentUnderground)
                    {
                        //increment the step count
                        m_CurrentStep++;
                        //set the location that the entity will need to emerge from
                        m_MoveLocation = m_AIReference.Target.transform.position;
                        m_UnderGroundTimer = 0.0f;
                        //set the entity to be attacking that way while it's emerging it can deal damage
                        m_AIReference.photonView.RPC("SetIsAttacking", PhotonTargets.All, true);
                    }
                    else
                    {
                        m_UnderGroundTimer += Time.deltaTime;
                    }

                    //until
                    if (m_UnderGroundTimer < (m_TimeSpentUnderground - 1))
                    {
                        Vector3 newLoc = m_AIReference.Target.transform.position;
                        newLoc.y -= m_ExtraDepth;
                        m_AIReference.transform.position = newLoc;
                    }

                    break;

                //move out of the ground
                case CurrentStep.Step3EmergFromGround:

                    MoveToLocation();
                    break;

                //for a few moments and then re-enter the ground
                case CurrentStep.Step4ReEnterGround:

                    if (m_WaitTimer > m_WaitTime)
                    {
                        MoveToLocation();
                    }
                    else
                    {
                        m_WaitTimer += Time.deltaTime;
                        //once the timer has been increased passed the wait time we set the movelocation. We do this here because it's the only logical
                        //place we're we can and have the ability to
                        if (m_WaitTimer > m_WaitTime)
                        {
                            m_MoveLocation = MathFunc.CalculateClosestGroundPosition(m_AIReference.transform.position);
                            m_MoveLocation.y -= m_ExtraDepth;
                        }
                    }
                    break;

                //pick a random spawn point in the room and move the entity to that location
                case CurrentStep.Step5MoveToRandomSpawnPoint:

                    Vector3 newPos = m_MoveLocation = m_AIReference.MyIslandRoom.RoomSpawnPoints[UnityEngine.Random.Range(0, m_AIReference.MyIslandRoom.RoomSpawnPoints.Count)].transform.position;
                    newPos.y -= m_ExtraDepth;
                    m_AIReference.transform.position = newPos;
                    m_CurrentStep++;
                    break;

                //move the entity above the ground
                case CurrentStep.Step6MoveAboveGroundAndFinish:

                    MoveToLocation();
                    break;

                //reset all of the node values because, we're done
                case CurrentStep.Step7Done:

                    m_AttackTimer = 0.0f;
                    return BehaviourState.Succeed;
            }

            return BehaviourState.Running;
        }

        //moves the target to the location specified
        private void MoveToLocation()
        {
            Vector3 movementThisFrame = (m_MoveLocation - m_AIReference.transform.position).normalized * m_DigSpeed * Time.deltaTime;
            m_AIReference.transform.position += movementThisFrame;

            //check to see if the location being moved to is less then a small value. If it is then moving to the location is done and it can move to the next step
            if ((m_MoveLocation - m_AIReference.transform.position).magnitude < 0.1f)
            {
                //increment the step value
                m_CurrentStep++;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Player player = other.GetComponent<Player>();
            //if the player component of the object we collided with isn't null and the nerg is coming out of the ground
            if ((player != null) && m_IsAttacking)
            {
                if (PhotonNetwork.isMasterClient)
                {
                    //figure out if who ever we collided with is are target
                    if (m_AIReference.Target != player)
                    {
                        //if they weren't are target, they are now
                        m_AIReference.Target = player;
                    }
                }

                if (player.photonView.isMine)
                {
                    //Status[] statuses = { m_AIReference.ElementType, Status.Stun };
                    Status[] statuses = { Status.Stun };
                    player.TakeDamage(m_Damage, m_AIReference.transform.position ,statuses);
                }

                if (PhotonNetwork.isMasterClient)
                {
                    //set the player to be right at the player they collided with's mid area
                    m_AIReference.transform.position = m_AIReference.Target.transform.position;

                    //increment the current step
                    m_CurrentStep++;
                    m_MoveLocation = MathFunc.CalculateClosestGroundPosition(m_AIReference.transform.position);
                }
            }
        }

        public bool IsAttacking { get { return m_IsAttacking; } set { m_IsAttacking = value; } }
        public Vector3 MoveLocation { get { return m_MoveLocation; } set { m_MoveLocation = value; } }
        public CurrentStep DigNodeStep { get { return m_CurrentStep; } set { m_CurrentStep = value; } }
        public int Damage { get { return m_Damage; } set { m_Damage = value; } }

        //reset all values in the node that way the next time it's accessed all values are there default values
        public override void Reset()
        {
            m_UnderGroundTimer = 0.0f;
            m_WaitTimer = 0.0f;
            m_IsAttacking = false;
            m_IsInitialized = false;
            m_CurrentStep = CurrentStep.Step1MoveUnderGround;
        }

        public enum CurrentStep
        {
            Step1MoveUnderGround = 0,
            Step2MoveToPlayerPos = 1,
            Step3EmergFromGround = 2,
            Step4ReEnterGround = 3,
            Step5MoveToRandomSpawnPoint = 4,
            Step6MoveAboveGroundAndFinish = 5,
            Step7Done = 6
        }
    }
}