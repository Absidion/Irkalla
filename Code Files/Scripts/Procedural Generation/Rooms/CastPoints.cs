using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Writer: Liam
//Last Edited: Liam 12/30/2017

[RequireComponent(typeof(BoxCollider))]

public class CastPoints : MonoBehaviour
{
    public Vector3 center;
    private Vector3 m_HalfExtents;

    public Vector3 HalfExtents { get { return m_HalfExtents; } }


    void Awake()
    {
        BoxCollider collider = GetComponent<BoxCollider>();
        center = collider.center;
        m_HalfExtents = collider.size * 0.5f;
    }
}
