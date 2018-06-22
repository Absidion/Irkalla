using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Josue
//Last edited: 1/04/2018

namespace TheNegative.AI.Node
{
    public class GroundSpikesNode : Node
    {
        #region Variables/Properties/Constructor

        private GameObject m_SpikePrefab;                   //prefab for pool
        private float m_SpikeMovementTime = 0.5f;           //how long spikes take to lerp to their location

        private List<GroundSpike> m_ActiveSpikes;           //list of the current active spikes
        private float m_SpikeSpacing = 0.5f;                //how far the spikes are distanced from each other
        private float m_MinimumHeight = 0.0f;               //destination spike has to reach when moving downwards
        private float m_MaximumHeight = 0.0f;               //destination spike has to reach when moving upwards
        private Vector3 m_Direction = Vector3.zero;         //direction that the spikes are rotated to and moving towards
        private bool m_HitWall = false;                     //is the last spike hitting the wall or not?
        private int m_WallCheckLayerMask = 0;               //layer mask for the raycast that checks if there is a wall
        private Vector3 m_AIPosition = Vector3.zero;        //position of the AI at the time they did the attack
        private Vector3 m_TargetPosition = Vector3.zero;    //position of the target at the start of the attack

        private const string m_SpikePoolName = "Spikes";


        public GroundSpikesNode(AI reference, GameObject spikePrefab) : base(reference)
        {
            m_SpikePrefab = spikePrefab;

            Init();
        }

        public GroundSpikesNode(AI reference, GameObject spikesPrefab, float spikeMovementTime) : base(reference)
        {
            m_SpikePrefab = spikesPrefab;
            m_SpikeMovementTime = spikeMovementTime;

            Init();
        }

        public override void Init()
        {
            ObjectPoolManager.Instance.CreateNetworkPoolWithName(m_SpikePoolName, "Attacks/" + m_SpikePrefab.name, 10, 50, false);

            m_ActiveSpikes = new List<GroundSpike>();
            m_WallCheckLayerMask = LayerMask.GetMask("Wall");
            m_WallCheckLayerMask |= (1 << LayerMask.NameToLayer("Door"));
            m_WallCheckLayerMask |= (1 << LayerMask.NameToLayer("MapGeometry"));
        }

        #endregion

        #region Functions

        public override BehaviourState UpdateNodeBehaviour()
        {
            //set target so AI only rotates on Y axis
            Vector3 target = new Vector3(m_AIReference.Target.transform.position.x, m_AIReference.transform.position.y, m_AIReference.Target.transform.position.z);
            m_AIReference.transform.LookAt(target); //rotate towards the target
            SetUpSpikes();
            SoundManager.GetInstance().photonView.RPC("PlaySFXRandomizedPitchNetworked", PhotonTargets.All, "HitGround", m_AIReference.transform.position);
            return BehaviourState.Succeed;
        }

        public override void LateUpdate()
        {
            if (m_ActiveSpikes.Count != 0)
            {
                UpdateSpikes();
            }
        }

        public override void Stop()
        {
            if (PhotonNetwork.isMasterClient)
            {
                if (m_ActiveSpikes != null)
                {
                    for (int i = 0; i < m_ActiveSpikes.Count; i++)
                    {
                        m_ActiveSpikes[i].ResetValues();
                        m_ActiveSpikes[i].ToggleSpike(false);
                    }

                    m_ActiveSpikes.Clear();
                }
            }
        }

        //Set up the first three spikes at the beginning of the coroutine
        private void SetUpSpikes()
        {
            //reset hit wall variable
            m_HitWall = false;

            //set up direction and min/max height and ai and target position
            m_AIPosition = m_AIReference.transform.position;
            m_TargetPosition = m_AIReference.Target.transform.position;
            m_Direction = m_AIReference.transform.forward; //set the direction

            RaycastHit hit;

            if (Physics.Raycast(m_AIPosition, -(m_AIReference.transform.up), out hit, 100.0f, m_WallCheckLayerMask))
            {
                m_MinimumHeight = hit.point.y - 2.0f;
                m_MaximumHeight = hit.point.y + 0.5f;
            }
            else
            {
                Stop();
            }

            //setup the first spike
            GameObject spikeOne = ObjectPoolManager.Instance.GetObjectFromNetworkPool(m_SpikePoolName);

            if (spikeOne != null)
            {
                spikeOne.transform.position = m_AIPosition + (m_Direction * m_SpikeSpacing);                            //set the position of the spike to be spaced ahead in the direction the AI is facing
                spikeOne.transform.position = new Vector3(spikeOne.transform.position.x, m_MinimumHeight, spikeOne.transform.position.z);   //set the height of the spike to the minimum height

                //set target so spike only rotates on Y axis
                Vector3 target = new Vector3(m_AIPosition.x, spikeOne.transform.position.y, m_AIPosition.z);
                spikeOne.transform.LookAt(target); //rotate towards the target
                spikeOne.transform.Rotate(new Vector3(0, 90, 0));
                spikeOne.gameObject.GetComponent<GroundSpike>().ToggleSpike(true);

                m_ActiveSpikes.Add(spikeOne.GetComponent<GroundSpike>());
            }

            //setup the next two spikes the same way
            for (int i = 0; i < 2; i++)
            {
                GameObject spike = ObjectPoolManager.Instance.GetObjectFromNetworkPool(m_SpikePoolName);

                if (spike != null)
                {
                    spike.transform.position = m_ActiveSpikes[m_ActiveSpikes.Count - 1].transform.position + (m_Direction * m_SpikeSpacing); //each spike is spaced ahead of the last spike
                    spike.transform.rotation = m_ActiveSpikes[m_ActiveSpikes.Count - 1].transform.rotation;
                    spike.gameObject.GetComponent<GroundSpike>().ToggleSpike(true);
                    m_ActiveSpikes.Add(spike.GetComponent<GroundSpike>());
                }
            }
        }

