using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TheNegative.AI;

//Writer: Liam
//Last Updated: Josue 11/30/2017

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance;                  //Instance of the mosnter manager    

    void Awake()
    {
        Instance = this;
        GameManager.LevelPopulation += SpawnMonsters;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        GameManager.LevelPopulation -= SpawnMonsters;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    void SpawnMonsters(object sender, EventArgs e)
    {
        if (!PhotonNetwork.isMasterClient)
            return;

        //get all of spawn points in the scene
        EnemySpawnPoint[] sceneSpawnPoints = FindObjectsOfType<EnemySpawnPoint>();

        foreach (EnemySpawnPoint spawnPoint in sceneSpawnPoints)
        {
            string objectToSpawn = string.Empty;

            if (spawnPoint.CustomEnemySpawn != null)
                objectToSpawn = spawnPoint.CustomEnemySpawn.name;

            if (objectToSpawn == string.Empty)
            {
                objectToSpawn = GetMonsterBasedOnWeights(spawnPoint.MinimumWeight, spawnPoint.MaximumWeight);
            }

            //use photon to instantiate the object over the network
            GameObject obj = PhotonNetwork.InstantiateSceneObject("AI/" + objectToSpawn, spawnPoint.transform.position, spawnPoint.transform.rotation, 0, null);

            RoomManager.Instance.AddEnemyToRoom(obj.GetComponent<AI>(), spawnPoint.SpawnPointRoom);
        }
    }

    public string GetMonsterBasedOnWeights(int minWeight, int maxWeight)
    {
        string name = string.Empty;

        while (name == string.Empty)
            name = XmlUtilities.GetNameFromMonsterXML(minWeight, maxWeight);

        return name;
    }
}

//enum of enemy types
public enum MonsterType
{
    Humanoid,
    Flying,
    Mutation,
    Enviromental,
    Boss
}