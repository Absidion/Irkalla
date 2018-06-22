using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Author: Josue
//Last edited: 12/4/2017

public class ConnectRoomScrollView : MonoBehaviour
{
    public Button CreateButton;         //reference to Create Room button
    public List<Button> RoomButtons;    //reference to list of room lobbie UI elements

    private int m_OldRoomCount = 0;     //old room count before photon room list is updated
	
	void Update()
    {
        //if new room is in the lobby
	   	if (m_OldRoomCount != PhotonNetwork.GetRoomList().Length)
        {
            int numberOfRooms = PhotonNetwork.GetRoomList().Length;

            for (int i = 0; i < numberOfRooms; i++)
            {
                //if first room
                if (i == 0)
                {
                    Navigation nav = RoomButtons[i].navigation;
                    nav.selectOnUp = CreateButton;
                    RoomButtons[i].navigation = nav;
                }

                //if last room
                if (i == numberOfRooms - 1)
                {
                    if (numberOfRooms > 1)
                    {
                        Navigation previousNav = RoomButtons[i - 1].navigation;
                        previousNav.selectOnDown = RoomButtons[i];              //second-last room move down = last room
                        RoomButtons[i - 1].navigation = previousNav;
                    }

                    Navigation createNav = CreateButton.navigation;
                    Navigation lastNav = RoomButtons[i].navigation;

                    lastNav.selectOnDown = CreateButton;                    //last room move down = create button
                    createNav.selectOnUp = RoomButtons[i];                  //create button move up = last room
                    createNav.selectOnDown = RoomButtons[0];                //create button move down = first room

                    RoomButtons[i].navigation = lastNav;
                    CreateButton.navigation = createNav;
                }

                //if not first or last room
                if (i != 0 && i != numberOfRooms - 1)
                {
                    Navigation previousNav = RoomButtons[i - 1].navigation;
                    Navigation currentNav = RoomButtons[i].navigation;

                    previousNav.selectOnDown = RoomButtons[i];              //previous room move down = this room
                    currentNav.selectOnUp = RoomButtons[i - 1];             //this room move up = previous room

                    RoomButtons[i - 1].navigation = previousNav;
                    RoomButtons[i].navigation = currentNav;
                }
            }

            //reset the old room count
            m_OldRoomCount = numberOfRooms;
        }
	}
}
