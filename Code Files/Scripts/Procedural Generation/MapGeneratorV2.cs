using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MersenneTwister;
using System;

//Writer: Liam
//Laste Updated: 1/15/2017

public class MapGeneratorV2 : MonoBehaviour
{
    #region Room Prefabs
    [Tooltip("Place the boss rooms into the List of Boss room prefabs. All rooms will be built off of the boss room")]
    public List<GameObject> BossRoomPrefab;
    [Tooltip("Add to this List every normal room prefab that isn't unique prefab you want to spawned")]
    public List<GameObject> RoomPrefabs;
    [Tooltip("Add to this List every unique room prefab that will be created after the dungeon is made.")]
    public List<GameObject> UniqueRooms;
    [Tooltip("Place the spawn room prefab here. This will be the last room built")]
    public GameObject SpawnRoom;
    #endregion

    #region Testing Variables
    public float SpawnRate = 1.0f;                         //The rate at which rooms will spawn. Useful for having time to watch the level build out. Set to 0 if not testing
    public bool IsTesting = false;                         //Check in the editor if you are currently in test mode. This will not require the level to be built over the network. Useful for just seeing a level build with a certain seed.
    public bool DrawGizmos = false;                        //This will allow the MapGeneratorV2 to draw gizmos on the rooms when they attempt to spawn.
    private float m_CreateNewRoomTimer = 0.0f;             //How long it has been since a room was last created.

    private List<Vector3> m_GizmoPositions;                //A list of gizmo positions
    private List<Vector3> m_GizmoSizes;                    //A list of gizmo sizes
    #endregion

    #region Variables for Room Creation
    public float SpawnRadius = 200;                        //The radius around the boss room that other rooms can be spawned in

    private GameObject m_CurrentRoomToBuildFrom;           //This value keeps track of the current IslandRoom being built from
    private List<GameObject> m_RoomsToBuildFrom;           //This list keeps track of all of the IslandRooms that can be built from

    private List<int> m_BuiltUniqueRooms;                  //A list of indices to store every uniquely built room. Keeps things generic and expandable

    private int m_AmountOfRoomsBuilt = 0;                  //The number of rooms built in the level
    private int m_RoomsSinceLastUniqueRoomCreation = 0;    //How many rooms have been created since the last time a "unique" room has been created

    private bool m_IsSpawnRoomSpawned = false;             //Bool to determine if all of the unique rooms have been built yet

    private int m_TwoPortalRoomsInARow = 0;                 //Used to check how many 2 portal rooms have been spawned in a row
    #endregion

    #region Other Variables
    public int RoomLimit = 6;                              //This value represents the number of normal rooms that can be built on this floor. This value will increment each time the scene is reloaded
    public int UniqueRoomRule = 2;                         //How many normal rooms should be built before a unique room is built

    [SerializeField]
    private ulong m_Seed = 69;                             //The seed that is being used in the Unity Random method and the MarsenneTwister.
    [SerializeField]
    private bool m_IsDoneBuildingLevel = false;            //Whether or not the level is finished building out or not.
    private MT19937 m_RandomNumberGenerator;               //An instance of the random number generator, used to seed the level.
    private int m_LayerMask;                               //LayerMask used during the checks in the generator.
    private int m_UniqueName = 0;                          //A unique name which helps identify rooms, but also helps with removing rooms from the m_RoomsToBuildFrom list.

    private static List<GameObject> m_UsedBossRooms;       //A static list of all the used boss rooms
    #endregion

