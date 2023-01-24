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
    // Globals
    private bool isDead = false;
    
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


    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private PlayerInput playerInput;

    public Vector2 velocity;

    public Vector2 aimInput;
    public Vector2 lastAimInput = Vector2.right;
    
    private float movementInput = 0;

    // Powerups
    public GameObject shieldPrefab;
    private GameObject equipedShield;
    private bool hasShieldPowerUp = false;

    private bool hasSpeedPowerUp = false;
    public float speedPowerUpTimer = 5f;

    // Wall & ground
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
    private int ammo = 5;
    public bool canShoot;


    // Dash variables
    [Header ("Dash")]
    [SerializeField] private float dashingPower = 30f;
    [SerializeField] private float dashingTimer = 0.2f;
    [SerializeField] private float dashingCooldown = 1f;
    private TrailRenderer trailRenderer; 
    private bool canDash = true;
    private bool isDashing;

    // Particle Anim
    public GameObject jumpSmoke;
    public GameObject slideSmoke;
    
    
    // Audio
    public AudioSource audioSourceJump;
    public AudioSource audioSourceDash;
    public AudioSource audioSourcePick;
    public AudioSource audioSourceThrow;
    public AudioSource audioSourceSlide;
    public AudioSource audioSourceShield;
    public AudioSource audioSourceSpeed;
    public AudioSource audioSourceShieldBreak;
    public AudioSource EmptyAmmo;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        trailRenderer = GetComponent<TrailRenderer>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
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
        canShoot = ammo > 0; 
        if (canShoot)
        {
            if (_context.started)
            {
                isFiring = true;
                animator.SetTrigger("Throw");
            }
            if (_context.canceled)
            {
                Projectile localProjectile = Instantiate(projectile, transform.position, transform.rotation);
                animator.SetTrigger("ThrowReleased");
                audioSourceThrow.Play();
                localProjectile.SetOwner(this.gameObject);
                ammo -= 1;

                if (aimInput != Vector2.zero)
                {
                    localProjectile.SetDirection(aimInput);
                }
                else
                    localProjectile.SetDirection(lastAimInput);

                isFiring = false;
            }
        }
        else
        {
            EmptyAmmo.Play();
        }

    }

    // Dash coroutine
    private IEnumerator Dash()
    {
        animator.SetTrigger("Dash");
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

    public void GetHit()
    {
        if (hasShieldPowerUp && equipedShield != null)
        {
            Destroy(equipedShield);
            audioSourceShieldBreak.Play();
            hasShieldPowerUp = false;
        }
        else if (!isDead)
        {
            animator.SetTrigger("Death");
            
            playerInput.DeactivateInput();
        }
    }

    private void FixedUpdate()
    {
        float acceleration = isGrounded ? walkAcceleration : airAcceleration;
        float deceleration = isGrounded ? groundDeceleration : airDeceleration;

        CalculateVerticalVelocity();

        CalculateHorizontalVelocity(acceleration, deceleration);

        SetMovement();

        animator.SetBool("Grounded", isGrounded);
        animator.SetBool("HorizontalMovement", velocity.x != 0);
        animator.SetBool("Fall", velocity.y < -2f);
    }

    private void Update()
    {
        SetSpriteFacing();
        SetLastAim();
        print(ammo);
    }
    
    public void OnCollisionEnter2D(Collision2D collision)
    {
        switch(collision.gameObject.tag)
        {
            case "Wall":
                //print("OnCollisionEnter2D: Wall");
                wallJump = true;
                wallDirection = collision.GetContact(0).normal; // Set wall direction
                break;

            case "Ground":
                //print("OnCollisionEnter2D: Ground");
                isGrounded = true;
                break;

            case "Player":
                print("OnCollisionEnter2D: Player");
                if(collision.GetContact(0).normal.y > 0) 
                    collision.gameObject.GetComponent<PlayerController>().GetHit(); // Kill player beneath us
                break;

            default:
                print("OnCollisionEnter2D: Unknown Tag (" + collision.gameObject.tag + ")");
                break;
        }
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        switch (collider.gameObject.tag)
        {
            case "PowerUp":
                print("OnCollisionEnter2D: PowerUp");
                if (collider.gameObject.layer == LayerMask.NameToLayer("ShieldUp") && !hasShieldPowerUp)
                {
                    PickUpShield(collider.gameObject);
                }
                if (collider.gameObject.layer == LayerMask.NameToLayer("SpeedUp") && !hasSpeedPowerUp)
                {
                    PickUpSpeed(collider.gameObject);
                }
                break;

            case "Projectile":
                Projectile projectile = collider.gameObject.GetComponent<Projectile>();
                if (projectile.anchored)
                {
                    print("OnCollisionEnter2D: Pickup Projectile");
                    projectile.PickedUp();
                    ammo += 1;
                    audioSourcePick.Play();
                }
                break;

            default:
                    print("OnTriggerEnter2D: Unknown Tag (" + collider.gameObject.tag + ")");
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
    private void PickUpShield(GameObject _powerUp)
    {
        print("J'ai un shield");
        Destroy(_powerUp);
        audioSourceShield.Play();
        GameObject shield = Instantiate(shieldPrefab, transform.position, transform.rotation);
        shield.transform.SetParent(transform);
        hasShieldPowerUp = true;
        equipedShield = shield;
    }

    private void PickUpSpeed(GameObject _powerUp)
    {
        float timer;
        audioSourceSpeed.Play();
        print("je vais plus vite");
        Destroy(_powerUp);

        timer = speedPowerUpTimer; 
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
                StartAnimationJump();
                audioSourceJump.Play();
                GameObject jumpsmoke = Instantiate(jumpSmoke, transform.position, transform.rotation);
                jumpsmoke.transform.SetParent(transform);

            }
        }////////////////////

        if (wallJump)
        {
            if (velocity.y < 0)
            {
                velocity.y /= 2;
                if (!audioSourceSlide.isPlaying)
                {
                audioSourceSlide.Play();
                    GameObject slidesmoke = Instantiate(slideSmoke, transform.position, transform.rotation);
                    slidesmoke.transform.SetParent(transform);
                    slidesmoke.SetActive(true);
                }    
                animator.SetBool("WallSlide", true);
                
            }
            if (jumped)
            {
                animator.SetBool("WallSlide", false);
                animator.SetBool("Jump", true);
                velocity = new Vector2(wallJumpForce * wallDirection.x, Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(customGravity)));
                audioSourceJump.Play();
                StartAnimationJump();
                

            }
        }
        else if (!wallJump)
        { 
            
            animator.SetBool("WallSlide", false);
            audioSourceSlide.Stop();
        }

        velocity.y += customGravity * Time.deltaTime;
    }

    public void StartAnimationJump()
    {
        animator.SetBool("Jump", true);
    }
    public void EndAnimationJump()
    {
        animator.SetBool("Jump", false);
    }

}
