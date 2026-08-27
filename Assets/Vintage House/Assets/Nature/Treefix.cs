using UnityEngine;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class Treefix : MonoBehaviour
{
#if UNITY_EDITOR
    [ContextMenu("Fix Bounds For All Trees")]
    void FixAllBounds()
    {
        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>(true);

        int fixedCount = 0;

        foreach (MeshFilter filter in filters)
        {
            Mesh mesh = filter.sharedMesh;

            if (mesh == null)
                continue;

            mesh.RecalculateBounds();

            EditorUtility.SetDirty(mesh);

            fixedCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Finished! Recalculated bounds for {fixedCount} meshes.");
    }
#endif
}