        //Update the spikes by lerping their positions and spawning more spikes until they hit a wall
        private void UpdateSpikes()
        {
            float maxDistance = (m_ActiveSpikes[m_ActiveSpikes.Count - 1].transform.position - m_AIPosition).magnitude;

            //set the position of spikes on the ground every update loop
            SetSpikeGroundPosition();

            if (m_ActiveSpikes.Count == 0)
                return;

            //if we hit a wall with raycast, set the boolean
            if (Physics.Raycast(m_AIPosition, m_Direction, maxDistance, m_WallCheckLayerMask))
            {
                m_HitWall = true;
            }

            //loop through the active spikes and lerp them according to whether they are moving up or down
            for (int i = 0; i < m_ActiveSpikes.Count; i++)
            {
                if (m_ActiveSpikes[i].IsMovingUp)
                {
                    //lerp towards the max height over specified time given
                    m_ActiveSpikes[i].LerpTimer += Time.deltaTime;
                    float t = m_ActiveSpikes[i].LerpTimer / m_SpikeMovementTime;

                    Vector3 newPosition = m_ActiveSpikes[i].gameObject.transform.position;
                    newPosition.y = Mathf.Lerp(m_MinimumHeight, m_MaximumHeight, t);
                    m_ActiveSpikes[i].gameObject.transform.position = newPosition;
                }
                else if (!m_ActiveSpikes[i].IsMovingUp)
                {
                    //lerp towards the min height over specified time given
                    m_ActiveSpikes[i].LerpTimer += Time.deltaTime;
                    float t = m_ActiveSpikes[i].LerpTimer / m_SpikeMovementTime;

                    Vector3 newPosition = m_ActiveSpikes[i].gameObject.transform.position;
                    newPosition.y = Mathf.Lerp(m_MaximumHeight, m_MinimumHeight, t);
                    m_ActiveSpikes[i].gameObject.transform.position = newPosition;
                }
            }

            //if the first spike is moving up and has passed the maxmimum height
            if (m_ActiveSpikes[0].IsMovingUp && m_ActiveSpikes[0].gameObject.transform.position.y >= m_MaximumHeight)
            {
                m_ActiveSpikes[0].IsMovingUp = false; //set the spike to start moving down
                m_ActiveSpikes[0].LerpTimer = 0.0f;   //reset the lerp timer

                //if there is still room to spawn a spike, create another one
                if (m_HitWall == false)
                {
                    GameObject spike = ObjectPoolManager.Instance.GetObjectFromNetworkPool(m_SpikePoolName);

                    if (spike != null)
                    {
                        spike.transform.position = m_ActiveSpikes[m_ActiveSpikes.Count - 1].transform.position + (m_Direction * m_SpikeSpacing);
                        spike.transform.position = new Vector3(spike.transform.position.x, m_MinimumHeight, spike.transform.position.z);
                        spike.transform.rotation = m_ActiveSpikes[0].transform.rotation;
                        spike.GetComponent<GroundSpike>().ToggleSpike(true);
                        m_ActiveSpikes.Add(spike.GetComponent<GroundSpike>());
                    }
                }
            }
            else if (m_ActiveSpikes[0].IsMovingUp == false && m_ActiveSpikes[0].gameObject.transform.position.y <= m_MinimumHeight)
            {
                //if the first spike is moving down and has passed the minimum height
                m_ActiveSpikes[0].ToggleSpike(false); //return game object back to pool
                m_ActiveSpikes[0].ResetValues();      //reset variables for the next time this is used
                m_ActiveSpikes.RemoveAt(0);
            }
        }

        private void SetSpikeGroundPosition()
        {
            RaycastHit hit;
            Vector3 rayCastOrigin = m_ActiveSpikes[m_ActiveSpikes.Count - 1].transform.position;
            rayCastOrigin.y += 5.0f;

            if (Physics.Raycast(rayCastOrigin, Vector3.down, out hit, 100.0f, m_WallCheckLayerMask))
            {
                m_MinimumHeight = hit.point.y - 2.0f;
                m_MaximumHeight = hit.point.y + 0.5f;
            }
            else
            {
                Stop();
            }
        }

        #endregion
    }
}