using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ObjectRecognizer : MonoBehaviour
{
    public static bool isEmergencyBraking = false; 

    [Header("Settings")]
    public float scanDistance = 50f;
    
    // --- CHANGED FROM 5f TO 10f ---
    public float brakeDistance = 10f; 
    // ------------------------------
    
    public float activationDelay = 5.0f; 
    public LayerMask scannableLayers; 
    
    [Header("UI Assignments")]
    public RectTransform selectionBox; 
    public TextMeshProUGUI readoutText; 
    public Camera mainCam; 

    private float startTime;
    private Image boxImage;

    void Start()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (selectionBox != null) boxImage = selectionBox.GetComponent<Image>();
        if (selectionBox != null) selectionBox.gameObject.SetActive(false);
        if (readoutText != null) readoutText.text = "";

        startTime = Time.time;
        if(readoutText) readoutText.fontStyle = FontStyles.Bold;
    }

    void Update()
    {
        Vector3 sensorStartPos = transform.position + Vector3.up + (transform.forward * 2.0f);
        Ray ray = new Ray(sensorStartPos, transform.forward); RaycastHit hit;

        isEmergencyBraking = false;

        if (Physics.Raycast(ray, out hit, scanDistance, scannableLayers))
        {
            bool isSystemReady = (Time.time - startTime) > activationDelay;

            // Check if distance is less than the NEW 10m limit
            if (isSystemReady && hit.distance <= brakeDistance)
            {
                isEmergencyBraking = true;

                if (readoutText)
                {
                    readoutText.text = "BRAKE!";
                    readoutText.color = Color.red;
                    readoutText.fontSize = 25;
                }
                
                if (boxImage)
                {
                    Color transparentRed = Color.red;
                    transparentRed.a = 0.3f; 
                    boxImage.color = transparentRed;
                }
            }
            else
            {
                IdentifyObject(hit.collider.gameObject, hit.distance);
            }
            
            if (selectionBox != null)
            {
                selectionBox.gameObject.SetActive(true);
                selectionBox.position = mainCam.WorldToScreenPoint(hit.collider.bounds.center);
            }
            Debug.DrawLine(ray.origin, hit.point, Color.red);
        }
        else
        {
            if(readoutText) readoutText.text = ""; 
            if (selectionBox != null) selectionBox.gameObject.SetActive(false);
            Debug.DrawRay(ray.origin, transform.forward * scanDistance, Color.green);
        }
    }

    void IdentifyObject(GameObject obj, float dist)
    {
        if (readoutText == null) return;

        string tag = "Untagged";
        if (!obj.CompareTag("Untagged")) tag = obj.tag;
        else if (obj.transform.root != null) tag = obj.transform.root.tag;
        
        readoutText.text = $"[{tag}] {dist:F1}m";
        readoutText.fontSize = 30;

        Color targetColor = Color.white;

        switch (tag)
        {
            case "Pedestrian":
                targetColor = Color.Lerp(Color.red, Color.yellow, Mathf.PingPong(Time.time * 10, 1f));
                break;
            case "Car":
                targetColor = Color.cyan;
                break;
            case "Obstacle":
                 targetColor = new Color(1f, 0.5f, 0f); 
                 break;
            default:
                targetColor = Color.white;
                break;
        }

        readoutText.color = targetColor;

        if (boxImage)
        {
            targetColor.a = 0.3f; 
            boxImage.color = targetColor;
        }
    }
}