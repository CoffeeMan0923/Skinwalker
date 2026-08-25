using UnityEngine;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class Treefix : MonoBehaviour
{
#if UNITY_EDITOR
    [ContextMenu("Recalculate Tree Bounds")]
    void Recalculate()
    {
        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter filter in filters)
        {
            if (filter.sharedMesh == null)
                continue;

            // Make a unique mesh copy so the imported asset isn't modified.
            Mesh mesh = Instantiate(filter.sharedMesh);
            mesh.name = filter.sharedMesh.name + "_FixedBounds";
            mesh.RecalculateBounds();

            filter.sharedMesh = mesh;
        }

        Debug.Log("Tree mesh bounds recalculated.");
    }
#endif
}
