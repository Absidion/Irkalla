using System;
using System.Collections.Generic;
using UnityEngine;
using TheNegative.AI;
using UnityEngine.SceneManagement;

public class RoomManager : Photon.MonoBehaviour, IPunObservable
{
    public Color BossPortalColor;
    public Color ShopPortalColor;
    public Color TreasurePortalColor;
    public Color DefaultPortalColor;

    public bool ShowRoomConnections = false;
    public static RoomManager Instance;
    

    public int DynamicEnemyName = 0;
    [SerializeField]
    private List<IslandRoom> m_RoomList;            //A list to keep track of all of the rooms inside of the scene
    private List<LineRenderer> m_LineRenderers;     //A list of line renderers which will show a visual connection between portals

    void Awake()
    {
        //initialize the list of rooms
        if (m_RoomList == null)
        {
            m_RoomList = new List<IslandRoom>();
        }
        Instance = this;
        
        m_LineRenderers = new List<LineRenderer>();
        GameManager.PostPlayer += FinalizeRooms;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        GameManager.PostPlayer -= FinalizeRooms;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnValidate()
    {
        if (m_LineRenderers == null)
            return;

        foreach (LineRenderer line in m_LineRenderers)
        {
            line.enabled = ShowRoomConnections;
        }
    }

    private void FinalizeRooms(object sender, EventArgs args)
    {
        List<Portal> portalsToRemove = new List<Portal>();

        foreach (IslandRoom room in m_RoomList)
        {
            foreach (Portal portal in room.PortalsInRoom)
            {
                if (portal == null)
                    portalsToRemove.Add(portal);
            }

            foreach(Portal portal in portalsToRemove)
            {
                room.PortalsInRoom.Remove(portal);
            }
        }
        ResetAllRooms();

        if(PhotonNetwork.isMasterClient)
            SyncEnemies();
    }

    //adds a room to our list of rooms in the scene
    public void AddRoomToList(IslandRoom room)
    {
        if (!m_RoomList.Contains(room))
        {
            m_RoomList.Add(room);
        }
    }

    //decrement the enemy count in a room
    public void DecrementEnemyCount(IslandRoom room)
    {
        if (m_RoomList.Contains(room))
        {
            room.DecrementEnemyCount();
        }
    }


    //increment the enemy count in a room
    public void IncrementEnemyCount(IslandRoom room)
    {
        if (m_RoomList.Contains(room))
        {
            room.IncrementEnemyCount();
        }
    }

    private void ResetAllRooms()
    {
        foreach(IslandRoom room in m_RoomList)
        {        
            room.CloseIslandRoom();
            room.IsRoomCleared = false;            
        }
    }

    public void ConnectedPortalsTogeather(Portal portalA, Portal portalB)
    {
        //connected the portals togeather
        portalA.ConnectedPortal = portalB;
        portalB.ConnectedPortal = portalA;

        //save the connected room
        portalA.ConnectedRoom = portalB.Room;
        portalB.ConnectedRoom = portalA.Room;

        //swap the portal materials with each other
        Material tempMat = portalA.Renderer.material;
        portalA.Renderer.material = portalB.Renderer.material;
        portalB.Renderer.material = tempMat;

        //increment the CurrentlyUsedPortal count in both portal's room value
        portalA.Room.CurrentlyUsedPortalsCount++;
        portalB.Room.CurrentlyUsedPortalsCount++;

        //add a new line renderer between the two portals
        GameObject obj = new GameObject();
        obj.transform.parent = this.transform;

        LineRenderer line = obj.AddComponent<LineRenderer>();
        line.SetPosition(0, portalA.transform.position);
        line.SetPosition(1, portalB.transform.position);
        line.useWorldSpace = true;
        line.enabled = false;

        m_LineRenderers.Add(line);

        portalA.SetPortalEffectLayer();
        portalB.SetPortalEffectLayer();
    }

    public void AddEnemyToRoom(AI ai, IslandRoom room)
    {
        //set ai name over network
        if (ai.name.Contains("(Clone)"))
        {
            string dynamicName = ai.name;
            dynamicName = dynamicName.Replace("(Clone)", "");
            ai.SetName(dynamicName + DynamicEnemyName.ToString());
            DynamicEnemyName++;
        }
        //add the enemy to the room and set the AI's room
        room.EnemiesInRoom.Add(ai);
        room.EnemiesInRoomCount = room.EnemiesInRoom.Count;
        ai.MyIslandRoom = room;

        ai.gameObject.SetActive(false);
        ai.enabled = false;
    }

    public void SwapLODs(IslandRoom roomBeingEntered)
    {
        List<IslandRoom> connectedRooms = new List<IslandRoom>();
        connectedRooms.Add(roomBeingEntered);

        for(int i = 0; i < roomBeingEntered.PortalsInRoom.Count; i++)
        {
            if(roomBeingEntered.PortalsInRoom[i] != null)
                connectedRooms.Add(roomBeingEntered.PortalsInRoom[i].ConnectedRoom);
        }        
        
        foreach(IslandRoom room in m_RoomList)
        {
            //if the room in the room list contains the room being iterated through then that means we need to use the high poly version because the player will have direct contact with this portal
            if(connectedRooms.Contains(room))
            {
                if (room.LODMesh.activeInHierarchy == true)
                    room.LODMesh.SetActive(false);

                if (room.DetailedMesh.activeInHierarchy == false)
                    room.DetailedMesh.SetActive(true);
            }
            //otherwise this mesh can get the LOD version
            else
            {
                if(room.LODMesh.activeInHierarchy == false)
                    room.LODMesh.SetActive(true);

                if(room.DetailedMesh.activeInHierarchy == true)
                    room.DetailedMesh.SetActive(false);
            }
        }
    }

    public Color GetColorFromRoomType(RoomType type)
    {
        Color color = Color.clear;
        switch (type)
        {         
            case RoomType.Boss:
                color = BossPortalColor;
                break;

            case RoomType.Shop:
                color = ShopPortalColor;
                break;

            case RoomType.Treasure:
                color = TreasurePortalColor;
                break;

            case RoomType.Spawn:
                color = Color.white;
                break;

            default:
                color = DefaultPortalColor;
                break;
        }
        return color;
    }

    private void SyncEnemies()
    {
        foreach (IslandRoom room in m_RoomList)
        {
            foreach (AI ai in room.EnemiesInRoom)
            {
                photonView.RPC("RPCFindAIandIslandRoomThenInit", PhotonTargets.All, room.name, ai.name);
            }
        }
    }

    [PunRPC]
    private void RPCFindAIandIslandRoomThenInit(string roomName, string aiName)
    {
        GameObject roomGO = GameObject.Find(roomName);
        AI[] aiInGame = Resources.FindObjectsOfTypeAll<AI>();
        IslandRoom room = roomGO.GetComponent<IslandRoom>();
        AI ai = null;

        foreach(AI individualAI in aiInGame)
        {
            if(individualAI.name == aiName)
            {
                ai = individualAI;
                break;
            }
        }

        if (!PhotonNetwork.isMasterClient)
        {
            if(room != null && ai != null)
            {
                AddEnemyToRoom(ai, room);
            }
        }

        ai.Init();
        ai.enabled = false;
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream == null)
            return;

        if (stream.isWriting && photonView.isMine)
        {
            for (int i = 0; i < m_RoomList.Count; i++)
            {
                stream.SendNext(m_RoomList[i].EnemiesInRoom.Count);
            }

            stream.SendNext(DynamicEnemyName);

        }
        if (stream.isReading && !photonView.isMine)
        {
            for (int i = 0; i < m_RoomList.Count; i++)
            {
                m_RoomList[i].EnemiesInRoomCount = (int)stream.ReceiveNext();
            }

            DynamicEnemyName = (int)stream.ReceiveNext();
        }
    }
}

public enum RoomType
{
    Small,
    Medium,
    Large,
    ExtraLarge,
    Boss,
    Shop,
    Treasure,
    Spawn
}