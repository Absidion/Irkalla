using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Writer: Liam
//Last Updated: Liam 11/30/2017

namespace TheNegative.Items
{
    public class ShopItemMesh : ItemMesh
    {
        private float m_BuyCooldown = 1.0f;

        private int m_Value = 0;
        private float m_BuyTimer = 0.0f;

        private bool CanPlayerAffordItem(Player player)
        {
            if (player.GetCurrency() >= m_Value)
            {
                player.ChangeCurrency(-m_Value);
                SoundManager.GetInstance().photonView.RPC("PlaySFXNetworked", PhotonTargets.All, "BoughtItem", gameObject.transform.position);
                return true;
            }

            return false;
        }

        [PunRPC]
        public void ItemSetUp(ItemType type, string assemblyName, int value, string nameAndDirectory)
        {
            m_Value = value;
            ItemSetUp(type, assemblyName, nameAndDirectory);
        }

        protected override void Update()
        {
            base.Update();

            if (m_ItemCollected)
            {
                
                if (m_BuyTimer < m_BuyCooldown)
                {
                    m_BuyTimer += Time.deltaTime;
                }
                else if (m_BuyTimer >= m_BuyCooldown)
                {
                    m_BuyTimer = 0.0f;
                    m_ItemCollected = false;
                }               
            }
        }

        protected override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);

            if (other.tag == "Player")
            {
                //get the player component from the collider
                Player player = other.GetComponent<Player>();

                if (player.photonView.isMine)
                {
                    //TODO: get item m_name instead of assembly name
                    string itemName = gameObject.name.Replace("TheNegative.Items.", "");
                    PlayerHUDManager.instance.ToggleShopWindow(true, itemName, Value.ToString());
                }
            }
        }

        protected override void OnTriggerStay(Collider other)
        {
            Player player = other.GetComponent<Player>();

            if (player != null)
            {
                if (player.photonView.isMine && !m_ItemCollected && CharacterController.GetInteractHeld())
                {
                    if (CanPlayerObtainItem == null)
                    {
                        photonView.RPC("RPCGetItemDelegate", PhotonTargets.All);
                    }

                    if (!CanPlayerObtainItem(player))
                        return;

                    if (!CanPlayerAffordItem(player))
                        return;
                    //
                    if (m_Type == ItemType.PickUp)
                    {
                        m_ItemCollected = true;
                        ItemManager.Instance.AddItemToPlayerInvertoryByName(player.PlayerNumber, m_AssemblyName, m_Type);
                    }
                    else
                    {
                        photonView.RPC("AddItemToPlayerAndDeleteItem", PhotonTargets.All, player.PlayerNumber);
                        PlayerHUDManager.instance.ToggleInteractImage(false);
                        PlayerHUDManager.instance.ToggleShopWindow(false);
                    }
                }
            }
        }

        protected override void OnTriggerExit(Collider other)
        {
            base.OnTriggerExit(other);

            if (other.tag == "Player")
            {
                //get the player component from the collider
                Player player = other.GetComponent<Player>();

                if (player.photonView.isMine)
                    PlayerHUDManager.instance.ToggleShopWindow(false);
            }
        }

        public int Value { get { return m_Value; } set { m_Value = value; } }
    }
}
