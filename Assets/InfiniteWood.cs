using UnityEngine;

public class InfiniteWood : MonoBehaviour
{
    [SerializeField] private GameObject woodPrefab;

    public GameObject GetWood()
    {
        if (woodPrefab == null)
            return null;

        return Instantiate(woodPrefab);
    }
}