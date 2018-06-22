using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TheNegative.Items;
using MersenneTwister;

namespace TheNegative.Items
{
    public delegate bool CanObtainItem(Player p);
}

public class ItemManager : SyncBehaviour
{
    public static ItemManager Instance;                                             //the singleton instance of the item manager in the scene

    public GameObject ItemMeshPrefab;
    public GameObject ShopItemMeshPrefab;

    public GameObject ItemMesh3DPrefab;
    public GameObject ShopItemMesh3DPrefab;

    public float DropRate = 10.0f;                                                  //base game drop rate

    private List<AbstractItem> m_UsedItemList = new List<AbstractItem>();           //a list of the used treasure room items to prevent the reapearance of the same items
    private List<AbstractItem> m_UsedShopItems = new List<AbstractItem>();          //a list of the used shop items to prevent the reapearance of the same items
    private MT19937 m_RandomNumberGenerator;                                        //an instance of our random number generator
    private Player m_LocalPlayer;                                                   //the local player
    private Player m_OtherPlayer;                                                   //the none local player 

    private float m_CommonItemChance = 60.0f;                                       //the percent chance that the treasure room will spawn a common item on a pedistal
    private float m_RareItemChance = 35.0f;                                         //the percent chance that the treasure room will spawn a rare item on a pedistal
    private float m_LegendaryItemChance = 5.0f;                                     //the percent chance that the treasure room will spawn a legendary item on a pedistal

    private const float m_CommonItemChangeValue = 4.0f;                             //if a common item is aquired then we remove this amount from the common item chance value and split it between the other 2 values
    private const float m_RareItemChangeValue = 4.0f;                               //if a rare item is aquired then we remove this amount from the rare item chance value and split it between the other 2 values
    private const float m_LegendaryItemChangeValue = 2.0f;                          //if a legendary item is aquired then we remove this amount from the legendary item chance value and split it between the other 2 values

    private bool m_Initialized = false;

    private Dictionary<string, AbstractItem> m_ItemsInGame;
    private Dictionary<string, AbstractItem> m_PickUpItemsInGame;

    protected override void Awake()
    {
        if (Instance != null)
        {
            photonView.enabled = false;
            gameObject.SetActive(false);
            Destroy(this.gameObject);
            return;
        }

        base.Awake();
        Instance = this;
        MapGeneratorV2 mg = FindObjectOfType<MapGeneratorV2>();
        if (mg.IsTesting)
        {
            m_RandomNumberGenerator = new MT19937(mg.Seed);
            UnityEngine.Random.InitState((int)mg.Seed);
        }
        else
        {
            m_RandomNumberGenerator = new MT19937((ulong)GameManager.Instance.SeedValue);
            UnityEngine.Random.InitState(GameManager.Instance.SeedValue);
        }

        m_ItemsInGame = new Dictionary<string, AbstractItem>();
        m_PickUpItemsInGame = new Dictionary<string, AbstractItem>();

        GameManager.Instance.DontDestroyNormalObject(gameObject);
        GameManager.PostPlayer += InitializeItemManager;
    }

