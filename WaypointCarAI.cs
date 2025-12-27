using UnityEngine;

public class WaypointCarAI : MonoBehaviour
{
    [Header("Setup")]
    public WaypointPath path;
    public float speed = 15f;
    public float turnSpeed = 5f;
    public float reachDistance = 5.0f;
    
    // Internal variables
    private int currentNode = 0;
    private bool isFinished = false; // Flag to check if we are done
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb) 
        {
            rb.isKinematic = false; 
            rb.useGravity = true;
        }
    }

    void FixedUpdate()
    {
        // Safety Checks
        if (path == null || path.nodes.Count == 0) return;
        if (isFinished) return; // If we finished the path, do nothing (Stay Stopped)

        Vector3 targetPos = path.nodes[currentNode].position;

        // --- 1. Move Car ---
        Vector3 direction = targetPos - transform.position;
        direction.y = 0; 

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
        }

        if(rb)
        {
            Vector3 velocity = transform.forward * speed;
            rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
        }
        else
        {
            transform.position += transform.forward * speed * Time.fixedDeltaTime;
        }

        // --- 2. Check Waypoint "Vanishing" Logic ---
        float distance = Vector3.Distance(transform.position, targetPos);
        
        if (distance < reachDistance)
        {
            // We hit the node! Move to the next one.
            currentNode++;

            // CHECK: Did we run out of nodes?
            if (currentNode >= path.nodes.Count)
            {
                isFinished = true; // STOP THE CAR
                Debug.Log("Path Completed. Stopping car.");
                
                // Optional: Slam the brakes physically
                if(rb) rb.linearVelocity = Vector3.zero;
            }
        }
    }
}