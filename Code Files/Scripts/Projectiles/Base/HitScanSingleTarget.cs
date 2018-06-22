using UnityEngine;
using TheNegative.AI;

//Author: Josue
//Last edited: Daniel 1/19/2018

public class HitScanSingleTarget : HitScanProjectile
{
    //Raycasts forward to check if any enemies are in range. If so, deal damage.
    [PunRPC]
    public void FireSingleTargetHitscan(Vector3 origin, Vector3 direction, int damage, int owner, float range, float hitscanRange, int ignoreLayer, int targetMask, Vector3 gunLocation)
    {
        RaycastHit hit; //local variable that will store info of any objects we hit

        if (Physics.SphereCast(origin, 0.1f, direction, out hit, range, ignoreLayer)) //if the raycast is successful
        {
            RenderProjectilePath(gunLocation, hit.point, 0, 1); //draw the projectile path

            HandleHitLogic(hit, targetMask, damage, owner, hitscanRange, gunLocation);
        }
    }

    [PunRPC]
    public void FireSingleTargetEnemyHitscan(Vector3 origin, Vector3 direction, int damage, int owner, float range, float hitscanRange, int ignoreLayer, int targetMask, Vector3 gunLocation)
    {
        RaycastHit hit; //local variable that will store info of any objects we hit

        if (Physics.Raycast(origin, direction, out hit, range, ignoreLayer)/* (origin, 1.5f, direction, out hit, range, ignoreLayer)*/) //if the raycast is successful
        {
            RenderProjectilePath(gunLocation, hit.point, 0, 1); //draw the projectile path

            HandleHitLogic(hit, targetMask, damage, owner, hitscanRange, gunLocation, true);
        }
    }

    [PunRPC]
    public void FireSingleTargetMultipleHitscan(Vector3 origin, Vector3 direction, int damage, int owner, float range, int ignoreLayer, int targetMask, Vector3 gunLocation, int drawIndexA, int drawIndexB)
    {
        RaycastHit hit; //local variable that will store info of any objects we hit

        RenderProjectilePath(gunLocation, gunLocation + (direction * range), drawIndexA, drawIndexB); //draw the projectile path

        if (Physics.Raycast(origin, direction, out hit, range, ignoreLayer)) //if the raycast is successful
        {
            HandleHitLogic(hit, targetMask, damage, owner, range, gunLocation);
        }
    }

    protected void HandleHitLogic(RaycastHit hit, int targetMask, int damage, int owner, float range, Vector3 gunLocation, bool enemyShot = false)
    {
        if (((1 << hit.collider.gameObject.layer) & targetMask) != 0) //if the raycast hit an object on the specific layer
        {
            AITakeDamageInterface enemy = null;
            Player p = null;

            if (enemyShot)
                p = hit.collider.transform.root.GetComponent<Player>();
            else
                enemy = hit.collider.transform.root.GetComponent<AITakeDamageInterface>();

            if (enemy != null) //if the raycasted object has a health component
            {
                if (PhotonNetwork.isMasterClient)
                {
                    //Get the distance from the hit and the gun
                    float distance = Vector3.Distance(hit.point, gunLocation);
                    int FinalDamage = MathFunc.CalculateDamageDropoff(damage, distance, range);

                    enemy.TakeDamage(owner, FinalDamage, null, AIUtilits.GetCritMultiplier(hit.collider.gameObject));
                }
            }
            else if (p != null)
            {
                if (p.photonView.isMine)
                    p.TakeDamage(damage, Vector3.zero, null);
            }
        }
    }
}
