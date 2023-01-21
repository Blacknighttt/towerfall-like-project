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
    [SerializeField] private float wallJumpForce = 50f;



    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;

    private Vector2 velocity;
<<<<<<< Updated upstream

    public Vector2 aimInput;
    public Vector2 lastAimInput = Vector2.right;
=======
    public float fallSpeed;
    
    public Vector2 aimInput = Vector2.right;
>>>>>>> Stashed changes
    private float movementInput = 0;

    public bool jumped = false;

    public bool grounded;
    public bool onWallRight;
    public bool onWallLeft;


    // Projectile
    [Header("Projectile")]
    public Projectile projectile;
    private bool isFiring;


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
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    // Get horizontal and vertical input
    public void OnAim(InputAction.CallbackContext _context)
    {
        aimInput = _context.ReadValue<Vector2>();
    }

    // Set movementInput to the input movement value
    public void OnMove(InputAction.CallbackContext _context)
    {
        movementInput = _context.ReadValue<Vector2>().x;
        aimInput = _context.ReadValue<Vector2>();
    }

    // Set jumped to true when input action is triggered
    public void OnJump(InputAction.CallbackContext _context)
    {
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
        if (_context.started)
            isFiring = true;
        if (_context.canceled)
        {
            Projectile localProjectile = Instantiate(projectile, transform.position, transform.rotation);
            if (aimInput != Vector2.zero)
            {
                localProjectile.SetDirection(aimInput);
            }
            else
                localProjectile.SetDirection(lastAimInput);

            isFiring = false;
        }
    }

    // Debug function
    public void OnDebug()
    {
        print("key pressed");
    }

    private void SetSpriteFacing()
    {
        if (lastAimInput.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (lastAimInput.x > 0)
        {
            spriteRenderer.flipX = false;
        }
    }


    private void Update()
    {
        SetSpriteFacing();
    }


    private void FixedUpdate()
    {
<<<<<<< Updated upstream
        if (aimInput != Vector2.zero)
        {
            if (aimInput.x != 0)
            {
                lastAimInput.x = aimInput.x;
            }
        }

=======
        
>>>>>>> Stashed changes
        if (grounded)
        {
            velocity.y = 0;

            if (jumped)
            {
                    // Calculate the velocity required to achieve the target jump height
                    velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(customGravity));
            }
        }
        else if (onWallLeft)
        {
            if (velocity.y < 0)
            {
                velocity.y /= 2;
            }
            if (jumped)
            {
                //velocity.x = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(customGravity) * -1);
                velocity = new Vector2(wallJumpForce * -1, Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(customGravity)));
            }
        }
        else if (onWallRight)
        {
            if (velocity.y < 0)
            {
                velocity.y /= 2;
            }
            if (jumped)
            {
                //velocity.x = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(customGravity) * -1);
                velocity = new Vector2(wallJumpForce * 1, Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(customGravity)));
            }
        }

        float acceleration = grounded ? walkAcceleration : airAcceleration;
        float deceleration = grounded ? groundDeceleration : airDeceleration;

        if (!isFiring)
        {
            if (movementInput != 0)
            {
                velocity.x = Mathf.MoveTowards(velocity.x, speed * movementInput, acceleration * Time.deltaTime);
            }
            else
            {
                velocity.x = Mathf.MoveTowards(velocity.x, 0, deceleration * Time.deltaTime);
            }
        }


        velocity.y += customGravity * Time.deltaTime;

        transform.Translate(velocity * Time.deltaTime);

        grounded = false;
        onWallRight = false;
        onWallLeft= false;

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
<<<<<<< Updated upstream
                // If we intersect an object on our right or left, set wall ??? to true
                else if (Vector2.Angle(colliderDistance.normal, Vector2.right) < 90 || Vector2.Angle(colliderDistance.normal, Vector2.left) < 90)
                {
                    print("wall");
=======
                if (Vector2.Angle(colliderDistance.normal, Vector2.right) < 90)
                {
                    onWallRight = true;
                }
                if (Vector2.Angle(colliderDistance.normal, Vector2.left) < 90)
                {
                    onWallLeft = true;
>>>>>>> Stashed changes
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
