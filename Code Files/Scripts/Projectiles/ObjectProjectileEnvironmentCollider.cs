using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Josue
//Last edited: 11/14/2017

public class ObjectProjectileEnvironmentCollider : MonoBehaviour
{
    private int m_EnvironmentLayerMask = 0; //when the object collides with any of these layers, it will dissapear
    private void OnTriggerEnter(Collider other)
    {
        //if the layer mask has not been set yet
        if (m_EnvironmentLayerMask == 0)
        {
            m_EnvironmentLayerMask = LayerMask.GetMask("Wall", "Door", "Stairs");
        }

        if (((1 << other.gameObject.layer) & m_EnvironmentLayerMask) != 0)
        {
            transform.parent.gameObject.SetActive(false);
        }
    }
}
