using System;
using System.Collections.Generic;
using UnityEngine;

//Author: Liam
//Last Edited : Liam 10/2/2017

[Serializable]
public abstract class CameraBehaviour
{
    protected Player m_Player;
    protected FirstPersonCamera m_Camera;

    public CameraBehaviour()
    {

    }

    public virtual void Init(Player player, FirstPersonCamera cam)
    {
        m_Player = player;
        m_Camera = cam;
    }

    public virtual void Activate()
    {

    }

    public virtual void Deactivate()
    {

    }

    public virtual Vector3  GetControlRotation()
    {
        return m_Camera.transform.rotation.eulerAngles;
    }

    public abstract void UpdateCamera();
}
