using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: James
//Last edited: James 11/12/2017
namespace TheNegative.Items
{
    public class DoubleJumpBoots : AbstractPassive
    {
        public DoubleJumpBoots()
        {
            m_NeedsUpdate = false;
            m_Name += "DoubleJumpBoots";
            m_Description = "A pair of booties that gives the player an additional jump!";
            m_Tooltip = "Double jumps";
            m_SpriteName = "DoubleJumpBoots";
            m_NetworkAffect = false;
            m_NetworkVisual = false;
        }

        public override void ActivateItem(Player player)
        {

            player.GetComponent<Movement>().AddJump();
        }
    }
}
