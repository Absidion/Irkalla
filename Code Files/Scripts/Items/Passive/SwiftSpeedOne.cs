using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.Items
{
    public class SwiftSpeedOne : AbstractPassive
    {
        public SwiftSpeedOne()
        {
            m_Description = "It gives an increase to Movement Speed";
            m_Name = "SUPER SWIFT";
            m_SpriteName += "SwiftSpeedOne";
            m_Tooltip = "Increased movement speed";
        }
        public override void ActivateItem(Player player)
        {
            player.IncreaseStat(StatType.MOVEMENT_SPEED, 1);

            Debug.Log("+2 Movement added to the Player");
        }
    }
}
