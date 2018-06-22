using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//Author: Josue
//Last edited; 12/04/2017

public class DeselectLobbyRoom : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private GameObject m_CreateButton;
    private EventSystem m_EventSystem;
    private bool m_selected = false;

    private void Start()
    {
        m_CreateButton = GameObject.Find("Create Room");
        m_EventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        m_EventSystem.SetSelectedGameObject(m_CreateButton);
    }

    private void Update()
    {
        if (m_selected)
        {
            if (Input.GetButtonUp("Cancel"))
            {
                m_EventSystem.SetSelectedGameObject(m_CreateButton);   
            }
        }
    }


    public void OnSelect(BaseEventData eventData)
    {
        m_selected = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        m_selected = false;
    }
}
