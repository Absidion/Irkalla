using System;
using System.Collections;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TheNegative.AI;
using TheNegative.Items;


public class DebugConsole : MonoBehaviour {
    public float TeleportVerticalOffset = 1; 

    private InputField m_InputField;        // Inputfield used to get the text
    private Image      m_Background;        // The inputfield's background
    private Player     m_LocalPlayerRef;    // The local player Commands are used on

    // Use this for initialization
    void Start ()
    {
        //Get the input field
        m_InputField = GetComponent<InputField>();
        m_InputField.DeactivateInputField();
        m_InputField.enabled = false;
        m_Background = GetComponent<Image>();
        m_Background.enabled = false;
    }

    // Update is called once per frame
    void Update ()
    {
        // If the Local Player is null Try and find it
        // If you cant find the player return   
        if(m_LocalPlayerRef == null)
        {
            GetLocalPlayer();
            if (m_LocalPlayerRef == null)
                return;
        }
		//When the ~ key is pressed set the Inputfield to active and focused
        //If the ~ Key is pressed again deactivate the Inputfield
        if(Input.GetKeyDown(KeyCode.BackQuote))
        {
            //Toggle the Activation of the m_InputField
            if (m_InputField.enabled == true)
            {
                m_InputField.DeactivateInputField();
                m_InputField.enabled = false;
                m_Background.enabled = false;
                m_InputField.text = string.Empty;

            }
            else
            {
                m_InputField.enabled = true;
                m_Background.enabled = true;
                m_InputField.ActivateInputField();
                m_InputField.Select();
            }
        }

        // If the Enter Key is pressed Process The string with ProcessCommand
        // The Inputfield has to be active for this to work
        if (Input.GetKeyDown(KeyCode.Return) && m_InputField.enabled == true)
        {
            // Check if the inputfeild text is empty
            if(m_InputField.text.Length != 0)
            {
                ProcessCommand(m_InputField.text);
            }
        }
    }


    private void GetLocalPlayer()
    {
        //Get all the players in the game
        Player[] players = FindObjectsOfType<Player>();

        //If the players havent been spawned yet return
        if (players.Length <= 0)
            return;

        //Loop through the players found
        foreach (Player player in players)
        {
            if (player.photonView.isMine)
            {
                m_LocalPlayerRef = player;
            }
        }

    }

    private void ProcessCommand(string Input)
    {
        // The Command is split into 3 parts
        // 1 The Command ex: Spawn
        // 2 The Object  ex: Nerg
        // 3 the Amount  ex: 3

        //Get the Commands from the input
        string[] Substrings = Input.Split(' ');
        string   Command = Substrings[0];
        // Get the command from from the Input and call the call the correct command and pass in the Object and amount
        // Check for special Commands like "KillSelf or Invinciblity"
        // Make the Command not case sensitive
        Command = Command.ToLower();
        switch (Command)
        {
            case "spawnmonster":
                SpawnMonsterCommand(Input);
                break;

            case "spawnitem":
                SpawnItemCommand(Input);
                break;

            case "give":
                GiveCommand(Input);
                break;

            case "stat":
                StatCommand(Input);
                break;

            case "teleport":
                TeleportCommand(Input);
                break;

            case "killself":
                KillSelfCommand();
                break;

            case "invis":
                InvinciblityCommand();
                break;
            case "/r":
                ResetCommand();
                break;
            case "nextlevel":
                NextLevelCommand();
                break;
        }

        //Clear the text
        m_InputField.text = string.Empty;

    }

    #region Generic Commands
    private void TeleportCommand(string Input)
    {
        //Search the Game for a Spawned Room Matching the name of the Input and teleport the player to a Spawn Point in The Room
        string[] Substrings = Input.Split(' ');
        if (Substrings.Length < 2 || Substrings.Length > 2)
            return;
        //Get the Object to teleport to 
        string Object = Substrings[1];

        //Find an object that matches the room
        IslandRoom[] roomObjs = GameObject.FindObjectsOfType<IslandRoom>();
        //Check if the object is actually a room
        foreach (IslandRoom room in roomObjs)
        {
            var roomTyoe = (RoomType)Enum.Parse(typeof(RoomType), Object);
            if (room.TypeOfRoom == roomTyoe)
            {
                Vector3 roomPos = room.transform.position;  
                roomPos.y += TeleportVerticalOffset;
                m_LocalPlayerRef.transform.position = roomPos; 
            }
        }

    }

