using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheNegative.AI;

public class NetworkLineRenderer : Photon.PunBehaviour
{
    public LineRenderer m_LineRenderer;

    private Transform m_Target = null;
    private AI m_BaseAI = null;

    private void Awake()
    {
        AI enemy = GetComponent<AI>();

        if (enemy != null)
        {
            m_BaseAI = enemy;
        }
	}

    private void Update()
    {
        if (m_Target != null)
        {
            m_LineRenderer.SetPosition(1, m_Target.position);
        }
    }

    [PunRPC]
    public void RPCSetLineRendererEnabled(bool flag)
    {
        m_LineRenderer.enabled = flag;
    }

    [PunRPC]
    public void RPCSetTarget(int playerNumber)
    {
        m_LineRenderer.SetPosition(0, m_BaseAI.transform.position);

        foreach (Player p in m_BaseAI.PlayerList)
        {
            if (p.PlayerNumber == playerNumber)
                m_Target = p.transform;
        }
    }

    [PunRPC]
    public void RPCRemoveTarget()
    {
        m_Target = null;
    }

    public void SetTarget(int playerNumber)
    {
        photonView.RPC("RPCSetTarget", PhotonTargets.All, playerNumber);
    }

    public void RemoveTarget()
    {
        photonView.RPC("RPCRemoveTarget", PhotonTargets.All);
    }

    public void SetLineRendererEnabled(bool flag)
    {
        photonView.RPC("RPCSetLineRendererEnabled", PhotonTargets.All, flag);
    }

    public LineRenderer LineRenderer { get { return m_LineRenderer; } set { m_LineRenderer = value; } }
}
