using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    // Movement variables
    [Header ("Movement")]
    [SerializeField] public float speed = 10;
    [SerializeField] private float walkAcceleration = 100;
    [SerializeField] private float airAcceleration = 100;
    [SerializeField] private float groundDeceleration = 150;
    [SerializeField] private float airDeceleration = 100;
    [SerializeField] private float jumpHeight = 3;
    [SerializeField] public float customGravity = -30f;
    [SerializeField] private float wallJumpForce = 50f;


    public PowerUp powerUp;
    public BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;

    private Vector2 velocity;

    public Vector2 aimInput;
    public Vector2 lastAimInput = Vector2.right;
    
    private float movementInput = 0;

    public bool jumped = false;

    public bool isGrounded;
    public bool onWallRight;
    public bool onWallLeft;

    // Raycast
    private float raycastDistance = 0.2f;
    public LayerMask groundLayer;
    public LayerMask wallLayer;



    // Projectile
    [Header("Projectile")]
    public Projectile projectile;
    private Projectile lastProjectile;
    private BoxCollider2D projectileCollider;
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

    // Instantiate projectile and set its direction to aim
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
            lastProjectile = localProjectile;
            projectileCollider = lastProjectile.GetComponentInChildren<BoxCollider2D>();
        }
    }

    // Dash coroutine
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

    public void Die()
    {
        Destroy(gameObject);
    }


    private void Update()
    {
        SetSpriteFacing();
        SetLastAim();
    }

    public void OnCollisionEnter(Collision collision)
    {
        print("collion enter");
    }


    private void FixedUpdate()
    {

        float acceleration = isGrounded ? walkAcceleration : airAcceleration;
        float deceleration = isGrounded ? groundDeceleration : airDeceleration;

        CalculateVerticalVelocity();

        CalculateHorizontalVelocity(acceleration, deceleration);

        SetMovement();

        CheckCollisions();
    }

    // Calculate x velocity
    private void CalculateHorizontalVelocity(float _acceleration, float _deceleration)
    {
        if (!isFiring)
        {
            if (movementInput != 0)
            {
                velocity.x = Mathf.MoveTowards(velocity.x, speed * movementInput, _acceleration * Time.deltaTime);
            }
            else
            {
                velocity.x = Mathf.MoveTowards(velocity.x, 0, _deceleration * Time.deltaTime);
            }
        }
    }

    // Set player movement based on its velocity
    private void SetMovement()
    {
        transform.Translate(velocity * Time.deltaTime);
    }

    // Shoot left or right based on last aim input if no new input
    private void SetLastAim()
    {
        if (aimInput != Vector2.zero)
        {
            if (aimInput.x != 0)
            {
                lastAimInput.x = aimInput.x;
            }
        }
    }

    // Set velocity based on jump (ground or wall)
    private void CalculateVerticalVelocity()
    {
        if (isGrounded)
        {
            velocity.y = 0;
        
            if (jumped)
            {
                // Calculate the velocity required to achieve the target jump height
                velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(customGravity));
            }
        }
        if (onWallLeft)
        {
            if (velocity.y < 0)
            {
                velocity.y /= 2;
            }
            if (jumped)
            {
                velocity = new Vector2(wallJumpForce * -1, Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(customGravity)));
            }
        }
        if (onWallRight)
        {
            if (velocity.y < 0)
            {
                velocity.y /= 2;
            }
            if (jumped)
            {
                velocity = new Vector2(wallJumpForce * 1, Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(customGravity)));
            }
        }
        velocity.y += customGravity * Time.deltaTime;
    }

    // Check all collisions


    private void CheckCollisions()
    {
        isGrounded = false;
        onWallRight = false;
        onWallLeft = false;
    
        // Retrieve all colliders we have intersected after velocity has been applied
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, boxCollider.size, 0);
    
        foreach (Collider2D hit in hits)
        {
            // Ignore our own collider & our own projectile
    
            if (hit == boxCollider || hit == projectileCollider)
                continue;
    
            ColliderDistance2D colliderDistance = hit.Distance(boxCollider);
    
            // Ensure that we are still overlapping this collider
            // The overlap may no longer exist due to another intersected collider pushing us out of this one
            if (colliderDistance.isOverlapped)
            {
                if (hit.gameObject.CompareTag("PowerUp"))
                {
                    hit.gameObject.GetComponent<PowerUp>().Activate();
                }
                transform.Translate(colliderDistance.pointA - colliderDistance.pointB);

                // If we intersect an object beneath us, set grounded to true
                if (Vector2.Angle(colliderDistance.normal, Vector2.up) < 90 && velocity.y < 0)
                {
                    isGrounded = true;
                }

                // If we intersect a player beneath us, kill player
                if (Vector2.Angle(colliderDistance.normal, Vector2.up) < 90 && hit.gameObject.CompareTag("Player"))
                {
                    hit.gameObject.GetComponent<PlayerController>().Die();
                }

                // If we intersect an object on our right or left, set wall ??? to true
                else if (Vector2.Angle(colliderDistance.normal, Vector2.right) < 90 || Vector2.Angle(colliderDistance.normal, Vector2.left) < 90)
                {
                    print("wall detected");
                    if (Vector2.Angle(colliderDistance.normal, Vector2.right) < 90)
                    {
                        onWallRight = true;
                    }
                    if (Vector2.Angle(colliderDistance.normal, Vector2.left) < 90)
                    {
                        onWallLeft = true;
                    }
                }
                if (hit.gameObject.CompareTag("Projectile"))
                {
                    Projectile projectile = hit.gameObject.GetComponent<Projectile>();
                    if (projectile.anchored)
                    {
                        print("projectile");
                        projectile.PickedUp();
                        return;
                    }
                }
            }
        }
    }
}
