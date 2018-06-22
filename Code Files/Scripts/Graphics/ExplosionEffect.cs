using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Josue
//Last edited: Josue 10/31/2017

public class ExplosionEffect : LiamBehaviour
{
    public float m_Speed = 10.0f;                //how fast the scale incremements over time

    private Mesh m_Mesh;                        //sphere mesh
    private MeshRenderer m_Renderer;            //sphere renderer
    private float m_MaxRadius = 0.0f;           //max radius the sphere must scale to
    private bool m_ExplosionActivated = false;  //effect only updates when this is true

    protected override void Awake()
    {
        base.Awake();
        m_Mesh = gameObject.GetComponent<MeshFilter>().mesh;
        m_Renderer = gameObject.GetComponent<MeshRenderer>();
	}
	
	protected override void Update()
    {
        base.Update();

        if (m_ExplosionActivated)
        {
            UpdateExplosion();
        }
	}

    //Script that instantiates this object activates it and gives it the max radius
    [PunRPC]
    public void ActivateExplosion(float maxRadius)
    {
        m_MaxRadius = maxRadius;
        m_ExplosionActivated = true;
    }

    //Increases the size of the explosion over time. Once the radius has past the max radius, it destroys itself
    private void UpdateExplosion()
    {
        //scale the explosion over time
        transform.localScale += new Vector3(Time.deltaTime * m_Speed, Time.deltaTime * m_Speed, Time.deltaTime * m_Speed);

        if (GetRadius() > (0.5f * m_MaxRadius))
        {
            if (GetRadius() >= m_MaxRadius)
            {
                Destroy(gameObject);
            }

            float oldValue = m_Renderer.material.GetFloat("_ClipRange");
            float newValue = Mathf.Clamp01(oldValue - Time.deltaTime * 2.0f);
            m_Renderer.material.SetFloat("_ClipRange", newValue);
        }
    }

    //Returns distance from origin to any of the vertices. This is our radius.
    private float GetRadius()
    {
        Vector3[] vertices = m_Mesh.vertices;
        float radius = (transform.TransformPoint(vertices[0]) - gameObject.transform.position).magnitude;
        return radius;
    }
}
