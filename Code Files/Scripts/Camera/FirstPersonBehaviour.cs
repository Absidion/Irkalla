using System;
using System.Collections.Generic;
using UnityEngine;

//Author: Liam
//Last Edited : Liam 10/2/2017
//Known Issues:
//1. Camera rotation is off

[Serializable]
public class FirstPersonBehaviour : CameraBehaviour
{
    public float MaximumX = 85.0f;
    public float MinimumX = -85.0f;
        
    private GameObject m_EyeLocation;                   //The location of the player's eye
    private Vector3 m_EyeOffset;                        //The offset of the eye position from the center of the player prefab

    public FirstPersonBehaviour()
    {

    }

    public override void Init(Player player, FirstPersonCamera cam)
    {
        base.Init(player, cam);
        m_EyeLocation = player.EyeLocation;
        m_EyeOffset = Vector3.zero;
        m_Camera.transform.SetParent(m_EyeLocation.transform);
    }

    public override void Activate()
    {
        base.Activate();
        m_Camera.transform.localPosition = Vector3.zero;
        m_Camera.transform.localRotation = Quaternion.identity;
    }

    public override void Deactivate()
    {
        base.Deactivate();
        m_Camera.transform.SetParent(null);
    }

    public override void UpdateCamera()
    {

        return;
        //calculate eye offset
        m_EyeOffset = m_EyeLocation.transform.position;

        //set the camera's position to be equal to the player's position plus the offset for the eye
        m_Camera.transform.position = m_EyeLocation.transform.position;
        m_Camera.transform.rotation = m_EyeLocation.transform.rotation;        

        //get the change in eulerAngles this frame between the player and the camera
        Vector3 eulerAngleDiff = m_Player.transform.rotation.eulerAngles - m_Camera.transform.rotation.eulerAngles;
        eulerAngleDiff.x = m_Player.PitchChange;
        
        Quaternion quat = Quaternion.Euler(m_Camera.transform.rotation.eulerAngles + eulerAngleDiff);

        //rotate the camera based on the difference in eulerAngles
        m_Camera.transform.rotation = MathFunc.ClampQuaternionXRotation(quat, MaximumX, MinimumX);
    }
}
