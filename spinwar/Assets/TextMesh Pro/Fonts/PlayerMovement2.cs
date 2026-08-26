using System;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(NetworkObject))]
public class PlayerMovement2 : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] public float moveSpeed;
    [SerializeField] public float slideSpeed;
    [SerializeField] public float inputSmoothSpeed;
    [SerializeField] public float dashSpeed;
    [SerializeField] public float dashSpeedChangeFactor;
    [SerializeField] public float groundDrag;

    [Header("Ground Check")]
    [SerializeField] public float playerHeight;
    [SerializeField] public LayerMask whatIsGround;
    private bool grounded;

    [SerializeField] public Transform orientation;

    private float horizontalInput;
    private float verticalInput;

    private Vector3 moveDirection;
    private Vector3 smoothedMoveDirection;

    private Rigidbody rb;

    [SerializeField] public MovementState currentstate;
    public enum MovementState
    {
        sliding,
        dashing
    }
    [SerializeField] public bool dashing;

    [SerializeField] private float desiredMoveSpeed;
    [SerializeField] private float lastDesiredMoveSpeed;
    [SerializeField] private float speedChangeFactor;
    [SerializeField] private MovementState lastState;

    [Header("Spin Top Collision Filter")]
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private string targetTag = "SpinTop";
    [SerializeField] private bool useTagFilter = true;

    [Header("Spin Top Bounce Settings")]
    [SerializeField] private float bounceForceMultiplier = 1.5f;
    [SerializeField] private float minBounceSpeed = 10f;
    [SerializeField] private float knockbackDuration = 0.3f;

    private float knockbackTimer = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void StateHandler()
    {
        if (dashing)
        {
            currentstate = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
            speedChangeFactor = dashSpeedChangeFactor;
        }
        else
        {
            currentstate = MovementState.sliding;
            desiredMoveSpeed = slideSpeed;
            speedChangeFactor = 1f;
        }

        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 0.01f)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float duration = 0.05f;
        float startValue = moveSpeed;

        float boostFactor = speedChangeFactor;
        while (time < duration)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / duration);
            time += Time.deltaTime * boostFactor;
            yield return null;
        }
        moveSpeed = desiredMoveSpeed;
        speedChangeFactor = 1f;
    }

    private void Update()
    {
        if (knockbackTimer > 0f)
            knockbackTimer -= Time.deltaTime;

        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.5f, whatIsGround);

        StateHandler();
        MyInput();

        if (grounded && (currentstate == MovementState.sliding || currentstate == MovementState.dashing))
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0.1f;
    }

    private void FixedUpdate()
    {
        MovePlayer();
        SpeedControl();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Fire1");
        verticalInput = Input.GetAxisRaw("Fire2");

        if (horizontalInput != 0 || verticalInput != 0)
        {
            Debug.Log("입력 발생: " + horizontalInput + ", " + verticalInput);
        }
    }

    private void MovePlayer()
    {
        Vector3 targetDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        smoothedMoveDirection = Vector3.Lerp(smoothedMoveDirection, targetDirection.normalized, Time.deltaTime * inputSmoothSpeed);
        float finalForce = moveSpeed * 8f;

        rb.AddForce(smoothedMoveDirection * finalForce, ForceMode.Force);
    }

    private void SpeedControl()
    {
        if (knockbackTimer > 0f) return;

        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = Vector3.MoveTowards(flatVel, flatVel.normalized * moveSpeed, 0.02f);
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.rigidbody == null) return;

        GameObject hitObj = collision.gameObject;

        bool isTargetLayer = (targetLayers.value & (1 << hitObj.layer)) != 0;
        if (!isTargetLayer) return;

        if (useTagFilter && !string.IsNullOrEmpty(targetTag))
        {
            if (!hitObj.CompareTag(targetTag)) return;
        }

        ContactPoint contact = collision.GetContact(0);
        Vector3 normal = contact.normal;
        normal.y = 0f;
        normal.Normalize();

        Vector3 myFlatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        Vector3 reflectDir = Vector3.Reflect(myFlatVelocity.normalized, normal).normalized;

        if (reflectDir == Vector3.zero)
            reflectDir = -normal;

        float bounceSpeed = Mathf.Max(myFlatVelocity.magnitude * bounceForceMultiplier, minBounceSpeed);

        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        rb.AddForce(reflectDir * bounceSpeed, ForceMode.Impulse);

        knockbackTimer = knockbackDuration;
    }

    [ServerRpc]
    public void UpdateStateServerRpc(MovementState state)
    {
        currentstate = state;
    }
}