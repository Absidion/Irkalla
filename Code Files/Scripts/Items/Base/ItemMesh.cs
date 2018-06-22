using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Writer: Liam
//Last Update: Liam 11/30/2017

namespace TheNegative.Items
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider))]
    public class ItemMesh : SyncBehaviour
    {
        [SerializeField]
        [SyncThis]
        protected string m_AssemblyName = "TheNegative.Items.";

        protected Sprite m_Sprite;
        protected SpriteRenderer m_Renderer;

        protected bool m_ItemCollected = false;

        [SerializeField]
        protected ItemType m_Type;

        protected float m_Timer = 0.0f;

        public CanObtainItem CanPlayerObtainItem;

        protected override void Awake()
        {
            base.Awake();

            m_Renderer = GetComponent<SpriteRenderer>();
            if (m_Renderer != null)
            {
                m_Renderer.sprite = m_Sprite;
            }            
        }

        protected virtual void Update()
        {
            if (m_Renderer != null)
            {
                transform.LookAt(Camera.main.transform);
            }

            m_Timer += Time.deltaTime;
        }

        [PunRPC]
        public void RPCGetItemDelegate()
        {
            CanPlayerObtainItem = ((AbstractItem)ItemManager.Instance.GetObjectFromName(m_AssemblyName)).CanObtainItem;
        }

        [PunRPC]
        public void AddItemToPlayerAndDeleteItem(int number)
        {
            m_ItemCollected = true;
            if (PhotonNetwork.isMasterClient)
            {
                ItemManager.Instance.AddItemToPlayerInvertoryByName(number, m_AssemblyName, m_Type);
                PhotonNetwork.Destroy(gameObject);
            }
        }

        //loads the sprite from an asset
        [PunRPC]
        public void ItemSetUp(ItemType type, string assemblyName, string nameAndDirectory)
        {
            if (m_Renderer != null)
            {
                m_Sprite = Resources.Load<Sprite>( nameAndDirectory);
                m_Renderer.sprite = m_Sprite;
            }
            
            m_Type = type;
            m_AssemblyName = assemblyName;
            gameObject.name = assemblyName;

            if (type != ItemType.PickUp)
                CanPlayerObtainItem = ItemManager.Instance.ItemsInGame[assemblyName].CanObtainItem;
            else
                CanPlayerObtainItem = ItemManager.Instance.PickUpItemsInGame[assemblyName].CanObtainItem;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                //get the player component from the collider
                Player player = other.GetComponent<Player>();

                if (player.photonView.isMine && m_Type != ItemType.PickUp)
                    PlayerHUDManager.instance.ToggleInteractImage(true);
            }
        }

        protected virtual void OnTriggerStay(Collider other)
        {
            //get the player component from the collider
            Player player = other.GetComponent<Player>();

            if (player.photonView.isMine && !m_ItemCollected)
            {
                if (!CanPlayerObtainItem(player) && m_Timer >= 2.0f)
                    return;

                if (m_Type != ItemType.PickUp)
                {
                    if (CharacterController.GetInteractHeld())
                    {
                        PlayerHUDManager.instance.ToggleInteractImage(false);
                        photonView.RPC("AddItemToPlayerAndDeleteItem", PhotonTargets.All, player.PlayerNumber);
                    }
                }
                else if (m_Type == ItemType.PickUp)
                {
                    photonView.RPC("AddItemToPlayerAndDeleteItem", PhotonTargets.All, player.PlayerNumber);
                }
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (other.tag == "Player")
            {
                //get the player component from the collider
                Player player = other.GetComponent<Player>();

                if (player.photonView.isMine)
                    PlayerHUDManager.instance.ToggleInteractImage(false);
            }
        }

        public string AssemblyName { get { return m_AssemblyName; } set { m_AssemblyName = value; } }
        public Sprite Sprite { get { return m_Sprite; } set { m_Sprite = value; } }
        public ItemType Type { get { return m_Type; } set { m_Type = value; } }
    }
}