    private void SpawnMonsterCommand(string Input)
    {
        string[] Substrings = Input.Split(' ');
        if (Substrings.Length < 3)
            return;
        //Get the Object to Spawn
        string Object = Substrings[1];
        //Get the Amount to modify
        int  Amount = int.Parse(Substrings[2]);
        //If the parse failed or equals 0 set the number to 1
        if (Amount == 0)
            Amount = 1;

        // Find all the usable Spawn points in the player's current room
        EnemySpawnPoint[] AllLocalSpawnPoints = m_LocalPlayerRef.MyIslandRoom.RoomSpawnPoints.ToArray();
        //Spawn the specified amout of monsters at the found spawn point(s)
        for (int i = 0; i < AllLocalSpawnPoints.Length && i < Amount; i++)
        {
            //use photon to instantiate the object over the network
            GameObject obj = PhotonNetwork.InstantiateSceneObject("AI/" + Object, AllLocalSpawnPoints[i].transform.position, AllLocalSpawnPoints[i].transform.rotation, 0, null);

            if (obj == null)
                return;
            //Add the monster to the room
            RoomManager.Instance.AddEnemyToRoom(obj.GetComponent<AI>(), m_LocalPlayerRef.MyIslandRoom);
            //Activate the AI
            AI ai = obj.GetComponent<AI>();
            ai.enabled = true;
            ai.Init();

            obj.SetActive(true);

        }
    }

    private void SpawnItemCommand(string Input)
    {
        string[] Substrings = Input.Split(' ');
        if (Substrings.Length < 3)
            return;
        //Get the Object to Spawn
        string Object = Substrings[1];
        //Get the Amount to modify
        int Amount = int.Parse(Substrings[2]);
        //If the parse failed or equals 0 set the number to 1
        if (Amount == 0)
            Amount = 1;

        // Find all the usable Spawn points in the player's current room

        //Make an array for the LocalSpawnPoint's children

        //Set AllLocalSpawnPoints to the children to the Local spawn point

        EnemySpawnPoint[] AllLocalSpawnPoints = m_LocalPlayerRef.MyIslandRoom.RoomSpawnPoints.ToArray();


        bool isXmlItem = IsObjectXML(Object);
        //Spawn the specified amout of monsters at the found spawn point(s)
        if (isXmlItem)
        {
            //Spawn XML items
            for (int i = 0; i < AllLocalSpawnPoints.Length && i < Amount; i++)
            {
                SpawnXMLItem(Object, AllLocalSpawnPoints[i].transform);
            }
        }
        else
        {
            //SpawnPickups
            for (int i = 0; i < AllLocalSpawnPoints.Length && i < Amount; i++)
            {
                SpawnPickup(Object, AllLocalSpawnPoints[i].transform);
            }
        }
    }

    private bool IsObjectXML(string obj)
    {
        //Check for special cases
        if(obj.ToLower() == "heart"
            ||
            obj.ToLower() == "armor")
        {
            return false;
        }

        //Check the XML's for this items name

        //Check the shop items for the item
        TextAsset ShopXml = Resources.Load("Xmls/ShopItems") as TextAsset;
        if (ShopXml.text.Contains("TheNegative.Items." + obj))
            return true;

        //Check the items for the item
        TextAsset ItemXml = Resources.Load("Xmls/Items") as TextAsset;
        if (ItemXml.text.Contains("TheNegative.Items." + obj))
            return true;

        //If this item does not exits try to spawn it as a
        return false;
    }

    private void SpawnXMLItem(string obj, Transform pos)
    {

        string AssemblyName = "TheNegative.Items." + obj;

        object actualItem = ItemManager.Instance.GetObjectFromName(AssemblyName);
        ItemManager.Instance.photonView.RPC("RPCCreateAndAddItemToList", PhotonTargets.All, AssemblyName);

        GameObject itemMesh = PhotonNetwork.Instantiate("Items/Meshes/" + ItemManager.Instance.ItemMeshPrefab.name, pos.position, pos.rotation, 0);
        itemMesh.GetComponent<ItemMesh>().CanPlayerObtainItem = (actualItem as AbstractItem).CanObtainItem;
        //instantiate the new ItemMesh into the world            
        itemMesh.GetComponent<ItemMesh>().photonView.RPC("ItemSetUp",
                                                         PhotonTargets.All,
                                                          (actualItem as AbstractItem).ItemType,
                                                         AssemblyName,
                                                          (actualItem as AbstractItem).SpriteName);

    }
    private void SpawnPickup(string obj, Transform pos)
    {
        GameObject itemMesh = ObjectPoolManager.Instance.GetObjectFromNetworkPool(obj);

        //other 3D items that are not chests

        ItemMesh3D item = itemMesh.GetComponent<ItemMesh3D>();
        //set the item assembly of the item 
        item.AssemblyName = "TheNegative.Items." + obj + "PickUp";
        
        float speed = UnityEngine.Random.Range(5.0f, 7.5f);
        Vector3 direction = Vector3.up;
        direction.x = UnityEngine.Random.Range(-0.25f, 0.25f);
        direction.z = UnityEngine.Random.Range(-0.25f, 0.25f);

        item.SetActive(true, pos.position, Quaternion.identity.eulerAngles, direction, speed);

        object actualItem = ItemManager.Instance.GetObjectFromName(item.AssemblyName);

        item.photonView.RPC("Item3DSetUp", PhotonTargets.All, item.AssemblyName, (actualItem as AbstractItem).ItemType);
        item.CanPlayerObtainItem = (actualItem as AbstractItem).CanObtainItem;
    }

