using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectLookAtCamera : LiamBehaviour {
	
	protected override void Update ()
    {
        base.Update();

        if (!photonView.isMine)
        {
            transform.LookAt(new Vector3(transform.position.x, Camera.main.transform.position.y, transform.position.z));
        }
	}
}
