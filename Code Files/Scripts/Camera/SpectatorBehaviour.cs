using System;
using System.Collections.Generic;
using UnityEngine;

//Author: Liam
//Last Edited : Liam 10/2/2017

[Serializable]
public class SpectatorBehaviour : CameraBehaviour
{
    private List<Player> m_OtherPlayersInGame;
    private LayerMask m_HitMask;
    private GameObject m_SpectatorPosition;

    private int m_PlayerBeingSpectated = 0;
    private float m_SwapViewTimer = 0.0f;

    public override void Init(Player player, FirstPersonCamera cam)
    {
        //call the base Init
        base.Init(player, cam);
        //set the layer mask to collide with everything but the player
        m_HitMask = ~(LayerMask.GetMask("Player") | LayerMask.GetMask("Room") | LayerMask.GetMask("Door"));
        //initialize the list of players in the game
        m_OtherPlayersInGame = new List<Player>();
    }

    public override void Activate()
    {
        base.Activate();

        //get the list of players in the game
        Player[] players = GameObject.FindObjectsOfType<Player>();
        if (players.Length == 2)
        {
            for (int i = 0; i < players.Length; i++)
            {
                //check to make sure that A. the player in the list isn't the default player and B. that we don't already have the player in the list
                if ((players[i] != m_Player) && !m_OtherPlayersInGame.Contains(players[i]))
                {
                    m_OtherPlayersInGame.Add(players[i]);
                }
            }
            //set the player being spectate to be the first one in the list
            m_PlayerBeingSpectated = 0;

            m_SpectatorPosition = m_OtherPlayersInGame[m_PlayerBeingSpectated].SpectatorPosition;
        }
    }

    public override void UpdateCamera()
    {
        //if there is more then 1 player in the list then update the abilit to change perspectives otherwsie don't
        if (m_OtherPlayersInGame.Count > 1)
        {
            if ((CharacterController.GetMovementThisFrame().magnitude > Mathf.Epsilon) && (m_SwapViewTimer > 1.0f))
            {
                SpectateNextPlayer();
            }
            else
            {
                m_SwapViewTimer += Time.deltaTime;
            }
        }

        if (m_SpectatorPosition == null)
            return;

        //update the position of the camera so that it follows the other player slightly behind
        m_Camera.transform.position = m_SpectatorPosition.transform.position;
        m_Camera.transform.LookAt(m_OtherPlayersInGame[m_PlayerBeingSpectated].transform.position);

        //check to see if there are any obstacles between the camera and the player. If there is fade them out.
        HandleObstacles();
    }

    private void SpectateNextPlayer()
    {
        if(CharacterController.GetHorizontalAxis() > 0.0f)
        {
            //increment the player that is being spectated value
            m_PlayerBeingSpectated++;
            //however if the current player index is equal to the count that means we went over the number players in our list, so we reset to the first player value
            if(m_OtherPlayersInGame.Count == m_PlayerBeingSpectated)
            {
                m_PlayerBeingSpectated = 0;
            }

            m_SpectatorPosition = m_OtherPlayersInGame[m_PlayerBeingSpectated].SpectatorPosition;
        }
        else if(CharacterController.GetHorizontalAxis() < 0.0f)
        {
            //decrement the counter
            m_PlayerBeingSpectated--;
            //however if the value is less then zero we need to set it to be the player at the end of the list
            if(m_PlayerBeingSpectated < 0)
            {
                m_PlayerBeingSpectated = m_OtherPlayersInGame.Count - 1;
            }

            m_SpectatorPosition = m_OtherPlayersInGame[m_PlayerBeingSpectated].SpectatorPosition;
        }
    }

    private void HandleObstacles()
    {
        //calculate the ray direction and return early if the ray has a length of 0
        Vector3 rayStartPosition = m_OtherPlayersInGame[m_PlayerBeingSpectated].transform.TransformPoint(Vector3.zero);
        Vector3 rayEndPosition = m_Camera.transform.position;
        Vector3 rayDirection = rayEndPosition - rayStartPosition;

        float rayDistance = rayDirection.magnitude;
        
        if (rayDistance <= 0.0f)
            return;
        rayDirection /= rayDistance;

        //sphere cast in order to check if we need to move the camera forward
        RaycastHit[] hitInformation = Physics.SphereCastAll(
            rayStartPosition,
            0.5f,
            rayDirection,
            rayDistance,
            m_HitMask
            );

        //if the length of the array is equal to or less then 0 then we collided with nothing, return
        if (hitInformation.Length <= 0)
            return;

        //set the minimum move up distance to be the maximum value of a float
        float minimumMoveDistance = float.MaxValue;
        //loop through all raycast data so we can move the camera up appropriately
        foreach (RaycastHit hitInfo in hitInformation)
        {
            float moveUpDist = hitInfo.distance;
            if(moveUpDist > minimumMoveDistance)
            {
                continue;
            }
            minimumMoveDistance = moveUpDist;
        }
        //if the value we want to move up by is less then the value of a float that means that the value was overriden at some point and we need to move up the camera
        if(minimumMoveDistance < float.MaxValue)
        {            
            m_Camera.transform.position = rayStartPosition + rayDirection * minimumMoveDistance;
        }
    }
}
