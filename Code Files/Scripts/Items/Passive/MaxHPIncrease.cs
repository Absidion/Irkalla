using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.Items
{
    public class MaxHPIncrease : AbstractPassive
    {
        public MaxHPIncrease()
        {
            m_Description = "It gives an increase to max HP";
            m_Name = "SUPER HEALTH";
            m_SpriteName += "HeartPickUp";
            m_Tooltip = "Increased max HP";
        }

        public override void UpdateItem()
        {
            
        }

        public override void UpdateNetworkAffect()
        {
          
        }

        public override void UpdateNetworkVisual()
        {
            
        }

        public override void ActivateItem(Player player)
        {
            player.Health.ChangeMaxHP(50);

            Debug.Log("50 hp added to player");
        }

    }
}
