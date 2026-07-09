using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public float rotationSpeed = 1000f; // Degrees per second
    private float _currentSpin = 0f;
    /// Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //transform.Rotate(new Vector3(0f, 1000f, 0f));
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0f;

        if (camForward.sqrMagnitude > 0.01f)
        {
            // 2. Increment the spin independent of movement
            _currentSpin += rotationSpeed * Time.deltaTime;
            _currentSpin %= 1000f; // Keep it within 0-360

            // 3. Create the Base Rotation (facing with camera)
            Quaternion baseRotation = Quaternion.LookRotation(camForward);

            // 4. Add the spinning offset on the Y axis
            Quaternion spinOffset = Quaternion.Euler(0, _currentSpin, 0);

            // 5. Final Result: Base * Offset
            transform.rotation = baseRotation * spinOffset;
        }

    }
}
