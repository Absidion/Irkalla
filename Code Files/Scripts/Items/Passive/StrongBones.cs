using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.Items
{
    public class StrongBones : AbstractPassive
    {
        public StrongBones()
        {
            m_Description = "It gives an increase to Defense and Damage";
            m_Name = "Strong Bones";
            m_SpriteName += "StrongBonesAbility";
            m_Tooltip = "Increased defense/damage";
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
            player.IncreaseStat(StatType.DEFENSE, 5);
            player.IncreaseStat(StatType.DAMAGE, 2);

            Debug.Log("+5 Defense added to the Player");
            Debug.Log("+2 Damage added to the Player");
        }
    }
}
