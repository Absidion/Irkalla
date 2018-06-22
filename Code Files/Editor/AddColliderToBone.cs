using UnityEditor;
using UnityEngine;

public class AddColliderToBone : ScriptableWizard
{
    public enum ColliderType { Box, Sphere, Capsule, Mesh, Tag, PhysicsTag}
    public string Tag = string.Empty;
    public string PhysicsTag = string.Empty;
    public bool AddToAllObjects = false;
    public ColliderType ColliderToAdd = ColliderType.Box;
    public bool IsTrigger = false;
    public PhysicMaterial PhysicsMaterial = null;

    [Tooltip("Use: SphereCollider, CapsuleCollider")]
    public float Radius = 0.5f;
    [Tooltip("Use: CapsuleCollider")]
    public float Height = 1.0f;
    [Tooltip("Use: MeshCollider")]
    public Mesh Mesh = null;
    [Tooltip("Use: BoxCollider")]
    public Vector3 Size = Vector3.one;

    [MenuItem("Custom Tools/Add Colliders to Bones...")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<AddColliderToBone>("Add Colliders to Bones", "Add to Selected");
    }

    private void OnWizardCreate()
    {
        if (Selection.activeTransform != null)
        {
            //get the transform of the object that is currently selected
            Transform selectedObject = Selection.activeTransform;
            if (AddToAllObjects)
                AddColliders(selectedObject.root);
            else
                AddColliders(selectedObject);
        }
    }

    private void AddColliders(Transform currentIteration)
    {
        //iterate through all children and recursively call this function
        for (int i = 0; i < currentIteration.childCount; i++)
        {
            AddColliders(currentIteration.GetChild(i));
        }

        switch (ColliderToAdd)
        {
            case ColliderType.Box:
                BoxCollider colliderB = currentIteration.gameObject.AddComponent<BoxCollider>();
                colliderB.isTrigger = IsTrigger;
                colliderB.size = Size;
                break;

            case ColliderType.Sphere:
                SphereCollider colliderS = currentIteration.gameObject.AddComponent<SphereCollider>();
                colliderS.isTrigger = IsTrigger;
                colliderS.radius = Radius;
                break;

            case ColliderType.Capsule:
                CapsuleCollider colliderC = currentIteration.gameObject.AddComponent<CapsuleCollider>();
                colliderC.isTrigger = IsTrigger;
                colliderC.radius = Radius;
                colliderC.height = Height;
                break;

            case ColliderType.Mesh:
                MeshCollider colliderM = currentIteration.gameObject.AddComponent<MeshCollider>();
                colliderM.isTrigger = IsTrigger;
                colliderM.sharedMesh = Mesh;
                break;

            case ColliderType.Tag:
                currentIteration.tag = Tag;
                break;

            case ColliderType.PhysicsTag:
                currentIteration.gameObject.layer = LayerMask.GetMask(PhysicsTag);
                break;
        }
    }
}