    private void GiveCommand(string Input)
    {
        string[] Substrings = Input.Split(' ');
        if (Substrings.Length < 3)
            return;
        //Get the Object to Spawn
        string Object = Substrings[1];
        //Get the Amount to modify
        int Amount = int.Parse(Substrings[2]);
        //If the parse failed or equals 0 set the number to 1
        if (Amount == 0)
            Amount = 1;

        for (int i = 0; i < Amount; i++)
        {
            // Get the Item from the Item Manager
            string AssemblyName = "TheNegative.Items." + Object;

            object actualItem = ItemManager.Instance.GetObjectFromName(AssemblyName);

            // Add the Item to the Local Players Inventory
            ItemManager.Instance.AddItemToPlayerInvertoryByName(m_LocalPlayerRef.PlayerNumber, AssemblyName, (actualItem as AbstractItem).ItemType);
        }
    }

    private void StatCommand(string Input)
    {
        //Get the Stat to modify
        string[] Substrings = Input.Split(' ');
        if (Substrings.Length < 3)
            return;

        //Get the Object to Spawn
        string Object = Substrings[1];
        // Set the object to lowercase
        Object = Object.ToLower();
        //Get the Amount to modify
        int Amount = int.Parse(Substrings[2]);
        //If the parse failed or equals 0 set the number to 1
        if (Amount == 0)
            Amount = 1;

        //Check if the stat is health is a special case (Does not use Player.IncreaseStat)
        //Return when you added for the special case

        //Add the Amount to the health
        if (Object.ToLower() == "health")
        {
            if (Substrings[2].ToLower() != "max")
                m_LocalPlayerRef.Health.ChangeHp(Amount);
            else
                m_LocalPlayerRef.Health.ChangeHp(10000000);

            return;
        }
        //Add the Amount to the Max health
        if (Object.ToLower() == "maxhealth")
        {
            if(Substrings[2].ToLower() != "max")
                m_LocalPlayerRef.Health.ChangeMaxHP(Amount);
            else
                m_LocalPlayerRef.Health.ChangeMaxHP(10000000);

            return;
        }
        //Add the Amount to the players Currency
        if (Object.ToLower() == "currency")
        {
            m_LocalPlayerRef.ChangeCurrency(Amount);
            return;
        }

        //Loop though the diffrent names in the Enum type
        string[] names = Enum.GetNames(typeof(StatType));
        foreach (string statTypeName in names)
        {
            // If the Types's name converts set the matching stat
            string statTypeNameLower = statTypeName.ToLower();
            if (statTypeNameLower.Contains(Object))
            {
                //Convert to stat type from string
                var stat = (StatType)Enum.Parse(typeof(StatType), Object.ToUpper());
                // Set the Stat to max if the Amount is MAX
                if(Substrings[2].ToLower() == "max")
                {
                    //TODO: Since max lives isnt capped the Max lives can just contunue to tick up 
                    m_LocalPlayerRef.IncreaseStat(stat, 1000000);
                }
                //Otherwise just add the amount to the start
                else
                {
                    m_LocalPlayerRef.IncreaseStat(stat, Amount);
                }
            }
        }



    }
    #endregion

    #region Special Commands

    private void NextLevelCommand()
    {
        GameManager.Instance.ProceedToNextLevel();
    }

    private void ResetCommand()
    {
        //Reset the level
        GameManager.Instance.ReloadCurrentLevel();
    }

    private void KillSelfCommand()
    {
        //Kill the local player
        m_LocalPlayerRef.Health.IsDead = true;
    }

    private void InvinciblityCommand()
    {
        if (m_LocalPlayerRef.Health.IsInvincible == false)
        {
            m_LocalPlayerRef.Health.IsInvincible = true;
            return;
        }
        if (m_LocalPlayerRef.Health.IsInvincible == true)
        {
            m_LocalPlayerRef.Health.IsInvincible = false;
            return;
        }
    }
    #endregion
}
