using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.ExceptionServices;
using Unity.Mathematics;
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
    
    void Awake()
    {
        spriteTransform = transform.GetChild(0).GetComponent<Transform>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    public void SetDirection(Vector2 _direction)
    {
        direction = _direction;
        StartCoroutine(WaitForGravity());
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
        if (!anchored)
        {
            velocity = new Vector2(direction.x, direction.y -= gravity) * speed;
            transform.Translate(velocity * Time.deltaTime);

            // Check with Raycast if projectile meets a platform object
            if (Physics2D.BoxCast(transform.position, boxCollider.size, 20f, direction, 0.2f, hitLayer))
            {
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

    public void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Player":
                if(collision.gameObject != owner)
                {
                    if (!anchored)
                    {
                        print("ProjectileOnCollision2D: Kill Enemy");
                        Destroy(collision.gameObject);
                    }
                }
                else if (collision.GetContact(0).normal.y > 0)
                {
                    print("ProjectileOnCollision2D: Kill Own Player");
                    //Destroy(owner); //Bug, we shall check direction + velocity or distance
                }
                break;

            default:
                print("ProjectileOnCollision2D: Unknown Tag (" + collision.gameObject.tag + ")");
                break;
        }
    }

    public void PickedUp()
    {
        Destroy(gameObject);
        print("picked up");
    }
}
