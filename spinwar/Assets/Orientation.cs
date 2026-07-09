using UnityEngine;

public class Orientation : MonoBehaviour
{
    public Transform cameraTransform; // Drag 'Main Camera' here
    public Transform orientation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 viewDir2 = transform.position - new Vector3(cameraTransform.position.x, transform.position.y, cameraTransform.position.z);

        // 2. Make the orientation object look in that direction
        if (viewDir2 != Vector3.zero)
        {
            orientation.forward = viewDir2.normalized;
        }
    }
}
