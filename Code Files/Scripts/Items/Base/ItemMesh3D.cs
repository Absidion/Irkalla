using UnityEngine;

//Writer: Liam
//Last Updated: Liam 12/2/2017

namespace TheNegative.Items
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(PhotonView))]
    public class ItemMesh3D : LiamBehaviour
    {
        protected Sprite m_JournalSprite = null;              //The sprite for the journal version of the item
        protected string m_AssemblyName = string.Empty;       //The assembly name of this item
        protected ItemType m_ItemType = ItemType.NULL;        //The type of item that this mesh represents
        [SyncThis]
        protected bool m_ItemCollected = false;               //Determines if the item is collected or naw

        protected float m_Timer = 0.0f;                       //This timer delays instante pickup of the item from spawn. Only matters for pickup based items
        protected bool m_IsTreasureRoomItem = false;          //Determines if this item is a treasure room item

        public CanObtainItem CanPlayerObtainItem;

        protected override void Update()
        {
            base.Update();
            m_Timer += Time.deltaTime;

            Vector3 euler = transform.eulerAngles;
            euler.x = 0.0f;
            euler.y += Time.deltaTime * 20.0f;            

            transform.rotation = Quaternion.Euler(euler);            
        }

        [PunRPC]
        protected void Item3DSetUp(string assemblyName, ItemType type, string spriteName, bool isTreasureRoomItem = false)
        {
            m_AssemblyName = assemblyName;
            m_ItemType = type;
            m_IsTreasureRoomItem = isTreasureRoomItem;

            //if the item being spawned is a treasure room item then we want to make sure that it cannot be influenced as a rigidbody
            //and then also make sure that the object's collider is that of a trigger so that the player can interact with it
            if (m_IsTreasureRoomItem)
            {
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
                BoxCollider collider = GetComponent<BoxCollider>();
                collider.isTrigger = true;
                collider.size *= 3.3f;
            }

            m_JournalSprite = Resources.Load<Sprite>(spriteName);

            //this section sets up all the mesh renderer data so that the proper mesh will be spawned
            {
                MeshFilter filter = GetComponent<MeshFilter>();
                MeshRenderer renderer = GetComponent<MeshRenderer>();

                GameObject itemsMesh = ((GameObject)Resources.Load("Items/Meshes/Tablet" + (assemblyName.Replace("TheNegative.Items.", ""))));
                if (itemsMesh != null)
                {
                    filter.mesh = itemsMesh.GetComponent<MeshFilter>().sharedMesh;
                    renderer.material = itemsMesh.GetComponent<MeshRenderer>().sharedMaterial;
                }
            }


            if (m_ItemType == ItemType.PickUp)
                CanPlayerObtainItem = ItemManager.Instance.PickUpItemsInGame[assemblyName].CanObtainItem;
            else
                CanPlayerObtainItem = ItemManager.Instance.ItemsInGame[assemblyName].CanObtainItem;
        }

        [PunRPC]
        public void RPCGetItemDelegate()
        {
            CanPlayerObtainItem = ((AbstractItem)ItemManager.Instance.GetObjectFromName(m_AssemblyName)).CanObtainItem;
        }

        [PunRPC]
        protected void ActivateItemAndDisable(int playerNumber)
        {
            //set the item to be collected
            m_ItemCollected = true;

            if (PhotonNetwork.isMasterClient)
            {             
                //tell the item manager instance on the master's end to add the item to the player's inventory
                ItemManager.Instance.AddItemToPlayerInvertoryByName(playerNumber, m_AssemblyName, m_ItemType);
                //disable the game object
                SetActive(false);
                m_ItemCollected = false;
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                Player player = other.GetComponent<Player>();
                //toggle the interact image for the local version of the player
                if (player.photonView.isMine && m_ItemType != ItemType.PickUp)
                    PlayerHUDManager.instance.ToggleInteractImage(true);
            }
        }

        protected virtual void OnTriggerStay(Collider other)
        {
            if (other.tag != "Player" || m_Timer <= 1.0f) return;

            Player player = other.GetComponent<Player>();

            //make sure that the item delegate is set up if it already isn't
            if (CanPlayerObtainItem == null)
                photonView.RPC("RPCGetItemDelegate", PhotonTargets.All);

            //check the item delegate and then also see if this item is a treasure room item and if the player has picked up a treasure room item
            if (!CanPlayerObtainItem(player) || m_ItemCollected || !player.photonView.isMine)
                return;

            //if this is a treasure room item
            if (m_IsTreasureRoomItem)
            {
                //if the player cannot loot anymore treasure from the treasure room. If the player isn't holding down the 
                //the interact key then they will not be able to pick up the item as well so return
                if (!player.CanLootTreasureRoom || !CharacterController.GetInteractHeld()) return;

                photonView.RPC("ActivateItemAndDisable", PhotonTargets.All, player.PlayerNumber);
                player.CanLootTreasureRoom = false;
                PlayerHUDManager.instance.ToggleInteractImage(false);
                m_Timer = 0.0f;
            }
            else
            {
                photonView.RPC("ActivateItemAndDisable", PhotonTargets.All, player.PlayerNumber);
                PlayerHUDManager.instance.ToggleInteractImage(false);
                m_Timer = 0.0f;
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (other.tag == "Player")
            {
                Player player = other.GetComponent<Player>();
                //if the local player just left the radius of the item then toggle the interact image off
                if (player.photonView.isMine)
                    PlayerHUDManager.instance.ToggleInteractImage(false);
            }
        }

        public string AssemblyName { get { return m_AssemblyName; } set { m_AssemblyName = value; } }
    }
}