    #region Unity Methods
    private void Awake()
    {
        //error checking for exposed values
        if (BossRoomPrefab.Count < 1)
            Debug.LogError("No boss rooms are attached to the BossRoomPrefab list in the MapGeneratorV2, fix this.", this);
        if (UniqueRooms.Count < 1)
            Debug.LogError("No unique rooms are attached to the UniqueRooms list in the MapGeneratorV2, fix this.", this);
        if (RoomPrefabs.Count < 1)
            Debug.LogError("No rooms are attached to the RoomPrefabs list in the MapGeneratorV2, fix this.", this);
        if (SpawnRoom == null)
            Debug.LogError("No spawn room is attached to the SpawnRoom list in the MapGeneratorV2, fix this.", this);

        //initliaze values
        m_RoomsToBuildFrom = new List<GameObject>();
        m_BuiltUniqueRooms = new List<int>();
        m_GizmoPositions = new List<Vector3>();
        m_GizmoSizes = new List<Vector3>();
        m_LayerMask = LayerMask.GetMask("MapGeometry");

        //intialize the random number generator
        if (!IsTesting)
        {
            m_Seed = (ulong)GameManager.Instance.SeedValue;
        }
        m_RandomNumberGenerator = new MT19937(m_Seed);

        if (m_UsedBossRooms == null)
        {
            m_UsedBossRooms = new List<GameObject>();
        }

        CreateNewBossRoom();

        RoomLimit += GameManager.FloorNumber * 2;

        GameManager.LevelGeneration += GenerateLevel;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        GameManager.LevelGeneration -= GenerateLevel;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnDrawGizmos()
    {
        if (DrawGizmos)
        {
            Gizmos.color = Color.red;
            if (m_GizmoSizes != null)
            {
                for (int i = 0; i < m_GizmoSizes.Count; i++)
                {
                    Gizmos.DrawCube(m_GizmoPositions[i], m_GizmoSizes[i]);
                }
            }
        }
    }
    #endregion

    private void GenerateLevel(object sender, EventArgs args)
    {
        while (!m_IsDoneBuildingLevel)
        {
            if (m_CreateNewRoomTimer >= SpawnRate)
            {
                GenerateMap();
                m_CreateNewRoomTimer = 0.0f;
            }
            else
                m_CreateNewRoomTimer += Time.deltaTime;
        }
    }

    private void GenerateMap()
    {
        //check to see if the spawn room has been built. If it has that means that it is time to close of the remainning Island rooms and finished the map generation process
        if (m_IsSpawnRoomSpawned)
        {
            CapOffRemainingRooms();
        }
        //check to see if it's time to build another special room and that the amount of built unique rooms doesn't match the number of unique rooms in the generator
        else if ((m_RoomsSinceLastUniqueRoomCreation >= UniqueRoomRule) && (m_BuiltUniqueRooms.Count != UniqueRooms.Count))
        {
            UniqueSpawnRoomRule();
        }
        //check to see if we have created the maximum amount of rooms
        else if (m_AmountOfRoomsBuilt < RoomLimit)
        {
            CreateNormalRoom();
        }
        //if the amount of rooms to build has been reached then we need to build the spawn room next
        else if (!m_IsSpawnRoomSpawned)
        {
            CreateSpawnRoom();
        }

        //finally if the amount of rooms to build from has no objects in its list that means that the whole list has been checked
        if (m_RoomsToBuildFrom.Count == 0 &&
            m_IsSpawnRoomSpawned)
        {
            m_IsDoneBuildingLevel = true;
            //TODO: Add the functionality of calling the event delegate in order to progress the level generation to the next step. ItemManager populating rooms and the NavMesh being baked
        }

        //now another room must be chosen to be built off of. This makes it seem more random, kinda.
        PickNewRoomToBuildFromRandomly();
    }

    //This will create a new room from a portal. The other value is used for unique rooms and the spawn room
    private void CreateNewRoom(Portal portalToBuildFrom, GameObject roomToBuild)
    {
        //if the roomToBuild value is null that means that no room was passed in meaning that the Generator doesn't need to make any custom room so we pull a normal room from the list
        GameObject obj = null;
        if (roomToBuild == null)
        {
            //calcaulte a random room from the list of prefabs
            int index = 0;
            if (RoomPrefabs.Count > 1)
            {
                index = m_RandomNumberGenerator.RandomRange(0, RoomPrefabs.Count - 1);
            }

            //set instantiate the room prefab at the given value
            obj = Instantiate(RoomPrefabs[index], m_CurrentRoomToBuildFrom.transform.position, m_CurrentRoomToBuildFrom.transform.rotation);
        }
        else
        {
            //instantiate the specially passed in room
            obj = Instantiate(roomToBuild, m_CurrentRoomToBuildFrom.transform.position, m_CurrentRoomToBuildFrom.transform.rotation);
        }

        //next a random portal will be picked to link the portal that gets passed in
        int randomNumber = m_RandomNumberGenerator.RandomRange(0, obj.GetComponent<IslandRoom>().PortalsInRoom.Count - 1);
        Portal portalToConnect = obj.GetComponent<IslandRoom>().PortalsInRoom[randomNumber];

        //orient the newly created room in random space
        bool hasRoomBeenPlaced = false;
        while (hasRoomBeenPlaced == false)
        {
            //Must be done this way because the random number generator doesn't have a random float or random inside unit sphere. Cannot use UnityEngine.Random because of core
            //problem that it is used different times for different people (client and master client) as well as it's the whole reason for having the random number generator
            Vector3 newPosition = new Vector3();
            newPosition.x = m_RandomNumberGenerator.RandomRange((int)(-SpawnRadius * 1000), (int)(SpawnRadius * 1000)) * 0.001f;
            newPosition.y = m_RandomNumberGenerator.RandomRange((int)(-SpawnRadius * 1000), (int)(SpawnRadius * 1000)) * 0.001f;
            newPosition.z = m_RandomNumberGenerator.RandomRange((int)(-SpawnRadius * 1000), (int)(SpawnRadius * 1000)) * 0.001f;
            obj.transform.position = newPosition;

            //TODO: in the future if rotating the rooms becomes a problem it must be fixed in this location before the cast point check is done

            //next we must check to see if that new location is a valid position for this room to be located using the cast point attached to the room
            CastPoints point = obj.GetComponentInChildren<CastPoints>();
            //if this check succeeds it means that the room was placed in a valid location in the world. If this check fails that means the location is invalid and the loop must run again
            if (CanRoomBeBuiltAtLocation(obj, point.transform.TransformPoint(point.center), point.transform.rotation, point.HalfExtents))
            {
                hasRoomBeenPlaced = true;
            }
        }

        //if we have arrived at this location it means that the room was placed in the world without any conflicts. The next thing to do is clean up for things like attaching the portals togeather
        RoomManager.Instance.ConnectedPortalsTogeather(portalToBuildFrom, portalToConnect);

        //if the room that was built is a normal room then things need to be updated in order to reflect the fact that a new room was created and added to the scene
        if (roomToBuild == null)
        {
            m_UniqueName++;                         //since we know the room will exist we can give it a "unique" name. Increment the unique name counter
            obj.name = m_UniqueName.ToString();     //set the newly spawned object's name to be that of the unique name value.

            m_AmountOfRoomsBuilt++;                 //increment the amount of rooms built value
            m_RoomsSinceLastUniqueRoomCreation++;   //increment the amount of rooms built since the last unique room was built value

            //add the newly created room to the list of rooms that can be built from
            m_RoomsToBuildFrom.Add(obj);
        }
        else
        {
            obj.name = roomToBuild.name;            //set the unique rooms name to be that rooms name, just without the (Clone) at the end
        }

        RoomManager.Instance.AddRoomToList(obj.GetComponent<IslandRoom>());
    }

    //This method will create the spawn room
    private void CreateSpawnRoom()
    {
        //the current room that is being built from, since this while loop may loop many times
        IslandRoom currentRoom = m_CurrentRoomToBuildFrom.GetComponent<IslandRoom>();

        while (true)
        {
            //generate a random number to be used for determinning what portal in the room to use
            int randomPortal = 0;
            if (currentRoom.PortalsInRoom.Count > 1)
            {
                randomPortal = m_RandomNumberGenerator.RandomRange(0, m_CurrentRoomToBuildFrom.GetComponent<IslandRoom>().PortalsInRoom.Count - 1);
            }

            //if the portal that might be used isn't connected to another portal then that means it can be built from.
            if (currentRoom.PortalsInRoom[randomPortal].ConnectedPortal == null)
            {
                CreateNewRoom(currentRoom.PortalsInRoom[randomPortal], SpawnRoom);

                m_IsSpawnRoomSpawned = true;
                //return in order to escape this method
                return;
            }
        }
    }

    //This method will create a normal room
    private void CreateNormalRoom()
    {
        //the current room that is being built from, since this while loop may loop many times
        IslandRoom currentRoom = m_CurrentRoomToBuildFrom.GetComponent<IslandRoom>();

        while (true)
        {
            //generate a random number to be used for determinning what portal in the room to use
            int randomPortal = 0;
            if (currentRoom.PortalsInRoom.Count > 1)
            {
                randomPortal = m_RandomNumberGenerator.RandomRange(0, m_CurrentRoomToBuildFrom.GetComponent<IslandRoom>().PortalsInRoom.Count - 1);
            }

            //if the portal that might be used isn't connected to another portal then that means it can be built from.
            if (currentRoom.PortalsInRoom[randomPortal].ConnectedPortal == null)
            {
                CreateNewRoom(currentRoom.PortalsInRoom[randomPortal], null);
                //return in order to escape this method
                return;
            }
        }
    }

    //this method will be the beginning of a unique room being created
    private void UniqueSpawnRoomRule()
    {
        //get the current room to build from.
        IslandRoom room = m_CurrentRoomToBuildFrom.GetComponent<IslandRoom>();

        while (true)
        {
            //choose a random room from the unique rooms and compare it against the unique rooms that are already built in the m_BuiltUniqueRooms dictionary
            int randomUniqueRoomIndex = 0;
            if (UniqueRooms.Count > 1)
            {
                randomUniqueRoomIndex = m_RandomNumberGenerator.RandomRange(0, UniqueRooms.Count - 1);
            }

            if (!m_BuiltUniqueRooms.Contains(randomUniqueRoomIndex))
            {
                //instead of finding a random portal to build from we're just going to find the first one as to not have nested while loops
                foreach (Portal portal in room.PortalsInRoom)
                {
                    if (portal.ConnectedPortal == null)
                    {
                        //now we build the room
                        CreateNewRoom(portal, UniqueRooms[randomUniqueRoomIndex]);

                        //before we end this build we need to do a few things:
                        //1. the unique room and index need to be added to the list 
                        m_BuiltUniqueRooms.Add(randomUniqueRoomIndex);
                        //2. we need to rest the amount of rooms since unique room was built value to zero
                        m_RoomsSinceLastUniqueRoomCreation = 0;
                        //finally we can return
                        return;
                    }
                }
            }
        }
    }

    //Checks to see if a room can be built at the described location
    private bool CanRoomBeBuiltAtLocation(GameObject obj, Vector3 origin, Quaternion rotation, Vector3 halfExtents)
    {
        m_GizmoSizes.Add(halfExtents * 2);
        m_GizmoPositions.Add(origin);

        Collider[] colliders = Physics.OverlapBox(
            origin,
            halfExtents,
            rotation,
            m_LayerMask);

        foreach (Collider coolider in colliders)
        {
            if (coolider.gameObject.transform.IsChildOf(obj.transform))
            {
                continue;
            }

            //if any of the colliders are MapGeometry and they are not a child of the room that means that some actual geometry was hit, cast failed
            if (coolider.tag == "MapGeometry")
            {
                return false;
            }
        }

        //if the loop was escaped from that means that nothing was overlapping, which means that cast was a success
        return true;
    }

    //Disables all of the unused portals in the room and then 
    private void CapOffRemainingRooms()
    {
        //loop through every room in the list of rooms and disable all other none used portals        
        foreach (GameObject obj in m_RoomsToBuildFrom)
        {
            FinishWithRoom(obj.GetComponent<IslandRoom>());
        }

        //now the rooms to build from list can be cleared because there is nothing else to deal with in the list
        m_RoomsToBuildFrom.Clear();
    }

    //This will pick a new room to build from randomly from the list of rooms that can be built from
    private void PickNewRoomToBuildFromRandomly()
    {
        //if there are no more rooms to choose from randomly then don't try to pick a new one to spawn objects from
        if (m_RoomsToBuildFrom.Count < 1)
            return;

        //create a bool to determine whether or not a new room has been chosen to build from
        bool roomChosen = false;

        //while a room hasn't been chosen this logic will continue
        while (roomChosen == false)
        {
            //create a random number using the marsenne twister
            int randomNumber = 0;
            if (m_RoomsToBuildFrom.Count > 1)
            {
                randomNumber = m_RandomNumberGenerator.RandomRange(0, (m_RoomsToBuildFrom.Count - 1));             
            }
            //get the IslandRoom component which will be used to check if the useable portals number is equal to the currently used portal count.
            IslandRoom room = m_RoomsToBuildFrom[randomNumber].GetComponent<IslandRoom>();

            //double check to see how many 2 door rooms have been spawned in a row
            bool canBuildRoom = true;
            if(m_TwoPortalRoomsInARow >= 1)
            {
                //if 2 or more 2 portal rooms have been spawned and this room only has 2 useable portals then don't let this room spawn
                if (room.PortalsInRoom.Count == 2)
                    canBuildRoom = false;                        
            }

            if (canBuildRoom)
            {
                //if the values aren't the same then the room can built from.
                if (room.UsablePortals != room.CurrentlyUsedPortalsCount)
                {
                    m_CurrentRoomToBuildFrom = m_RoomsToBuildFrom[randomNumber];
                    roomChosen = true;

                    m_TwoPortalRoomsInARow = (room.PortalsInRoom.Count == 2 ? (m_TwoPortalRoomsInARow + 1) : 0);

                }
                //if the values are the same then that room cannot be built from and should be removed from the list
                else
                {
                    FinishWithRoom(room);
                    m_RoomsToBuildFrom.RemoveAt(randomNumber);
                }
            }
        }
    }

    //Used to disable every none in use portal in the IslandRoom that is passed in
    private void FinishWithRoom(IslandRoom room)
    {
        //iterate through the portals in the room
        foreach (Portal portal in room.PortalsInRoom)
        {
            //if the connected portal is null that means no portal has been attached and this portal must be DESTROYED. IE disable the gameobject
            if (portal.ConnectedPortal == null)
            {
                Destroy(portal.gameObject);
            }
        }
    }

    private void CreateNewBossRoom()
    {
        //check to see if any of the boss rooms have already been used
        foreach (GameObject bossRoom in m_UsedBossRooms)
        {
            if (BossRoomPrefab.Contains(bossRoom))
            {
                BossRoomPrefab.Remove(bossRoom);
            }
        }

        int index = 0;

        //make sure that we can pull a valid room from the generator, because the RNG will give you a range from the lo value to high value including both those numbers
        if (BossRoomPrefab.Count > 1)
            index = m_RandomNumberGenerator.RandomRange(0, BossRoomPrefab.Count - 1);

        GameObject obj = Instantiate(BossRoomPrefab[index]);

        //once the boss room is used we no longer need to try to pull it again so remove it from the list
        m_UsedBossRooms.Add(obj);

        //place the room at the origin, (0,0,0)
        obj.transform.position = Vector3.zero;
        //set the boss room to be the room we are trying to build from
        m_CurrentRoomToBuildFrom = obj;

        RoomManager.Instance.AddRoomToList(obj.GetComponent<IslandRoom>());
    }

    public ulong Seed { get { return m_Seed; } set { m_Seed = value; } }
}