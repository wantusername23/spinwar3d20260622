
using System;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(NetworkObject))]

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField]
    public float moveSpeed;
    [SerializeField]
    public float slideSpeed;
    [SerializeField]
    public float inputSmoothSpeed;
    [SerializeField]
    public float dashSpeed;
    [SerializeField]
    public float dashSpeedChangeFactor;
    [SerializeField]
    public float groundDrag;
    [Header("Ground Check")]
    [SerializeField]
    public float playerHeight;
    [SerializeField]
    public LayerMask whatIsGround;
    bool grounded;

    [SerializeField]
    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Vector3 smoothedMoveDirection;

    Rigidbody rb;
    [SerializeField]
    public MovementState currentstate;
    public enum MovementState
    {
        sliding,
        dashing
    }
    [SerializeField]
    public bool dashing;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        
    }
    [SerializeField]
    private float desiredMoveSpeed;
    [SerializeField]
    private float lastDesiredMoveSpeed;
    [SerializeField]
    private float speedChangeFactor;
    [SerializeField]
    private MovementState lastState;


    [Header("Spin Top Collision Filter")]
    [SerializeField] private LayerMask targetLayers;     
    [SerializeField] private string targetTag = "SpinTop";
    [SerializeField] private bool useTagFilter = true;

    [Header("Spin Top Collision")]
    [SerializeField] private float bounceForceMultiplier = 1.5f; 
    [SerializeField] private float minBounceSpeed = 10f;          
    [SerializeField] private float knockbackDuration = 0.3f;
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
        //bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
        //if (lastState == MovementState.dashing) keepMomentum = true;
        //if (desiredMoveSpeedHasChanged)
        //{
        // if (keepMomentum)
        //{
        // StopAllCoroutines();
        // StartCoroutine(SmoothlyLerpMoveSpeed());

        //}
        // else
        // {
        //    StopAllCoroutines();
        //    moveSpeed = desiredMoveSpeed;
        //}


        lastDesiredMoveSpeed = desiredMoveSpeed;

    }
    private float knockbackTimer = 0f;

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

    void Update()
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
        horizontalInput = Input.GetAxisRaw("Horizontal");
        //verticalInput = Input.GetAxisRaw("Vertical");
        verticalInput = Input.GetAxisRaw("Vertical");
        if (horizontalInput != 0 || verticalInput != 0)
        {
            Debug.Log("입력 발생: " + horizontalInput + ", " + verticalInput);
        }
    }

    private void MovePlayer()
    {
        // 목표 방향 계산
        Vector3 targetDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // [답답함의 핵심] 방향 전환을 아주 느리게 만듭니다.
        smoothedMoveDirection = Vector3.Lerp(smoothedMoveDirection, targetDirection.normalized, Time.deltaTime * inputSmoothSpeed);

        // 이동하려는 방향과 현재 속도가 반대일 때 힘을 더 깎아버려 답답함을 가중시킵니다.
        float finalForce = moveSpeed * 8f;
        

        rb.AddForce(smoothedMoveDirection * finalForce, ForceMode.Force);
    }
    // Update is called once per frame
    private void SpeedControl()
    {
        if (knockbackTimer > 0f) return;
        // 수평 속도(X, Z)만 계산하여 중력(Y)에 영향을 주지 않도록 합니다.
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // 설정한 moveSpeed를 초과했을 경우
        if (flatVel.magnitude > moveSpeed)
        {
            // [답답함의 핵심] 즉시 고정하지 않고, 현재 속도에서 목표 속도로 '서서히' 줄입니다.
            // 0.05f 값은 '제동의 부드러움'입니다. 이 값이 작을수록 더 묵직하고 답답하게 속도가 줄어듭니다.
            Vector3 limitedVel = Vector3.MoveTowards(flatVel, flatVel.normalized * moveSpeed, 0.02f);

            // 실제 리지드바디에 적용 (Y축 속도는 보존하여 중력 유지)
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        // 1. Rigidbody 유무 확인
        if (collision.rigidbody == null) return;

        GameObject hitObj = collision.gameObject;

        // 2. 레이어 필터 검사 (비트마스크 연산)
        bool isTargetLayer = (targetLayers.value & (1 << hitObj.layer)) != 0;
        if (!isTargetLayer) return;

        // 3. 태그 필터 검사 (옵션 활성화 시)
        if (useTagFilter && !string.IsNullOrEmpty(targetTag))
        {
            if (!hitObj.CompareTag(targetTag)) return;
        }

        // 4. 반사각 계산 및 힘 적용
        ContactPoint contact = collision.GetContact(0);
        Vector3 normal = contact.normal;
        normal.y = 0f;
        normal.Normalize();

        Vector3 myFlatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        Vector3 reflectDir = Vector3.Reflect(myFlatVelocity.normalized, normal).normalized;

        if (reflectDir == Vector3.zero)
            reflectDir = -normal;

        float bounceSpeed = Mathf.Max(myFlatVelocity.magnitude * bounceForceMultiplier, minBounceSpeed);

        // 기존 수평 속도를 초기화하고 충격량(Impulse) 적용
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        rb.AddForce(reflectDir * bounceSpeed, ForceMode.Impulse);

        // 넉백 동안 제동 해제
        knockbackTimer = knockbackDuration;
    }
    [ServerRpc]
    public void UpdateStateServerRpc(MovementState state)
    {
        currentstate = state;
    }

}


