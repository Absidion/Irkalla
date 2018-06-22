using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Liam
//Last Edited : Liam 10/2/2017

public class FirstPersonCamera : MonoBehaviour
{
    public FirstPersonBehaviour FirstPerson;
    public SpectatorBehaviour SpectatorCamera;

    private Player m_Player;
    private CameraBehaviour m_CurrentBehaviour;
    private Camera m_Camera;    

    void Awake()
    {
        //StatusManager.CreateStatusList();
        m_Camera = Camera.main;
        GameManager.Instance.DontDestroyNormalObject(gameObject);
    }

    void Update()
    {
        //check to see if the player is null, or if the player isn't the local player
        if (m_Player == null)
            return;

        if (!m_Player.photonView.isMine)
            return;

        //if the player isn't dead then we use the first person camera
        if (!m_Player.Health.IsDead)
        {
            SetCameraBehaviour(FirstPerson);
        }
        //if the player is dead then the camera behaviour should change to the spectator
        else if (m_Player.Health.IsDead)
        {
            SetCameraBehaviour(SpectatorCamera);
        }
    }

    void LateUpdate()
    {
        //check to see if the player is null, or if the player isn't the local player
        if (m_Player == null)
            return;
        if (!m_Player.photonView.isMine)
            return;

        //Update camera behaviour
        if (m_CurrentBehaviour != null)
        {
            m_CurrentBehaviour.UpdateCamera();
        }
    }

    public void SetPlayer(Player player)
    {
        //set the player value
        m_Player = player;

        //initialize the first person camera
        FirstPerson.Init(player, this);

        //initialize the spectator behaviour
        SpectatorCamera.Init(player, this);

        //sets the camera behaviour to be the first person behaviour once the player is set
        SetCameraBehaviour(FirstPerson);
    }

    void SetCameraBehaviour(CameraBehaviour behaviour)
    {
        //if the current behaviour is equal to the passed in behaviour then return
        if (m_CurrentBehaviour == behaviour)
        {
            return;
        }

        //if the current behaviour isn't null
        if (m_CurrentBehaviour != null)
        {
            //call the deactivate method of the newly set behaviour
            m_CurrentBehaviour.Deactivate();
        }

        //set the current behaviour to the behaviour passed in
        m_CurrentBehaviour = behaviour;

        if (m_CurrentBehaviour != null)
        {
            //call the activate method of the newly set behaviour
            m_CurrentBehaviour.Activate();
        }
    }

    public Camera Camera { get { return m_Camera; } }
}
