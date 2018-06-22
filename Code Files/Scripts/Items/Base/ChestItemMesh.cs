using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Writer : Liam
//Last Updated : Liam 11/30/2017

namespace TheNegative.Items
{
    public class ChestItemMesh : LiamBehaviour
    {
        public GameObject TreasureSpawnPoint;               //The position that loot will spawn from
        public float ChestLifeCycle = 20.0f;                //The life cycle of how long the chest will stay around before returning to the obejct pool

        private Animator m_Animator;                        //Reference to the chest's animator
        private float m_Timer = 0.0f;                       //Timer for how long the chest has been active
        private float m_PlayerLuck = 0.0f;                  //The stored player's luck value which is used to determine what is going to come out of the chest
        [SyncThis]
        protected bool m_WasLootCollected = false;                    //Determines if loot has been collected yet

        protected override void Awake()
        {
            base.Awake();
            m_Animator = GetComponent<Animator>();
            SetActive(false);
        }

        protected override void Update()
        {
            base.Update();

            if (!PhotonNetwork.isMasterClient || !m_WasLootCollected)
                return;

            m_Timer += Time.deltaTime;
            if (m_Timer > ChestLifeCycle)
                ResetChest();
        }


        private void OnTriggerStay(Collider other)
        {
            //get the player component from the collided object
            Player p = other.gameObject.GetComponent<Player>();

            if (p != null)
            {
                //make sure the photon view is yours and that interact has been pressed
                if (p.photonView.isMine && CharacterController.GetInteractHeld())
                {
                    photonView.RPC("ActivateChest", PhotonTargets.All);
                    m_PlayerLuck = p.GetStat(StatType.LUCK);
                }
            }
        }

        [PunRPC]
        private void ActivateChest()
        {
            m_Animator.SetBool("IsChestOpenning", true);
        }

        private void EventChestOpenned()
        {
            //once the animation event is fired the master client will spawn items into the world again
            if (photonView.isMine)
            {
                int index = Random.Range(2, 5);
                //spawn items from the chest
                for (int i = 0; i < index; i++)
                {
                    ItemManager.Instance.SpawnItemIntoWorld(TreasureSpawnPoint.transform.position, m_PlayerLuck, false);
                }
            }
            //tell the chest that loot has been spawned
            m_WasLootCollected = true;
        }

        private void ResetChest()
        {
            m_Timer = 0.0f;
            m_WasLootCollected = false;
            m_Animator.SetBool("IsChestOpenning", false);
            SetActive(false);
        }
    }
}