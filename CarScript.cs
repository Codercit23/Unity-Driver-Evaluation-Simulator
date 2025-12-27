using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class CarWaypointNavigator : MonoBehaviour
{
    public NavMeshAgent agent;
    
    [Header("Route Settings")]
    public List<Transform> waypoints; // Drag your lane points here
    public bool loop = true;          // Should the car restart the path when finished?
    
    private int currentWaypointIndex = 0;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // CRITICAL: Uncheck this so the car doesn't slow down at every single point
        agent.autoBraking = false; 

        MoveToNextWaypoint();
    }

    void Update()
    {
        // 1. If the agent is currently calculating a path, do nothing
        if (agent.pathPending) return;

        // 2. If the agent has NO path (failed to find one), do not skip!
        if (agent.pathStatus == NavMeshPathStatus.PathInvalid || agent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            Debug.LogWarning("Car cannot reach Waypoint: " + currentWaypointIndex);
            return; 
        }

        // 3. Only switch waypoints if we have a path AND we are close
        if (agent.remainingDistance < 2.0f && agent.hasPath)
        {
            MoveToNextWaypoint();
        }
        
        // Debug Line remains helpful
        if(agent.hasPath) Debug.DrawLine(transform.position, agent.destination, Color.red);
    }

    void MoveToNextWaypoint()
    {
        if (waypoints.Count == 0) return;

        // Ensure the index stays within the list range
        if (currentWaypointIndex >= waypoints.Count)
        {
            if (loop)
            {
                currentWaypointIndex = 0; // Go back to start
            }
            else
            {
                return; // Stop at the end
            }
        }

        // Tell the agent to go to the current waypoint
        agent.SetDestination(waypoints[currentWaypointIndex].position);
        
        // Prepare for the next point
        currentWaypointIndex++;
    }
}