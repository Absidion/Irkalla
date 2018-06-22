using UnityEditor;
using UnityEngine;

public class RoomProcessor : AssetPostprocessor
{
    private int m_MeshColliderCount = 0;
    private int m_NumberOfSpawnPoints = 0;

    private void OnPostprocessModel(GameObject obj)
    {
        //if the obj name doesn't contain the word room, then it's not a room so we can ignore it
        string lowerCastPath = assetPath.ToLower();
        bool isInRoomDirectory = lowerCastPath.IndexOf("/rooms/") != -1;

        string lowerCaseName = obj.name.ToLower();

        if (!isInRoomDirectory)
            return;

        //add a room component onto the top gameobject that gets passed in        
        obj.tag = "Room";
        obj.layer = LayerMask.NameToLayer("Room");

        //set room values
        IslandRoom room = obj.AddComponent<IslandRoom>();
        room.EnemiesInRoomCount = 100;

        //next we need to apply the wall layer and tag to every gameobject in it's children that aren't stairs or doors
        ApplyLayersToSubObjects(obj, obj);

        if (m_NumberOfSpawnPoints < 1)
        {
            Debug.LogError("The number of spawn points in room " + obj.name + " is currently zero, please fix this and re-import");
            Editor.DestroyImmediate(obj);
            return;
        }

        if (m_MeshColliderCount > 100)
        {
            Debug.LogError("The number of mesh colliders in room " + obj.name + " is currently " + m_MeshColliderCount + " this is more then the allowed amount. Please change this and reimport");
            Editor.DestroyImmediate(obj);
            return;
        }

        Debug.Log("The number of mesh colliders in the object know as " + obj.name + " is " + m_MeshColliderCount);
        Debug.Log("The number of spawn points in the object know as " + obj.name + " is " + m_NumberOfSpawnPoints);

    }

    //passes in the main FBX's gameobject as well as the current gameobject that needs to be edited
    private void ApplyLayersToSubObjects(GameObject fbxGameObject, GameObject currentObject)
    {
        //save the imported FBX's obj transform into a var for use later
        Transform objTransform = currentObject.transform;

        //loop through all of the children in the transform we we're passed
        for (int i = 0; i < objTransform.childCount; i++)
        {
            //get the current child
            Transform child = objTransform.GetChild(i);
            if (child == null)
                continue;

            string lowerCaseName = child.name.ToLower();

            if (lowerCaseName.Contains("highpoly") || lowerCaseName.Contains("lod_mesh"))
                continue;

            //if this object has any children then we need to tag and layer those properly too
            if (child.childCount > 0)
            {
                ApplyLayersToSubObjects(fbxGameObject, child.gameObject);
            }

            //if the object contains the tag obstacle that means we need to tag and layer it oppropriatly
            if (lowerCaseName.Contains("obstacle"))
            {
                child.tag = "MapGeometry";
                child.gameObject.layer = LayerMask.NameToLayer("MapGeometry");
            }

            if (lowerCaseName.Contains("portal"))
            {
                Portal portal = child.gameObject.AddComponent<Portal>();
                GameObject obj = new GameObject();
                obj.transform.parent = portal.transform;
                obj.AddComponent<Camera>();
            }

            if (lowerCaseName.Contains("spawnpoint"))
            {
                //check if it's a player spawn point
                if (lowerCaseName.Contains("player"))
                {
                    child.gameObject.AddComponent<PlayerSpawnPoint>();
                }
                //check if it's an enemy spawn point
                else if (lowerCaseName.Contains("enemy"))
                {
                    m_NumberOfSpawnPoints++;
                    EnemySpawnPoint spawnPoint = child.gameObject.AddComponent<EnemySpawnPoint>();

                    IslandRoom r = fbxGameObject.GetComponent<IslandRoom>();
                    if (r.RoomSpawnPoints == null)
                        r.RoomSpawnPoints = new System.Collections.Generic.List<EnemySpawnPoint>();
                    r.RoomSpawnPoints.Add(spawnPoint);
                }
                //check if it's an item spawn point
                else if (lowerCaseName.Contains("item"))
                {
                    child.gameObject.AddComponent<ItemSpawnPoint>();
                }
            }

            //next we check for "dimentions" via the name. If it's a box throw on a box collider
            if (lowerCaseName.Contains("box"))
            {
                child.gameObject.AddComponent<BoxCollider>();
            }
            //if it's a circle or a sphere then we need to add a sphere collider to it
            else if (lowerCaseName.Contains("sphere") || lowerCaseName.Contains("circle"))
            {
                child.gameObject.AddComponent<SphereCollider>();
            }
            //if it's a capsule then add a capsule collider
            else if (lowerCaseName.Contains("capsule"))
            {
                child.gameObject.AddComponent<CapsuleCollider>();
            }
            //if it's a mesh collider then we begrudingly add a mesh collider to it
            else if (lowerCaseName.Contains("mesh") && !lowerCaseName.Contains("highpoly"))
            {
                m_MeshColliderCount++;
                child.gameObject.AddComponent<MeshCollider>();
                Transform findHighPoly = fbxGameObject.transform;

                for (int j = 0; j < findHighPoly.childCount; j++)
                {
                    if (findHighPoly.GetChild(j).name.Contains("highpoly") &&
                       findHighPoly.GetChild(j).name.Contains(lowerCaseName))
                    {
                        child.GetComponent<MeshFilter>().mesh = findHighPoly.GetComponent<MeshFilter>().mesh;
                        Editor.DestroyImmediate(findHighPoly.gameObject);
                        break;
                    }
                }
            }
            else if (lowerCaseName.Contains("castpoint"))
            {
                child.gameObject.AddComponent<CastPoints>();
            }

            if ((lowerCaseName.Contains("traverse") || lowerCaseName.Contains("traversable")) == false)
            {
                UnityEngine.AI.NavMeshObstacle obstacle = child.gameObject.AddComponent<UnityEngine.AI.NavMeshObstacle>();
                obstacle.carving = true;
                if (child.GetComponent<Collider>() != null)
                {
                    child.GetComponent<Collider>().material = Resources.Load<PhysicMaterial>("PhysicsMaterials/WallMaterial");
                }
            }

            if (lowerCaseName.Contains("invisablewall"))
            {
                child.gameObject.GetComponent<MeshFilter>().mesh = null;
            }
        }
    }
}