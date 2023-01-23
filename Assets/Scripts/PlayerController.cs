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
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;

    private Vector2 velocity;

    public Vector2 aimInput;
    public Vector2 lastAimInput = Vector2.right;
    
    private float movementInput = 0;

    public bool jumped = false;
    public bool isGrounded;
    private bool wallJump = false;
    private Vector2 wallDirection;

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

    //audio
    public AudioSource audioSourceJump;
    public AudioSource audioSourceDash;
    public AudioSource audioSourcePick;
    public AudioSource audioSourceThrow;

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
            audioSourceThrow.Play();
            localProjectile.SetOwner(this.gameObject);

            if (aimInput != Vector2.zero)
            {
                localProjectile.SetDirection(aimInput);
            }
            else
                localProjectile.SetDirection(lastAimInput);

            isFiring = false;
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
        audioSourceDash.Play();
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

    public void OnCollisionEnter2D(Collision2D collision)
    {
        switch(collision.gameObject.tag)
        {
            case "PowerUp":
                //print("OnCollisionEnter2D: PowerUp");
                collision.gameObject.GetComponent<PowerUp>().Activate();
                break;

            case "Wall":
                //print("OnCollisionEnter2D: Wall");
                wallJump = true;
                wallDirection = collision.GetContact(0).normal; // Set wall direction
                break;

            case "Projectile":
                Projectile projectile = collision.gameObject.GetComponent<Projectile>();
                if (projectile.anchored)
                {
                    print("OnCollisionEnter2D: Pickup Projectile");
                    projectile.PickedUp();
                    audioSourcePick.Play();
                }
                break;

            case "Ground":
                //print("OnCollisionEnter2D: Ground");
                isGrounded = true;
                break;

            case "Player":
                print("OnCollisionEnter2D: Player");
                if(collision.GetContact(0).normal.y > 0) 
                    collision.gameObject.GetComponent<PlayerController>().Die(); // Kill player beneath us
                break;

            default:
                print("OnCollisionEnter2D: Unknown Tag (" + collision.gameObject.tag + ")");
                break;
        }
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "PowerUp":
                //print("OnCollisionExit2D: PowerUp");
                break;

            case "Wall":
                //print("OnCollisionExit2D: Wall");
                wallJump = false;
                break;

            case "Projectile":
                //print("OnCollisionExit2D: Projectile");
                break;

            case "Ground":
                //print("OnCollisionExit2D: Ground");
                isGrounded = false;
                break;

            case "Player":
                print("OnCollisionEnter2D: Player");
                break;

            default:
                print("OnCollisionExit2D: Unknown Tag (" + collision.gameObject.tag + ")");
                break;
        }
    }


    private void FixedUpdate()
    {
        float acceleration = isGrounded ? walkAcceleration : airAcceleration;
        float deceleration = isGrounded ? groundDeceleration : airDeceleration;

        CalculateVerticalVelocity();

        CalculateHorizontalVelocity(acceleration, deceleration);

        SetMovement();
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
                audioSourceJump.Play();
            }
        }

        if (wallJump)
        {
            if (velocity.y < 0)
            {
                velocity.y /= 2;
            }
            if (jumped)
            {
                velocity = new Vector2(wallJumpForce * wallDirection.x, Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(customGravity)));
                audioSourceJump.Play();
            }
        }

        velocity.y += customGravity * Time.deltaTime;
    }
}
