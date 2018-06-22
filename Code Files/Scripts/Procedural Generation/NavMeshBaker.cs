using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

//Author: Josue
//Last edited: 10/17/2017

public class NavMeshBaker : MonoBehaviour
{
    private void Awake()
    {
        GameManager.LevelBaking += NavMeshGeneration;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        GameManager.PostPlayer -= NavMeshGeneration;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    void NavMeshGeneration(object sender, EventArgs args)
    {
        FindObjectOfType<NavMeshSurface>().BuildNavMesh();
    }
}
