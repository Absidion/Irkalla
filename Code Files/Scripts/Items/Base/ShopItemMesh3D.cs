using UnityEngine;

//Writer: Liam
//Last Updated: Liam 3/9/2018

namespace TheNegative.Items
{
    public class ShopItemMesh3D : ItemMesh3D
    {
        private int m_Value = 0;                    //The value of the item        

        protected override void Awake()
        {
            base.Awake();
            SetActive(true);

            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
            BoxCollider collider = GetComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size *= 3.3f;
        }

        [PunRPC]
        protected void ShopItem3DSetUp(string assemblyName, ItemType type, string spriteName, int itemCost)
        {
            //set the items cost
            m_Value = itemCost;

            string itemName = assemblyName;
            name = itemName.Replace("TheNegative.Items.", "");
            SetName(name);

            //call the 3D item set up since it will do everything that we require for item set up
            Item3DSetUp(assemblyName, type, spriteName, false);
        }

        protected bool CanPlayerAffordItem(Player player)
        {
            if (player.GetCurrency() >= m_Value)
            {
                player.ChangeCurrency(-m_Value);
                //SoundManager.GetInstance().photonView.RPC("PlayerSFXNetworked", PhotonTargets.All, "BoughtItem");
                return true;
            }
            return false;
        }

        //Toggles the purchase box on
        protected override void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                Player player = other.GetComponent<Player>();

                if (player.photonView.isMine)
                {
                    string itemName = gameObject.name.Replace("TheNegative.Items.", "");
                    PlayerHUDManager.instance.ToggleShopWindow(true, itemName, Value.ToString());
                }
            }
        }

        //will do the actual logic of obtainning an item
        protected override void OnTriggerStay(Collider other)
        {
            if (m_Timer <= 1.0f || other.tag != "Player" || !CharacterController.GetInteractDown()) return;
            
            Player potentialCustomer = other.GetComponent<Player>();

            //if the CanPlayerObtainItem delegate is null then it must be set, so call the RPC method
            if (CanPlayerObtainItem == null)
                photonView.RPC("RPCGetItemDelegate", PhotonTargets.All);

            //next check to make sure that the player can actually obtain the item
            //Also if the item is collected then just don't allow them to purchase as well
            if (!CanPlayerObtainItem(potentialCustomer) || m_ItemCollected || !potentialCustomer.photonView.isMine)
                return;

            //next we can check to see if the player has the money to do this, we do this in a seperate if statement because this function
            //call actually decreases the players currency in their inventory, it could just take money and not give them an item they aren't allowed to have
            if (!CanPlayerAffordItem(potentialCustomer))
                return;

            //if the item is a pick up item that means it can continuously be picked up so we only add it locally since it doesn't need to change anything over the network
            if (m_ItemType == ItemType.PickUp)
            {
                ItemManager.Instance.AddItemToPlayerInvertoryByName(potentialCustomer.PlayerNumber, m_AssemblyName, m_ItemType);
                m_Timer = 0.0f;
            }
            //if the item isn't a pick up then it must be added to the correct inventory across the network via the item manager
            else
            {
                photonView.RPC("ActivateItemAndDisable", PhotonTargets.All, potentialCustomer.PlayerNumber);                
                //toggle the interact window and the shop window since the item has now been taken away
                PlayerHUDManager.instance.ToggleInteractImage(false);
                PlayerHUDManager.instance.ToggleShopWindow(false);
                m_Timer = 0.0f;
            }
        }

        //Toggles the purchase box off
        protected override void OnTriggerExit(Collider other)
        {
            if (other.tag == "Player")
            {
                Player player = other.GetComponent<Player>();

                if (player.photonView.isMine)
                    PlayerHUDManager.instance.ToggleShopWindow(false);
            }
        }

        public int Value { get { return m_Value; } set { m_Value = value; } }
    }
}