    void Init()
    {
        //get an array of the players in the game
        Player[] players = FindObjectsOfType<Player>();
        //no players in the scene, return and try again later
        if (players == null)
        {
            return;
        }
        //if there are players, or rather more then 1 player then we can now begin to initalize our player references
        else if (players.Length == PhotonNetwork.playerList.Length)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].photonView.isMine)
                {
                    m_LocalPlayer = players[i];
                }
                else
                {
                    m_OtherPlayer = players[i];
                }
            }
            m_Initialized = true;

            //Debug line of code which will make it so the local player spawns with the item in the string at the end of the rpc
            //photonView.RPC("AddItemToInventory", PhotonTargets.All, m_LocalPlayer.PlayerNumber, "TheNegative.Items.BlackHole");

            if (PhotonNetwork.isMasterClient)
            {
                //get all values of the enum so we can create pools for them
                DroppableItems[] enumValues = Enum.GetValues(typeof(DroppableItems)) as DroppableItems[];

                //iterate through the enum values and create a pool for every droppable item
                foreach (DroppableItems item in enumValues)
                {
                    if (item != DroppableItems.NULL)
                        ObjectPoolManager.Instance.CreateNetworkPoolWithName(item.ToString(), "Items/Meshes/" + Resources.Load<GameObject>("Items/Meshes/" + item.ToString()).name, 10, 20, true);
                }
            }
        }
    }

    private void InitializeItemManager(object sender, EventArgs args)
    {
        //first the item manager needs to be initialized
        while (m_Initialized == false)
        {
            Init();
        }

        PopulateTreasureRoom();
        PopulateShop();
    }

    void Update()
    {
        if (m_LocalPlayer == null)
            return;

        //we need to update the personal affects of our passive items but only for the local player
        if (m_LocalPlayer.photonView.isMine)
        {
            m_LocalPlayer.Journal.UpdatePassiveItems();
            m_LocalPlayer.Journal.UpdateActiveItem();
        }
        //We need to update our local players network visuals and network affects that way both players can see the changes visually or get the affects that are networked
        {
            m_LocalPlayer.Journal.UpdatePassiveItemsNetwork();
            m_LocalPlayer.Journal.UpdateActiveItemNetwork();
        }
        //We now need to update the other player's passive items and active item, but only the network affects and network visuals
        if (m_OtherPlayer != null)
        {
            m_OtherPlayer.Journal.UpdatePassiveItemsNetwork();
            m_OtherPlayer.Journal.UpdateActiveItemNetwork();
        }
    }

    public void PopulateTreasureRoom()
    {
        if (!PhotonNetwork.isMasterClient)
            return;

        //get the treasure room from the world
        IslandRoom[] rooms = FindObjectsOfType<IslandRoom>();
        GameObject treasureRoom = null;
        foreach (IslandRoom room in rooms)
        {
            if (room.TypeOfRoom == RoomType.Treasure)
                treasureRoom = room.gameObject;
        }

        //get the transform from the treasure room which is the parent transform of all treasure spawn locations
        Transform treasureSpawnParent = treasureRoom.transform.Find("TreasureSpawnPoints");
        //create a new array of the child transforms from treasure spawn parent variable
        Transform[] treasureSpawnPoints = new Transform[treasureSpawnParent.childCount];

        for (int i = 0; i < treasureSpawnParent.childCount; i++)
        {
            treasureSpawnPoints[i] = treasureSpawnParent.GetChild(i);
        }

        for (int i = 0; i < treasureSpawnPoints.Length; i++)
        {
            //create the a string to store the item assembly name
            string itemAssemblyName = string.Empty;
            do
            {
                //get the assembly name base on the rareity and the index of the item we've grabed
                itemAssemblyName = XmlUtilities.GetNameFromItemXML();
            } while (m_ItemsInGame.ContainsKey(itemAssemblyName));

            photonView.RPC("RPCCreateAndAddItemToList", PhotonTargets.All, itemAssemblyName);

            GameObject itemMesh = PhotonNetwork.Instantiate("Items/Meshes/" + ItemMesh3DPrefab.name, treasureSpawnPoints[i].position, treasureSpawnPoints[i].rotation, 0);


            itemMesh.GetComponent<ItemMesh3D>().photonView.RPC("Item3DSetUp",
                                                 PhotonTargets.All,
                                                 itemAssemblyName,
                                                 m_ItemsInGame[itemAssemblyName].ItemType,
                                                 m_ItemsInGame[itemAssemblyName].SpriteName,
                                                 true);
        }
    }

    public void PopulateShop()
    {
        if (!PhotonNetwork.isMasterClient)
            return;

        //get the shop room from the world
        IslandRoom[] rooms = FindObjectsOfType<IslandRoom>();
        GameObject shopRoom = null;
        foreach (IslandRoom room in rooms)
        {
            if (room.TypeOfRoom == RoomType.Shop)
                shopRoom = room.gameObject;
        }

        //get the average luck stat of both players
        float avgLuck = 0.0f;
        if (m_OtherPlayer != null)
            avgLuck = (m_LocalPlayer.GetStat(StatType.LUCK) + m_OtherPlayer.GetStat(StatType.LUCK)) * 0.5f;
        else
            avgLuck = m_LocalPlayer.GetStat(StatType.LUCK);

        //get the transform from the shop room which is the parent transform of all item spawn locations
        Transform shopSpawnParent = shopRoom.transform.Find("ShopSpawnPoints");
        //create a new array of the child transforms from shop spawn parent variable
        Transform[] shopSpawnPoints = new Transform[shopSpawnParent.childCount];

        for (int i = 0; i < shopSpawnParent.childCount; i++)
        {
            shopSpawnPoints[i] = shopSpawnParent.GetChild(i);
        }

        for (int i = 0; i < shopSpawnPoints.Length; i++)
        {
            //the rarity value of the item that the item manager will spawn
            string rarity = "Other";

            //if we are populating the last item in the shop then do this code
            if (i != shopSpawnPoints.Length - 1)
            {
                rarity = "Basic";
            }

            //save the items cost value
            int itemValue = 0;
            string itemAssemblyName = string.Empty;
            do
            {
                //get the assembly name of the shop item to spawn
                itemAssemblyName = XmlUtilities.GetNameFromShopItemXML(rarity, out itemValue);
            } while (m_ItemsInGame.ContainsKey(itemAssemblyName));


            photonView.RPC("RPCCreateAndAddItemToList", PhotonTargets.All, itemAssemblyName);

            //instantiate mesh
            GameObject itemMesh = PhotonNetwork.Instantiate("Items/Meshes/" + ShopItemMesh3DPrefab.name, shopSpawnPoints[i].position, shopSpawnPoints[i].rotation, 0);

            //fill relevant data
            ItemType itemType = ItemType.NULL;
            string spriteName = string.Empty;

            if (m_ItemsInGame.ContainsKey(itemAssemblyName))
            {
                itemType = m_ItemsInGame[itemAssemblyName].ItemType;
                spriteName = m_ItemsInGame[itemAssemblyName].SpriteName;
            }
            else if (m_PickUpItemsInGame.ContainsKey(itemAssemblyName))
            {
                itemType = m_PickUpItemsInGame[itemAssemblyName].ItemType;
                spriteName = m_PickUpItemsInGame[itemAssemblyName].SpriteName;
            }

            itemMesh.GetComponent<ShopItemMesh3D>().photonView.RPC("ShopItem3DSetUp",
                                                                 PhotonTargets.All,
                                                                 itemAssemblyName,
                                                                 itemType,
                                                                 spriteName,
                                                                 itemValue);
        }
    }

    [PunRPC]
    private void RPCCreateAndAddItemToList(string assemblyName)
    {
        object obj = GetObjectFromName(assemblyName);

        if ((((AbstractItem)obj).ItemType != ItemType.PickUp) &&
           !m_ItemsInGame.ContainsKey(assemblyName))
        {
            m_ItemsInGame.Add(assemblyName, obj as AbstractItem);
        }
        else if ((((AbstractItem)obj).ItemType == ItemType.PickUp) &&
            !m_PickUpItemsInGame.ContainsKey(assemblyName))
        {
            m_PickUpItemsInGame.Add(assemblyName, obj as AbstractItem);
        }
    }

    public void SpawnItemFromRoom(Vector3 posToSpawn, float playerLuck, RoomType roomType)
    {
        if (!PhotonNetwork.isMasterClient)
            return;

        DroppableItems itemToSpawn = PickupDetails.GetDroppableItemFromRoom(roomType, playerLuck);

        //if the item wasn't set that means that the room that was cleared was a room that cannot spawn items, so we return
        if (itemToSpawn == DroppableItems.NULL)
            return;

        SpawnItem(posToSpawn, itemToSpawn);
    }

    public void SpawnItemIntoWorld(Vector3 posToSpawn, float playerLuck, bool chestInPool = true)
    {
        if (!PhotonNetwork.isMasterClient)
            return;

        float randomValue = UnityEngine.Random.Range(0.0f, 1.0f);
        //determine if it's possible to spawn an item this time round

        DroppableItems itemToSpawn = DroppableItems.Chest;

        //if chests are in the pool then spawn using the following function
        if (chestInPool)
        {
            if (randomValue > DropRate + playerLuck)
                return;

            //get a random droppable item
            itemToSpawn = PickupDetails.GetRandomItem();
        }
        else
        {
            itemToSpawn = PickupDetails.GetRandomNonChestItem();
        }

        SpawnItem(posToSpawn, itemToSpawn);
    }

    private void SpawnItem(Vector3 positionToSpawn, DroppableItems itemToSpawn)
    {
        GameObject itemMesh = ObjectPoolManager.Instance.GetObjectFromNetworkPool(itemToSpawn.ToString());

        //chests behave differently then regular pick up items but can still be dropped so they get managed here
        if (itemToSpawn == DroppableItems.Chest)
        {
            ChestItemMesh chest = itemMesh.GetComponent<ChestItemMesh>();
            //get the closest position to the ground and place the chest in that location
            chest.transform.position = MathFunc.CalculateClosestGroundPosition(positionToSpawn);
            chest.SetActive(true);
        }
        //other 3D items that are not chests
        else
        {
            ItemMesh3D item = itemMesh.GetComponent<ItemMesh3D>();
            //set the item assembly of the item 
            item.AssemblyName = PickupDetails.GetDroppableItemAssembly(itemToSpawn);

            if (!m_PickUpItemsInGame.ContainsKey(item.AssemblyName))
                photonView.RPC("RPCCreateAndAddItemToList", PhotonTargets.All, item.AssemblyName);

            float speed = UnityEngine.Random.Range(5.0f, 7.5f);
            Vector3 direction = Vector3.up;
            direction.x = UnityEngine.Random.Range(-0.25f, 0.25f);
            direction.z = UnityEngine.Random.Range(-0.25f, 0.25f);

            item.SetActive(true, positionToSpawn, Quaternion.identity.eulerAngles, direction, speed);

            object actualItem = GetObjectFromName(item.AssemblyName);

            item.photonView.RPC("Item3DSetUp", PhotonTargets.All, item.AssemblyName, (actualItem as AbstractItem).ItemType, (actualItem as AbstractItem).SpriteName, false);
            item.CanPlayerObtainItem = (actualItem as AbstractItem).CanObtainItem;
        }
    }

    #region Adding items to the player
    public void AddItemToPlayerInvertoryByName(int playerNum, string itemName, ItemType itemType)
    {
        photonView.RPC("AddItemToInventory", PhotonTargets.All, playerNum, itemName, itemType);
    }

    [PunRPC]
    private void AddItemToInventory(int playerNumber, string itemAssembly, ItemType itemType)
    {
        //set the player to be the local player
        Player player = m_LocalPlayer;

        //check to make sure the player number corresponds to the local player otherwise assign player to be equal to the otherPlayer
        if (player.PlayerNumber != playerNumber)
        {
            player = m_OtherPlayer;
        }

        //get the item that the player just picked up as an object
        AbstractItem itemInstance = null;
        if (itemType == ItemType.PickUp)
            itemInstance = m_PickUpItemsInGame[itemAssembly];

        else
            itemInstance = m_ItemsInGame[itemAssembly];

        //figure out what type the item is so that we can put it into the correct location in the player's journal
        if ((itemInstance as AbstractItem).ItemType == ItemType.Passive)
        {
            player.Journal.AddPassiveItemToJournal(itemInstance as AbstractPassive);
        }
        //if the item is an pick up item then don't add it to the journal just activate the effect
        else if ((itemInstance as AbstractItem).ItemType == ItemType.PickUp)
        {
            (itemInstance as AbstractItem).ActivateItem(player);
        }
        //if the item is an active item then we need to add it to the journal but also remove the old item and place it back into the world
        else
        {
            //TODO: place the old active item back into the world. Make this function return the old active item
            player.Journal.AquireNewActiveItem(itemInstance as AbstractItem);
        }
    }
    #endregion

    #region Update item chances
    private void UpdateCommonItemChance()
    {
        //change the common item chance so that it's less likely for players to get a common item next time the treasure rom is spawned
        m_CommonItemChance -= m_CommonItemChangeValue;
        m_RareItemChance += m_CommonItemChangeValue * 0.5f;
        m_LegendaryItemChance += m_CommonItemChangeValue * 0.5f;
    }

    private void UpdateRareItemChance()
    {
        //chance the rare item chance so that it's less likely for players to get a rare item next time the treasure room is spawned
        m_RareItemChance -= m_RareItemChangeValue;
        m_CommonItemChance += m_RareItemChangeValue * 0.5f;
        m_LegendaryItemChance += m_RareItemChangeValue * 0.5f;
    }

    private void UpdateLegendaryItemChance()
    {
        //change the legendary item chance so that it's less likely for players to get a legendary item next time the treasure room is spawned
        //unlike the others this a divisor value, so the chance of obtainning a legendary item is largely reduced when a legendary item is spawned
        float changeValue = m_LegendaryItemChance / m_LegendaryItemChangeValue;
        m_LegendaryItemChance -= changeValue;
        m_CommonItemChance += changeValue * 0.5f;
        m_RareItemChance += changeValue * 0.5f;
    }
    #endregion

    public void UpdateRoomBasedCooldown(int playerNumber)
    {
        //check to see if the local player number matches the player number passed in
        if (m_LocalPlayer.PlayerNumber == playerNumber)
        {
            m_LocalPlayer.Journal.DecreaseActiveRoomBasedCooldown();
        }
        //check to see if the other player number matches the player number passed in        
        else if (m_OtherPlayer.PlayerNumber == playerNumber)
        {
            m_OtherPlayer.Journal.DecreaseActiveRoomBasedCooldown();
        }
    }

    public object GetObjectFromName(string typeAssemblyName)
    {
        Type itemType = Type.GetType(typeAssemblyName);
        object itemInstance = Activator.CreateInstance(itemType);
        return itemInstance;
    }

    public Dictionary<string, AbstractItem> ItemsInGame { get { return m_ItemsInGame; } }
    public Dictionary<string, AbstractItem> PickUpItemsInGame { get { return m_PickUpItemsInGame; } }
}