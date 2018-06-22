using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Josue
//Last edited: Josue 10/12/2017

[RequireComponent(typeof(LineRenderer))]

public class HitScanProjectile: MonoBehaviour
{
    [SerializeField]
    private LineRenderer m_LinePath;   //rendered line that shoots out from the weapon towards location

    //Renders the projectile path from start of weapon to hit destination
    protected void RenderProjectilePath(Vector3 gunLocation, Vector3 destination, int drawIndexA, int drawIndexB)
    {
        if (m_LinePath.enabled) //if the line path is already enabled, just reset the alpha
        {
            m_LinePath.material.SetFloat("_Alpha", 0.6f);
        }
        else
        {
            m_LinePath.enabled = true;
        }

        m_LinePath.SetPosition(drawIndexA, gunLocation); //set start location
        m_LinePath.SetPosition(drawIndexB, destination); //set destination location
        if(m_LinePath.positionCount > drawIndexB + 1)
            m_LinePath.SetPosition(drawIndexB + 1, gunLocation);
    }

    //Fades out the projectile path over time
    protected void FadeOutProjectilePath()
    {
        if (m_LinePath.enabled)
        {
            float alpha = m_LinePath.material.GetFloat("_Alpha"); //get reference to alpha in shader
            alpha -= Time.deltaTime; 

            if (alpha <= 0) //if the path is no longer visible, disable the path and reset the alpha to full.
            {
                alpha = 0;
                m_LinePath.enabled = false;
                m_LinePath.material.SetFloat("_Alpha", 0.5f);
                return;
            }

            m_LinePath.material.SetFloat("_Alpha", alpha); //set alpha value in shader to modified alpha
        }
    }

    protected void Update()
    {
        FadeOutProjectilePath();
    }
}