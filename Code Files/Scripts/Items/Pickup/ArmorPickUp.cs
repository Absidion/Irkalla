using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.Items
{
    public class ArmorPickUp : AbstractItem
    {
        private int m_ArmorValue = 20;

        public ArmorPickUp()
        {
            m_SpriteName += "ArmourConsumable";
            m_Name = "Armor";
            m_Description = "Some armor";
            m_NetworkAffect = false;
            m_NetworkVisual = false;            
            m_ItemType = ItemType.PickUp;
        }

        public override bool CanObtainItem(Player player)
        {
            //check if the player's armor value is less then the maximum value
            if(player.Health.ArmorHP != player.Health.MaxArmorHP)
            {
                return true;
            }

            return false;
        }

        public override void ActivateItem(Player player)
        {
            player.Health.AddArmorHP(m_ArmorValue);
            SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "ArmorGain", player.transform.position);
        }
    }
}
