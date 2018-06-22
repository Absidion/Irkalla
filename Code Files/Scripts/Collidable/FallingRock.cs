using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Rigidbody))]
public class FallingRock : LiamBehaviour
{
    public int Damage = 10;
    public float FallDistance = 30.0f;
    private Vector3 m_StartPosition = Vector3.zero;
    private Rigidbody m_Body;

    protected override void Awake()
    {
        base.Awake();
        m_Body = GetComponent<Rigidbody>();
        gameObject.SetActive(false);
    }

    protected override void Update()
    {
        base.Update();
        if (!PhotonNetwork.isMasterClient)
            return;

        if ((transform.position - m_StartPosition).magnitude > FallDistance)
        {
            photonView.RPC("DeactiveRock", PhotonTargets.All);
            m_Body.velocity = Vector3.zero;
        }
    }

    [PunRPC]
    public void DeactiveRock()
    {
        gameObject.SetActive(false);
        m_Body.velocity = Vector3.zero;
    }

    [PunRPC]
    public void ActivateFallingRock(Vector3 pos)
    {
        Debug.Log("Activating Rock", this);
        gameObject.SetActive(true);
        m_Body.velocity = Vector3.zero;

        //set the position
        transform.position = pos;

        //set the falling rocks start location
        StartPosition = pos;
    }

    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            if (player.photonView.isMine)
            {
                Status[] statuses = { Status.Stun };
                player.TakeDamage(Damage, transform.position ,statuses);
                photonView.RPC("DeactiveRock", PhotonTargets.All);
            }
        }
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }

    public Vector3 StartPosition { get { return m_StartPosition; } set { m_StartPosition = value; } }
}
