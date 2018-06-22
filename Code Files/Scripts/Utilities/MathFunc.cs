using System.Collections.Generic;
using UnityEngine;

public class MathFunc
{
    public static float Epsilon = 0.00001f;
    public static float LargeEpsilon = 0.1f;

    public static Vector3 ProjectToBottomOfCapsule(Vector3 ptToProject, Vector3 capsuleCenter, float capsuleHeight, float capsuleRadius)
    {
        //Calculating the length of the line segment part of the capsule
        float lineSegmentLength = capsuleHeight - 2.0f * capsuleRadius;
        //Clamp line segment length
        lineSegmentLength = Mathf.Max(lineSegmentLength, 0.0f);
        //Calculate the line segment that goes along the capsules "Height"
        Vector3 bottomLineSegPt = capsuleCenter;
        bottomLineSegPt.y -= lineSegmentLength * 0.5f;
        //Get displacement from bottom of line segment
        Vector3 ptDisplacement = ptToProject - bottomLineSegPt;
        //Calculate needed distances
        float horizDistSqrd = ptDisplacement.x * ptDisplacement.x + ptDisplacement.z * ptDisplacement.z;
        float radiusSqrd = capsuleRadius * capsuleRadius;
        //The answer will be undefined if the pt is horizontally outside of the capsule
        if (horizDistSqrd > radiusSqrd)
        {
            return ptToProject;
        }
        //Calc projected pt
        float heightFromSegPt = -Mathf.Sqrt(radiusSqrd - horizDistSqrd);
        Vector3 projectedPt = ptToProject;
        projectedPt.y = bottomLineSegPt.y + heightFromSegPt;
        return projectedPt;
    }

    public static float Clamp(float valueToClamp, float minValue, float maxValue)
    {
        if (valueToClamp < minValue)
            return minValue;

        else if (valueToClamp > maxValue)
            return maxValue;

        return valueToClamp;
    }

    public static Vector3 CalculateClosestGroundPosition(Vector3 curPos)
    {
        int layerMask = ~(LayerMask.GetMask("Enemy") | LayerMask.GetMask("Player") | LayerMask.GetMask("Room"));

        RaycastHit hit;       

        if(Physics.Raycast(curPos, Vector3.down,  out hit, float.MaxValue, layerMask))
        {
            Debug.DrawLine(curPos, hit.point);
            return hit.point;
        }

        return curPos;
    }

    //returns how a value which represents how far something has travelled in this frame reletive to the speed passed in and the direction calculated from beginning and end position    
    public static Vector3 MoveToward(Vector3 startLocation, Vector3 endLocation, float speed)
    {
        Vector3 moveDir = (endLocation - startLocation).normalized;

        return moveDir * speed * Time.deltaTime;
    }

    public static float GetRollDegFromQuaternion(Quaternion q)
    {
        float SinR = 2 * (q.w * q.x - q.y * q.z);
        float CosR = 1 - 2 * (q.x * q.x + q.y * q.y);
        float rollEuler = Mathf.Atan2(SinR, CosR);
        rollEuler *= Mathf.Rad2Deg;
        return rollEuler;
    }

    public static float Get01ExponentialValue(float distance, float MaxDistance)
    {
        //Get the value on a exponential curve
        float normalizedDist = MaxDistance - distance;

        //Hard offset the balue by -0.2f
        float ExpValue = 2.0f * (Mathf.Pow(0.15f, normalizedDist / 2.5f));

        //Clamp the value from 0-1
        return Mathf.Clamp01(ExpValue);
    }

    public static int CalculateExponentialDamageDropoff(int damage, float distance, float range)
    {
        //Get the factor to multiply the damage by
        float damageDropFactor = Get01ExponentialValue(distance, range);
        //Reverse thedamage drop factor
        damageDropFactor = 1 - damageDropFactor;
        // Multiply the Damage by the Damage factor (which is a number between 0  and 1)
        // Floor the Damage to force the damage to be always rounded down
        int FinalDamage = Mathf.FloorToInt(damageDropFactor * damage);

        if (FinalDamage == 0)
            FinalDamage = 1;

        return FinalDamage;
    }

    public static int CalculateDamageDropoff(int damage, float distance, float range)
    {
        float damageDropFactor = distance / range;
        damageDropFactor = 1 - damageDropFactor;

        int FinalDamage = Mathf.FloorToInt(damageDropFactor * damage);

        if (FinalDamage <= 0)
            FinalDamage = 1;

        return FinalDamage;
    }

    public static float ApproximateNormalDistribution()
    {
        float randomValue = Random.Range(-1.0f, 1.0f);
        randomValue += Random.Range(-1.0f, 1.0f);
        randomValue += Random.Range(-1.0f, 1.0f);
        randomValue += Random.Range(-1.0f, 1.0f);

        return randomValue;
    }

    public static bool AlmostEquals(float value1, float value2)
    {
        return ((value1 - value2) <= LargeEpsilon) ? true : false;
    }

    public static bool AlmostEquals(Vector3 vector1, Vector3 vector2)
    {
        float magnitude = (vector1 - vector2).magnitude;
        return (magnitude <= LargeEpsilon) ? true : false;
    }

    public static bool AlmostEquals(Vector3 vector1, Vector3 vector2, float epsilonMultiplier)
    {
        float magnitude = (vector1 - vector2).magnitude;
        return (magnitude <= LargeEpsilon * epsilonMultiplier) ? true : false;
    }

    public static Quaternion ClampQuaternionXRotation(Quaternion quat, float maximumX, float minimumX)
    {
        //clamp the quaternion that will be our camera's angle
        quat.x /= quat.w;
        quat.y /= quat.w;
        quat.z /= quat.w;
        quat.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(quat.x);
        angleX = Mathf.Clamp(angleX, minimumX, maximumX);
        quat.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return quat;
    }

    public static Quaternion ClampQuaternionZRotation(Quaternion quat, float minZ, float maxZ)
    {
        Vector3 euler = quat.eulerAngles;

        if (euler.z < minZ)
            euler.z = minZ;
        else if (euler.z > maxZ)
            euler.z = maxZ;

        quat = Quaternion.Euler(euler);

        return quat;
    }
}
