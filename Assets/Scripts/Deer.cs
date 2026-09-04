using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class Deer : MonoBehaviour
{
    private GameObject[] waypoints;
    private GameObject targetWaypoint;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float distance;
    private bool switchingWaypoint = false;

    private int randomNum;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        waypoints = GameObject.FindGameObjectsWithTag("Waypoint");
        randomNum = Random.Range(0, waypoints.Length);
        targetWaypoint = waypoints[randomNum];
        distance = Vector3.Distance(this.gameObject.transform.position, targetWaypoint.transform.position);
        MoveToWaypoint();
    }

    void Update()
    {
        distance = Vector3.Distance(this.gameObject.transform.position, targetWaypoint.transform.position);
        if(distance <= 2 && switchingWaypoint == false)
        {
            switchingWaypoint = true;
            SwitchWaypoint();
        }
        
    }
    void SwitchWaypoint()
    {
        int newRanNum = Random.Range(0, waypoints.Length);
        while(newRanNum == randomNum)
        {
            newRanNum = Random.Range(0, waypoints.Length);
        }
        randomNum = newRanNum;
        targetWaypoint = waypoints[randomNum];
        switchingWaypoint = false;
        MoveToWaypoint();
    }
    void MoveToWaypoint()
    {
        agent.destination = targetWaypoint.transform.position;
    }
    public void Damage()
    {
        Destroy(gameObject);
    }
}
