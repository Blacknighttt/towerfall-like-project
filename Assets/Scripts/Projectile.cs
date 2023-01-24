using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.ExceptionServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Animations;

public class Projectile : MonoBehaviour
{
    public GameObject owner;
    private Transform spriteTransform;
    private BoxCollider2D boxCollider;
    

    public LayerMask hitLayer;
    public float speed = 100;
    public Vector2 direction;
    public float waitGravity = 0.5f;
    private float gravity = 0f;
    public float projectileGravity = 0.05f;
    private Vector2 velocity;
    public bool anchored;
    public bool deflected;
    private float noCollisionTimer = 0;

    //particle
    public GameObject particleHit;

    //audio
    public AudioSource audioSourceHit;
    public AudioSource audioSourceHitWall;
    public AudioSource audioSourceHitShield;
    public AudioSource audioSourceSuicide;


    void Awake()
    {
        spriteTransform = transform.GetChild(0).GetComponent<Transform>();
        boxCollider = GetComponent<BoxCollider2D>();
    }


    public void SetDirection(Vector2 _direction)
    {
        direction = _direction;
        StartCoroutine(WaitForGravity());
        noCollisionTimer = 0.1f;
    }

    public void SetOwner(GameObject _owner)
    {
        owner = _owner;
    }

    public GameObject GetOwner()
    {
        return owner;
    }

    private void SetSpriteOrientation()
    {
        if(velocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            spriteTransform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }


    private void FixedUpdate()
    {
        if (noCollisionTimer >= 0)
            noCollisionTimer -= Time.deltaTime;

        if (!anchored)
        {
            velocity = new Vector2(direction.x, direction.y -= gravity) * speed;
            transform.Translate(velocity * Time.deltaTime);

            // Check with Raycast if projectile meets a platform object
            if (Physics2D.BoxCast(transform.position, boxCollider.size, 20f, direction, 0.2f, hitLayer))
            {
                audioSourceHitWall.Play();
                velocity = Vector2.zero;
                speed = 0;
                anchored = true;
            }
        }
        SetSpriteOrientation();
    }

    private IEnumerator WaitForGravity()
    {
        if (direction.y == 0f)
        {
            yield return new WaitForSeconds(waitGravity*2);
        }
        else yield return new WaitForSeconds(waitGravity);

        gravity = projectileGravity;
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        switch (collider.gameObject.tag)
        {
            case "Player":
                if (!anchored && !deflected)
                {
                    PlayerController player = collider.gameObject.GetComponent<PlayerController>();
                    if(collider.gameObject != owner)
                    {
                        print("ProjectileOnCollision2D: Kill Enemy");
                        player.GetHit();
                        audioSourceHit.Play();
                        GameObject particlehit = Instantiate(particleHit, player.transform.position, player.transform.rotation);
                        particlehit.transform.SetParent(player.transform);
                    }
                    else if (noCollisionTimer <= 0)
                    {
                        print("ProjectileOnCollision2D: Kill Own Player");
                        audioSourceSuicide.Play();
                        player.GetHit();
                    }
                }
                break;

            case "PlayerShield":
                GameObject shieldOwner = collider.gameObject.transform.parent.gameObject;
                if (shieldOwner != owner)
                {
                    Deflect(shieldOwner.GetComponent<PlayerController>());
                }
                else if (noCollisionTimer <= 0)
                {
                    Deflect(shieldOwner.GetComponent<PlayerController>());
                }
                break;

            default:
                print("ProjectileOnCollision2D: Unknown Tag (" + collider.gameObject.tag + ")");
                break;
        }
    }

    //public void OnCollisionEnter2D(Collision2D collision)
    //{
    //    switch (collision.gameObject.tag)
    //    {
    //        case "Player":
    //            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
    //            if (collision.gameObject != owner)
    //            {
    //                if (!anchored)
    //                {
    //                    print("ProjectileOnCollision2D: Kill Enemy");
    //                    playerController.GetHit();
    //                    audioSourceHit.Play();
    //                }
    //            }
    //            else if (collision.GetContact(0).normal.y > 0 && noCollisionTimer <= 0)
    //            {
    //                print("ProjectileOnCollision2D: Kill Own Player");
    //                audioSourceSuicide.Play();
    //                playerController.GetHit();
    //            }
    //            break;
    //
    //        default:
    //            print("ProjectileOnCollision2D: Unknown Tag (" + collision.gameObject.tag + ")");
    //            break;
    //    }
    //}

    private void Deflect(PlayerController _player)
    {
        print("deflect");
        direction = direction * - 0.4f;
        gravity = projectileGravity;
        deflected = true;
        _player.GetHit();
    }


    public void PickedUp()
    {
        Destroy(gameObject);
        print("picked up");
    }
}
