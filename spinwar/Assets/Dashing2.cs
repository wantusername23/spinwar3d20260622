using System;
using System.Collections;
using UnityEngine;

public class Dashing2 : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerCam;
    private Rigidbody rb;
    private PlayerMovement2 pm;

    [Header("Dashing")]
    public float dashForce;
    public float dashUpwardForce;
    public float dashDuration;

    [Header("Cooldown")]
    public float dashCd;
    private float dashCdTimer;

    [Header("Input")]
    public KeyCode dashKey;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement2>();

    }
    private void Update()
    {
        if (Input.GetKeyDown(dashKey))
            Dash();
        if (dashCdTimer > 0)
            dashCdTimer -= Time.deltaTime;
    }
    private void Dash()
    {
        if (dashCdTimer > 0) return;
        else dashCdTimer = dashCd;

        pm.dashing = true;
        Vector3 forceToApply = orientation.forward * dashForce + orientation.up * dashUpwardForce;
        delayedForceToApply = forceToApply;
        Invoke(nameof(DelayedDashForce), 0.025f);
        Invoke(nameof(ResetDash), dashDuration);
    }
    private Vector3 delayedForceToApply;
    private void DelayedDashForce()
    {
        rb.AddForce(delayedForceToApply, ForceMode.Impulse);
    }
    // Update is called once per frame
    private void ResetDash()
    {
        pm.dashing = false;
    }
}
