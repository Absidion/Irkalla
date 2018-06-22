using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.Items
{
    public class ToughSkinOne : AbstractPassive
    {
        public ToughSkinOne()
        {
            m_Description = "It gives an increase to Defense!";
            m_Name = "Tough Skin";
            m_SpriteName += "ToughSkinOne";
            m_Tooltip = "Increased defense";
        }

        public override void ActivateItem(Player player)
        {
            player.IncreaseStat(StatType.DEFENSE, 2);

            Debug.Log("+2 Defense added to the Player");
        }
    }
}
