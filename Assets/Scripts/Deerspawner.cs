using UnityEngine;

public class Deerspawner : MonoBehaviour
{
    [SerializeField] private GameObject deer;
    [SerializeField] private GameObject[] spawnpoints;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Spawndeer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Spawndeer()
    {
        int randomnum = Random.Range(0, spawnpoints.Length);
        Instantiate(deer, spawnpoints[randomnum].transform);
    }
}
