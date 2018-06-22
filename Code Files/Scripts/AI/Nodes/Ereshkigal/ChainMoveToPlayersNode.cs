using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.AI.Node
{
    public class ChainMoveToPlayersNode : Node
    {
        private NetworkLineRenderer m_ChainLineRenderer = null;    //reference to line renderer that will display chain and move towards players
        private float m_TravelTime = 0.5f;                  //how much time the chain should take to travel towards the players
        private float[] m_ChainSpeeds;                      //varying speeds for both players that determines how far they move over a frame 
        private int[] m_PointIndexes;                       //stores indexes for end positions on the chain renderer
        private Transform m_StartPos = null;                //position at the tip of the spear where the chain starts
        private Dictionary<int, bool> m_PlayersHit;         //stores whether the player is hit or downed/dead

        public ChainMoveToPlayersNode(AI reference, Transform startPos, NetworkLineRenderer lineRenderer, float travelTime) : base(reference)
        {
            m_StartPos = startPos;
            m_ChainLineRenderer = lineRenderer;

            m_ChainSpeeds = new float[PhotonNetwork.playerList.Length];
            m_PointIndexes = new int[] { 1, 3 };
            m_PlayersHit = new Dictionary<int, bool>();
            m_TravelTime = travelTime;
        }

        public override void OnFirstTreeCall()
        {
            base.OnFirstTreeCall();           

            Vector3 pos = m_StartPos.position;
            pos.y = -21.1f;

            m_ChainLineRenderer.LineRenderer.SetPosition(0, m_StartPos.position);
            m_ChainLineRenderer.LineRenderer.SetPosition(1, m_StartPos.position);
            m_ChainLineRenderer.LineRenderer.SetPosition(2, m_StartPos.position);
            m_ChainLineRenderer.LineRenderer.SetPosition(3, m_StartPos.position);

            m_ChainLineRenderer.SetLineRendererEnabled(true);

            //iterate through each player to get initial direction and speed
            for (int i = 0; i < m_AIReference.PlayerList.Count; i++)
            {
                m_PlayersHit[m_AIReference.PlayerList[i].PlayerNumber] = false;
                m_ChainSpeeds[i] = (m_AIReference.PlayerList[i].transform.position - m_ChainLineRenderer.LineRenderer.GetPosition(m_PointIndexes[i])).magnitude / m_TravelTime;
            }
        }

        public override BehaviourState UpdateNodeBehaviour()
        {
            for (int i = 0; i < m_AIReference.PlayerList.Count; i++)
            {
                //skip any players we've already hit and players that are dead or downed
                if (m_AIReference.PlayerList[i].Health.IsDead || m_AIReference.PlayerList[i].IsDowned)
                {
                    if (m_PlayersHit[m_AIReference.PlayerList[i].PlayerNumber] == true)
                    {
                        continue;
                    }
                    else
                    {
                        m_PlayersHit[m_AIReference.PlayerList[i].PlayerNumber] = true;
                    }
                }

                Vector3 playerPosition = m_AIReference.PlayerList[i].transform.position;
                Vector3 chainPosition = m_ChainLineRenderer.LineRenderer.GetPosition(m_PointIndexes[i]);

                Vector3 displacement = m_AIReference.PlayerList[i].transform.position - chainPosition;
                Vector3 dir = displacement.normalized;
                //float distance = displacement.magnitude;

                //m_ChainSpeeds[i] = distance / m_TravelTime;

                chainPosition += dir * m_ChainSpeeds[i] * Time.deltaTime;
                m_ChainLineRenderer.LineRenderer.SetPosition(m_PointIndexes[i], chainPosition);

                Collider[] colliders = Physics.OverlapSphere(chainPosition, 3.0f, m_AIReference.TargetLayerMask);

                foreach (Collider collider in colliders)
                {
                    if (collider.gameObject == m_AIReference.PlayerList[i].gameObject)
                    {
                        m_ChainLineRenderer.LineRenderer.SetPosition(m_PointIndexes[i], m_AIReference.PlayerList[i].transform.position);
                        m_AIReference.PlayerList[i].SetVelocity(Vector3.zero);
                        m_AIReference.PlayerList[i].SetUseGravity(false);
                        m_AIReference.PlayerList[i].photonView.RPC("SetIsImmobile", PhotonTargets.All, true);
                        m_PlayersHit[m_AIReference.PlayerList[i].PlayerNumber] = true;
                    }
                }
            }

            float playersHit = 0;

            foreach (KeyValuePair<int, bool> player in m_PlayersHit)
            {
                if (player.Value)
                {
                    playersHit++;
                }
            }

            if (playersHit == m_PlayersHit.Count)
            {
                EreshkigalAI e = m_AIReference as EreshkigalAI;
                e.photonView.RPC("ActivateChainPull", PhotonTargets.All);
                m_ChainLineRenderer.SetLineRendererEnabled(false);                
                return BehaviourState.Succeed;
            }

            return BehaviourState.Running;
        }
    }
}
