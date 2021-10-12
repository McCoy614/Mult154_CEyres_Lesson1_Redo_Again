using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Patrolling : MonoBehaviour
{

    public List<GameObject> waypoints;
    private NavMeshAgent agent;
    private const float WP_THRESHOLD = 5.0f;
    private GameObject currentWP;
    private int currentWPIndex = -1;
   

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentWP = GetNextWaypoint();
    }

    GameObject GetNextWaypoint()
    {
        currentWPIndex++;
        if (currentWPIndex == waypoints.Count)
        {
            currentWPIndex = 0;
        }
        Debug.Log("Getting Next Waypoint " + currentWPIndex);
        return waypoints[currentWPIndex];
    }

    // Update is called once per frame
    public void PatrolWaypoints()
    {
        Debug.Log("PatrolWaypoints");
        if(Vector3.Distance(transform.position, currentWP.transform.position) < WP_THRESHOLD)
        {

            currentWP = GetNextWaypoint();
            agent.SetDestination(currentWP.transform.position);
        }
    }
}
