using UnityEngine;
using TMPro; // Standard Unity UI namespace

public class PlayerSafetySensor : MonoBehaviour
{
    [Header("UI Setup")]
    public TextMeshProUGUI warningLabel; // Drag your Text object here

    [Header("Settings")]
    public float detectionRange = 10f;
    public Vector3 sensorOffset = new Vector3(0, 1f, 0); 
    public Vector3 sensorSize = new Vector3(2.5f, 1f, 0.1f); 
    public LayerMask obstacleLayers;     

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if(warningLabel != null) warningLabel.text = ""; // Clear text at start
    }

    void FixedUpdate()
    {
        Vector3 center = transform.position + sensorOffset;
        Vector3 direction = transform.forward;

        RaycastHit hit;
        bool isBlocked = Physics.BoxCast(center, sensorSize / 2, direction, out hit, transform.rotation, detectionRange, obstacleLayers);

        Color debugColor = isBlocked ? Color.red : Color.green;
        ExtDebug.DrawBoxCastBox(center, sensorSize / 2, transform.rotation, direction, detectionRange, debugColor);

        // --- UI LOGIC ---
        if (isBlocked)
        {
            // Case 1: Speed Breaker (Show Text, Don't Brake)
            if (hit.collider.CompareTag("SpeedBreaker"))
            {
                UpdateUI("Speed Breaker Ahead", Color.yellow);
                return; // Exit here (No Braking)
            }
            
            // Case 2: Crossing (Show Text, Don't Brake)
            if (hit.collider.CompareTag("Crossing"))
            {
                UpdateUI("Zebra Crossing", Color.blue);
                return; // Exit here (No Braking)
            }

            // Case 3: Danger (Show Text AND Brake)
            UpdateUI("STOP! Obstacle Detected", Color.red);
            Debug.LogWarning("AUTO-BRAKE! Detected: " + hit.collider.name);
            
            if (rb != null)
            {
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, Time.fixedDeltaTime * 5f);
            }
        }
        else
        {
            // Nothing detected -> Clear the text
            if(warningLabel != null) warningLabel.text = "";
        }
    }

    // Helper to change text color and content
    void UpdateUI(string message, Color color)
    {
        if (warningLabel != null)
        {
            warningLabel.text = message;
            warningLabel.color = color;
        }
    }
}

// (Keep ExtDebug class at bottom)
public static class ExtDebug
{
    public static void DrawBoxCastBox(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Vector3 direction, float distance, Color color)
    {
        direction.Normalize();
        Box bottom = new Box(origin, halfExtents, orientation);
        Box top = new Box(origin + (direction * distance), halfExtents, orientation);
        Debug.DrawLine(bottom.frontTopLeft, top.frontTopLeft, color);
        Debug.DrawLine(bottom.frontTopRight, top.frontTopRight, color);
        Debug.DrawLine(bottom.frontBottomLeft, top.frontBottomLeft, color);
        Debug.DrawLine(bottom.frontBottomRight, top.frontBottomRight, color);
    }
    private struct Box
    {
        public Vector3 frontTopLeft, frontTopRight, frontBottomLeft, frontBottomRight;
        public Box(Vector3 origin, Vector3 halfExtents, Quaternion orientation)
        {
            frontTopLeft = origin + orientation * (new Vector3(-halfExtents.x, halfExtents.y, halfExtents.z));
            frontTopRight = origin + orientation * (new Vector3(halfExtents.x, halfExtents.y, halfExtents.z));
            frontBottomLeft = origin + orientation * (new Vector3(-halfExtents.x, -halfExtents.y, halfExtents.z));
            frontBottomRight = origin + orientation * (new Vector3(halfExtents.x, -halfExtents.y, halfExtents.z));
        }
    }
}