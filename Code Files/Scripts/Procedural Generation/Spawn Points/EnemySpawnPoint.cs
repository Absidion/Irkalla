using UnityEngine;

public class EnemySpawnPoint : SpawnPoint
{
    public int MinimumWeight = 1;                   //The minimum weight of this spawn point
    public int MaximumWeight = 6;                   //The maximum weight of this spawn point
    [Tooltip("Only set a Gameobject into this location if this spawn point requires a custom enemy to spawn, or if this spawn point can only spawn 1 thing.")]
    public GameObject CustomEnemySpawn;             //This should only be used if the spawn point is guarentied to only spawn one thing

    //if the value of the maximum or minimum weight ever changes you will need to change these values in order for that logic to be reflected properly

    private const int c_MinimumEnemySpawnPointWeight = 1;
    private const int c_MaximumEnemySpawnPointWeight = 6;

    private void OnValidate()
    {
        if (MinimumWeight < c_MinimumEnemySpawnPointWeight)
            MinimumWeight = c_MinimumEnemySpawnPointWeight;

        if (MaximumWeight > c_MaximumEnemySpawnPointWeight)
            MaximumWeight = c_MaximumEnemySpawnPointWeight;
    }
}
