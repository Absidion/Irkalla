using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalManager : MonoBehaviour
{
    public float MaximumPortalCameraRotationX = 70.0f;
    public float MinimumPortalCameraRotationX = -70.0f;

    private List<Portal> m_Portals;
    private Transform m_MainCamera;


    void Awake()
    {
        m_Portals = new List<Portal>();
        m_MainCamera = Camera.main.transform;
        GameManager.PostPlayer += GetActivePortalCameras;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        GameManager.PostPlayer -= GetActivePortalCameras;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    void Update()
    {
        foreach (Portal portal in m_Portals)
        {
            if (portal.enabled != true)
                continue;

            //calcualte the position change of the camera
            float heightChange = m_MainCamera.transform.position.y - portal.ConnectedPortal.transform.position.y;
            portal.PortalCamera.transform.position = new Vector3
                (portal.PortalCamera.transform.position.x,
                Mathf.Clamp(portal.transform.position.y + heightChange, portal.transform.position.y + portal.CameraMinHeight, portal.transform.position.y + portal.CameraMaxHeight),
                portal.PortalCamera.transform.position.z);

            //move the forward direction of the camera
            Vector3 reflectedDir = Vector3.Reflect(m_MainCamera.forward, portal.ConnectedPortal.transform.forward);
            reflectedDir = portal.ConnectedPortal.transform.InverseTransformDirection(reflectedDir);
            reflectedDir = portal.transform.TransformDirection(reflectedDir);

            Vector3 normalizeDir = ((portal.transform.position) - (portal.transform.position + reflectedDir)).normalized;
            normalizeDir.y = -normalizeDir.y;
            Vector3 newReflectedVector = Vector3.Reflect(normalizeDir, portal.transform.forward);
            //set the direction
            portal.PortalCamera.transform.forward = newReflectedVector;
        }
    }

    private void GetActivePortalCameras(object sender, EventArgs args)
    {
        //get every active portal in the scene
        Portal[] portals = FindObjectsOfType<Portal>();
        foreach (Portal portal in portals)
        {
            if (portal != null)
            {
                //turn off the camera and add the camera to the list of them in the scene
                portal.PortalCamera.enabled = false;
                m_Portals.Add(portal);
            }
        }

        //find all rooms and the then iterate through until the spawn room is found
        IslandRoom[] rooms = FindObjectsOfType<IslandRoom>();
        foreach (IslandRoom room in rooms)
        {
            if (room.TypeOfRoom == RoomType.Spawn)
            {
                //iterate through the room that is connected to the spawn room's portals
                foreach (Portal connectedRoomPortal in room.PortalsInRoom[0].ConnectedRoom.PortalsInRoom)
                {
                    //enabled the room that is connected to the spawn room's cameras
                    if (connectedRoomPortal.PortalCamera != null) 
                       connectedRoomPortal.PortalCamera.enabled = true;
                }
                break;
            }
        }
    }

    public static void SwapToNewCamera(IslandRoom oldRoom, IslandRoom newRoom)
    {
        if(oldRoom != null)
        {
            foreach(Portal portal in oldRoom.PortalsInRoom)
            {
                //disable all of the portal camera's of the room being exited
                portal.PortalCamera.enabled = false;
                //disable all of the portals that where connected to the previous room's portals
                portal.ConnectedPortal.PortalCamera.enabled = false;
            }            
        }

        if(newRoom != null)
        {
            foreach (Portal portal in newRoom.PortalsInRoom)
            {
                //disable all of the portal camera's in the room that I'm entering
                portal.PortalCamera.enabled = false;
                //turn the portal camera for the connected portal to be on
                portal.ConnectedPortal.PortalCamera.enabled = true;
            }
        }
    }
}
