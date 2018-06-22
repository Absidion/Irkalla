using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheNegative.Items
{
    public class HeartPickUp : AbstractItem
    {
        private int m_RecoveryValue = 20;

        public HeartPickUp()
        {
            m_Name = "Heart";
            m_Description = "Recover some health my dude";
            m_SpriteName += "HeartPickUp";
            m_NetworkAffect = false;
            m_NetworkVisual = false;
            m_ItemType = ItemType.PickUp;
        }

        public override bool CanObtainItem(Player player)
        {
            if(player.Health.HP < player.Health.MaxHp)
            {
                return true;
            }

            return false;
        }

        public override void ActivateItem(Player player)
        {
            player.Health.HP += m_RecoveryValue;
            if(player.Health.HP > player.Health.MaxHp)
            {
                player.Health.HP = player.Health.MaxHp;
                SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "HealthRestored", player.transform.position);
            }
        }
    }
}