using UnityEngine;
public class AIUtilits
{
    //pass in a gameobject and this method will check to see if that gameobject contains a crit point or not.
    public static int GetCritMultiplier(GameObject other)
    {
        CritPoint critPoint = other.GetComponent<CritPoint>();
        if (critPoint != null)
        {
            return critPoint.DamageMultiplier;
        }

        //if no crit point exists return 1 as to not mess up calculations
        return 1;
    }

    //This function will Calculate the closest position in a room from a search location. This function will look in the down direction.
    public static Vector3 CalcClosestPositionOnNavMeshBelowPos(Vector3 searchPosition)
    {
        RaycastHit hitLoc;
        Vector3 positionHit = searchPosition;

        if(Physics.Raycast(searchPosition, Vector3.down, out hitLoc, float.MaxValue, LayerMask.GetMask("MapGeometry")))
        {
            positionHit = hitLoc.point;
        }

        return positionHit;
    }

    //This function will Calculate the closest position in a room from a search location. This function will look from the X and Z positions passed in and the Y height
    //from which you wish to cast down from. The idleheight should generally be the height that the AI is currently at that you would like the AI to stay around. The margineofError
    //is used to determine how far off the the location can be from the idleHeight.
    public static Vector3 CalcClosestPositionOnNavMeshBelowPos(float searchX, float searchZ, float heightToSearchDownFrom, float idleHeight, float margineOfError)
    {
        Vector3 searchPosition = new Vector3(searchX, heightToSearchDownFrom, searchZ);        
        Vector3 idealPosition = new Vector3(searchX, idleHeight, searchZ);
        RaycastHit[] hitLocations = Physics.RaycastAll(searchPosition, Vector3.down, (searchPosition - idealPosition).magnitude, LayerMask.GetMask("MapGeometry"));

        float currentDistance = float.MaxValue;

        foreach(RaycastHit hit in hitLocations)
        {
            float distanceBetween = (hit.point - idealPosition).magnitude;

            //If the y hiehgt of the hit point minus the idle height is less the the margineOfError then this position can be remembered
            if((Mathf.Abs(hit.point.y - idleHeight) < margineOfError) &&  distanceBetween < currentDistance)
            {
                currentDistance = distanceBetween;
                idealPosition = hit.point;
            }
        }

        return idealPosition;
    }

    //This function will find the best location around a single point that the AI can move to. Uses OverlapSphere
    public static Vector3 CalcClosestPositionOnNavMeshAroundArea(Vector3 searchPosition, float searchRadius)
    {
        Collider[] hitData = null;
        Vector3 bestPositionInRadius = searchPosition;

        hitData = Physics.OverlapSphere(searchPosition, searchRadius, LayerMask.GetMask("MapGeometry"));

        float distanceFromSearchPos = float.MaxValue;

        foreach(Collider collider in hitData)
        {
            float distance = (collider.transform.position - searchPosition).magnitude;

            if (distance < distanceFromSearchPos)
            {
                bestPositionInRadius = collider.transform.position;
                distanceFromSearchPos = distance;
            }
        }

        return bestPositionInRadius;
    }
}

namespace TheNegative.AI
{
    public enum BehaviourState
    {
        Failed,
        Succeed,
        Running
    }

    public enum PaletteType
    {
        None,
        Fire,
        Ice,
        Poison
    }

}