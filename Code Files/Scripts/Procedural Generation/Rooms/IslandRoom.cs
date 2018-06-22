using System.Collections.Generic;
using UnityEngine;
using TheNegative.AI;

//Writer: Liam
//Laste Updated: 1/15/2017

[RequireComponent(typeof(UnityEngine.AI.NavMeshSurface))]
public class IslandRoom : MonoBehaviour
{
    public GameObject DetailedMesh;                     //Detailed mesh 
    public GameObject LODMesh;                          //The LOD version of this current room
    public List<EnemySpawnPoint> RoomSpawnPoints;       //List of spawn points in the Island Room
    public List<Portal> PortalsInRoom;                  //List of portals in the Island Room
    public LevelTransitionPortal TransitionPortal;      //Transition portal for specific rooms that allow for transitions
    public RoomType TypeOfRoom;                         //The type of room that this Island Room is
    [Tooltip("This value will determine how many of the portals in this room can be used before this room cannot have any other rooms built from it.")]
    public int UsablePortals = 3;

    [SerializeField]
    private List<Player> m_PlayersInRoom;               //List of the players in the room
    [SerializeField]
    private List<AI> m_EnemiesInRoom;                    //List of the enemies in the room
    [SerializeField]
    private int m_EnemiesInRoomCount = 100;             //Determines the amount of enemies still alive in the room
    private bool m_IsRoomCleared = false;               //Determines whether or not the room has been cleared of enemies yet or not
    private int m_CurrentlyUsedPortalsCount = 0;        //Determines the amount of used portals in this room.	    
    private ItemSpawnPoint m_ItemSpawnPosition;         //The location in the room at which objects are spawned from

    private void Awake()
    {
        //initalize private lists
        m_PlayersInRoom = new List<Player>();
        m_EnemiesInRoom = new List<AI>();

        //"error checking", making sure that the lists of values actually have there appropriate values within them. This is for spawn points and portals
        EnemySpawnPoint[] spawnPoints = GetComponentsInChildren<EnemySpawnPoint>();
        foreach (EnemySpawnPoint point in spawnPoints)
        {
            if (!RoomSpawnPoints.Contains(point))
                RoomSpawnPoints.Add(point);
        }

        m_ItemSpawnPosition = GetComponentInChildren<ItemSpawnPoint>();

        Portal[] portals = GetComponentsInChildren<Portal>();
        foreach (Portal portal in portals)
        {
            if (!PortalsInRoom.Contains(portal))
                PortalsInRoom.Add(portal);
        }

        //next we must change all of the spawn points in this room to know that they're in this room
        foreach (EnemySpawnPoint point in RoomSpawnPoints)
        {
            point.SpawnPointRoom = this;
        }

        //finally we do the same thing but with the portals, this way they know what room they're in
        foreach (Portal portal in PortalsInRoom)
        {
            portal.Room = this;
        }

        //double check if the useable count isn't greater then the portals in room count
        if (UsablePortals > PortalsInRoom.Count)
        {
            UsablePortals = PortalsInRoom.Count;
        }     
    }

    private void LateUpdate()
    {
        //if the room isn't cleared but the room count is less then or equal to zero that means the room needs to be opened up
        if (m_EnemiesInRoomCount <= 0 && !m_IsRoomCleared)
        {
            OpenIslandRoom();
        }
    }

    //increments the enemies in room counter
    public void IncrementEnemyCount()
    {
        m_EnemiesInRoomCount++;
    }

    //decrements the enemies in room counter
    public void DecrementEnemyCount()
    {
        m_EnemiesInRoomCount--;
    }

    public void CloseIslandRoom()
    {
        //TODO: make sure that the deactivate returns a bool and then until the value is true keep doing this logic
        m_IsRoomCleared = false;

        foreach (Portal portal in PortalsInRoom)
        {
            portal.DeactivatePortal();
        }

        if (TransitionPortal != null)
            TransitionPortal.gameObject.SetActive(false);

        if(m_PlayersInRoom.Count < 1)   
            TogglePortalFXLayer(false);
    }

