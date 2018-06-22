using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: James
//Last edited: James 12/1/2017
namespace TheNegative.Items
{
    public class BlackHole : AbstractActiveRoomBased
    {
        private GameObject m_BlackHoleProjectile;                   //When the Item is picked up, this stores a copy of the projectile from the Prefab in Resources.
        private string m_BlackHolePool = "BlackHoles";              //Name of te BlackHolePool
        private GameObject m_ActiveBlackHole;                       //A pool of Anchor Objects so we can easily disable and enable Anchors when using one.
        protected int m_RadiusOfPull = 30;                          //The radius used to check how far from the BlackHole we can detect Enemies and push them to it's location.        

        public BlackHole()
        {
            m_Name = "BlackHole";
            m_SpriteName += "BlackHole";
            m_Description = "A dark spherical object that when activated is thrown, and when activate again, pulls all enemies towards it.";
            m_Tooltip = "Suck enemies into the void";
            m_RoomCount = 1;
            m_RoomCooldown = 0;
            m_BlackHoleProjectile = (GameObject)Resources.Load("Items/Meshes/BlackHole");
            ObjectPoolManager.Instance.CreateNetworkPoolWithName(m_BlackHolePool, "Items/Meshes/" + m_BlackHoleProjectile.name, 2, 3, true);
        }

        public override void UpdateItem()
        {
            m_ActiveBlackHole = ObjectPoolManager.Instance.GetObjectFromNetworkPool(m_BlackHolePool);

            Vector3 direction = m_Player.ActiveItemLocation.transform.forward;
            Debug.DrawLine(m_Player.ActiveItemLocation.transform.position, m_Player.ActiveItemLocation.transform.position + (m_Player.ActiveItemLocation.transform.forward * 20.0f), Color.black, 20.0f);
            Vector3 launchposition = m_Player.ActiveItemLocation.transform.position;

            m_ActiveBlackHole.GetPhotonView().RPC("FireProjectile", PhotonTargets.All, launchposition, direction, m_Player.PlayerNumber, m_Player.WeaponLayerMask, 0, null);

            ResetCooldown();
        }
    }
}
