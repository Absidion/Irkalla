using UnityEngine;

//Writer: Liam
//Laste Updated: 1/15/2017

public abstract class SpawnPoint : MonoBehaviour
{
    [HideInInspector]
    public IslandRoom SpawnPointRoom;               //The room that this spawn point is located in
}