    public void AddEnemyToRoom(AI ai)
    {
        if (!m_EnemiesInRoom.Contains(ai))
            m_EnemiesInRoom.Add(ai);
        m_EnemiesInRoomCount = m_EnemiesInRoom.Count;
    }

    public void RemoveEnemyFromRoom(AI ai)
    {
        if (m_EnemiesInRoom.Contains(ai))
            m_EnemiesInRoom.Remove(ai);        
    }

    public AI GetRandomEnemy()
    {
        return m_EnemiesInRoom[Random.Range(0, m_EnemiesInRoom.Count)];
    }

    //This will find a random AI who's transform doesn't match transform t's root transform
    public AI GetRandomEnemy(Transform t)
    {
        if (m_EnemiesInRoom.Count == 1)
            return null;

        AI randomTarget = null;
        while(randomTarget == null)
        {
            AI currentTarget = GetRandomEnemy();

            if (currentTarget.transform.root != t.root)
            {
                randomTarget = currentTarget;
            }
        }

        return randomTarget;
    }

    public void OpenIslandRoom()
    {      
        //set the IslandRoom to be cleared
        m_IsRoomCleared = true;
        SoundManager.PlayAmbient();

        //calcualte the average luck of the players in the rooms so that it can be used to influence what type of items spawn into the room
        float averageLuck = 0.0f;
        //for every player that's in this room when it's cleared update the room based cooldown
        foreach (Player player in m_PlayersInRoom)
        {
            ItemManager.Instance.UpdateRoomBasedCooldown(player.PlayerNumber);
            averageLuck += player.GetStat(StatType.LUCK);
        }

        averageLuck /= m_PlayersInRoom.Count;
        if (m_ItemSpawnPosition != null)
            ItemManager.Instance.SpawnItemFromRoom(m_ItemSpawnPosition.transform.position, averageLuck, TypeOfRoom);
        else if (TypeOfRoom != RoomType.Boss && TypeOfRoom != RoomType.Shop && TypeOfRoom != RoomType.Spawn && TypeOfRoom != RoomType.Treasure)
            Debug.LogError("You need to put an item spawn location in this room, room number " + name + ", in order for items to spawn in this room.", this);


        if(TransitionPortal != null)
        {
            TransitionPortal.gameObject.SetActive(true);
        }

        TogglePortalFXLayer(true);

        //loop through each active portal and reactivate the portals in the room
        foreach (Portal portal in PortalsInRoom)
        {
            portal.ReactivatePortal();
        }
    }

    public void TogglePortalFXLayer(bool trigger)
    {
        foreach(Portal portal in PortalsInRoom)
        {
            portal.PortalEffect.gameObject.SetActive(trigger);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //get the player reference from the collider
        Player player = other.GetComponent<Player>();
        //if the player isn't null then add them to the room, set the player's room to be this room, and activate any AI in the room that aren't activated
        if (player != null)
        {
            Debug.Log("player " + player.name + " entered room " + name + ".");

            m_PlayersInRoom.Add(player);

            //swap the cameras of the newly activated room
            PortalManager.SwapToNewCamera(player.MyIslandRoom, this);

            //swap any additional room's LOD version with the appropriate version
            //RoomManager.Instance.SwapLODs(this);

            player.MyIslandRoom = this;

            foreach (AI ai in m_EnemiesInRoom)
            {
                if (ai != null)
                    if (!ai.enabled)
                    {
                        ai.SetActive(true);
                        ai.enabled = true;
                        ai.ActivateEnemy();
                    }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //get the player component and if it isn't null remove them from the room
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            m_PlayersInRoom.Remove(player);
        }
    }

    public int CurrentlyUsedPortalsCount { get { return m_CurrentlyUsedPortalsCount; } set { m_CurrentlyUsedPortalsCount = value; } }
    public List<Player> PlayersInRoom { get { return m_PlayersInRoom; } }
    public List<AI> EnemiesInRoom { get { return m_EnemiesInRoom; } }
    public int EnemiesInRoomCount { get { return m_EnemiesInRoomCount; } set { m_EnemiesInRoomCount = value; } }
    public bool IsRoomCleared { get { return m_IsRoomCleared; } set { m_IsRoomCleared = value; } }
}