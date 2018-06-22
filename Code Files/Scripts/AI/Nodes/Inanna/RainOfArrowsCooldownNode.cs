using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.AI.Node
{
    public class RainOfArrowsCooldownNode : CooldownNode
    {
        private float m_Diameter = 0;           //The diameter in which the cooldown will decrease quicker because the players are closer togeather

        public RainOfArrowsCooldownNode(AI reference, float cooldownValue, float diameter) : base(reference, cooldownValue)
        {
            m_Diameter = diameter;
        }

        public override void LateUpdate()
        {
            //if the player count is greater then one then the cooldown can decrease at the faster rate
            if (m_AIReference.PlayerList.Count > 1)
            {
                //next if both players are close enough togeather then the cooldown will decrease twice as fast
                if ((m_AIReference.PlayerList[0].transform.position - m_AIReference.PlayerList[1].transform.position).magnitude < m_Diameter)
                    m_Timer += Time.deltaTime;
            }

            base.LateUpdate();
        }
    }
}