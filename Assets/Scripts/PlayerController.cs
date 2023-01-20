using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    // Movement variables
    [Header ("Movement")]
    [SerializeField] private float speed = 10;
    [SerializeField] private float walkAcceleration = 100;
    [SerializeField] private float airAcceleration = 100;
    [SerializeField] private float groundDeceleration = 150;
    [SerializeField] private float airDeceleration = 100;
    [SerializeField] private float jumpHeight = 3;
    [SerializeField] private float customGravity = -30f;

    private BoxCollider2D boxCollider;

    private Vector2 velocity;
    Vector2[] directions = { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(1, -1), new Vector2(0, -1), new Vector2(-1, -1), new Vector2(-1, 0), new Vector2(-1, 1) };

    private float movementInput = 0;
    private float testMovementInputY = 0;

    private bool jumped = false;

    private bool grounded;

    // Projectile
    public GameObject projectile;
    public bool fired;

    // Dash variables
    [Header ("Dash")]
    [SerializeField] private float dashingPower = 30f;
    [SerializeField] private float dashingTimer = 0.2f;
    [SerializeField] private float dashingCooldown = 1f;
    private TrailRenderer trailRenderer; 
    private bool canDash = true;
    private bool isDashing;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    // Set movementInput to the input movement value
    public void OnMove(InputAction.CallbackContext _context)
    {
        movementInput = _context.ReadValue<Vector2>().x;
    }

    // Set jumped to true when input action is triggered
    public void OnJump(InputAction.CallbackContext _context)
    {
        //jumped = _context.action.triggered;
        if (!_context.action.IsPressed())
        jumped = _context.action.triggered;
    }

    // Start coroutine when input dash is triggered
    public void OnDash(InputAction.CallbackContext _context)
    {
        if (canDash & _context.action.triggered)
        StartCoroutine(Dash());
    }

    public void OnFire(InputAction.CallbackContext _context)
    {
        Vector2 inputDirection = new Vector2(movementInput, testMovementInputY);
        Instantiate(projectile, transform.position, Quaternion.identity);
    }

    // Debug function
    public void OnDebug()
    {
        print("key pressed");
    }






    private void FixedUpdate()
    {

        if (grounded)
        {
            velocity.y = 0;

            if (jumped)
            {
                // Calculate the velocity required to achieve the target jump height
                velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(customGravity));
            }
        }

        float acceleration = grounded ? walkAcceleration : airAcceleration;
        float deceleration = grounded ? groundDeceleration : airDeceleration;

        if (movementInput != 0)
        {
            velocity.x = Mathf.MoveTowards(velocity.x, speed * movementInput, acceleration * Time.deltaTime);
        }
        else
        {
            velocity.x = Mathf.MoveTowards(velocity.x, 0, deceleration * Time.deltaTime);
        }

        velocity.y += customGravity * Time.deltaTime;

        transform.Translate(velocity * Time.deltaTime);

        grounded = false;

        // Retrieve all colliders we have intersected after velocity has been applied
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, boxCollider.size, 0);

        foreach (Collider2D hit in hits)
        {
            // Ignore our own collider
            if (hit == boxCollider)
                continue;

            ColliderDistance2D colliderDistance = hit.Distance(boxCollider);

            // Ensure that we are still overlapping this collider
            // The overlap may no longer exist due to another intersected collider pushing us out of this one
            if (colliderDistance.isOverlapped)
            {
                transform.Translate(colliderDistance.pointA - colliderDistance.pointB);

                // If we intersect an object beneath us, set grounded to true
                if (Vector2.Angle(colliderDistance.normal, Vector2.up) < 90 && velocity.y < 0)
                {
                    grounded = true;
                }
            }
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = customGravity;
        customGravity = 0;
        velocity = new Vector2(movementInput * dashingPower, 0f);
        trailRenderer.emitting = true;
        yield return new WaitForSeconds(dashingTimer);

        trailRenderer.emitting = false;
        customGravity = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
}
