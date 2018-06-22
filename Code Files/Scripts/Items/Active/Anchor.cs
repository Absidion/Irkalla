using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: James
//Last edited: James 12/1/2017
namespace TheNegative.Items
{
    public class Anchor : AbstractActiveTimerBased
    {
        private GameObject m_AnchorProjectile;              //When the Item is picked up, this stores a copy of the projectile from the Prefab in Resources.
        private GameObject m_ActiveAnchor;                  //A reference to the currently active Anchor Object        
        private string m_AnchorPool = "Anchors";            //Name of te BlackHolePool
        private bool m_ReadyForUse = true;                  //This boolean is used to determine if the item is ready to use.

        public Anchor()
        {
            m_Name = "Anchor";
            m_SpriteName += "Anchor";
            m_Description = "Throw an Anchor! If it hits an enemy, pull them towards you.";
            m_Tooltip = "Pull enemies closer";
            m_CooldownTime = 30.0f;
            m_Timer = 0.0f;
            m_IsActivated = false;
            m_ReadyForUse = true;
            m_AnchorProjectile = (GameObject)Resources.Load("Items/Meshes/Anchor");
            ObjectPoolManager.Instance.CreateNetworkPoolWithName(m_AnchorPool, "Items/Meshes/" + m_AnchorProjectile.name, 2, 3, true);

        }

        public override void ActivateItem(Player player)
        {          
            if (m_ReadyForUse)
            {
                base.ActivateItem(player);
                m_Timer = m_CooldownTime;
            }
        }

        public override void UpdateItem()
        {
            if (!m_IsActivated && m_Timer >= m_CooldownTime && m_ReadyForUse)
            {
                m_ActiveAnchor = ObjectPoolManager.Instance.GetObjectFromNetworkPool(m_AnchorPool);

                Vector3 direction = m_Player.EyeLocation.transform.forward;

                m_ActiveAnchor.GetPhotonView().RPC("FireProjectile", PhotonTargets.All, m_Player.EyeLocation.transform.position, direction, m_Player.PlayerNumber, m_Player.WeaponLayerMask, 0, null);

                m_IsActivated = true;
                ResetTimer();
            }

            if (m_Timer >= m_CooldownTime)
            {
                m_IsActivated = false;
                m_ReadyForUse = true;
            }
        }
    }
}
