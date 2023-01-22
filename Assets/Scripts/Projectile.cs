using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using Unity.Mathematics;
using UnityEditorInternal;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public PlayerController playerController;
    private Transform spriteTransform;
    private BoxCollider2D boxCollider;

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
        }
        SetSpriteOrientation();
        CheckCollisions();
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
    private void CheckCollisions()
    {
        // Retrieve all colliders we have intersected after velocity has been applied
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, boxCollider.size, 0);

        foreach (Collider2D hit in hits)
        {
            // Ignore our own collider

            if (hit == boxCollider || hit == playerController.boxCollider || hit.gameObject.CompareTag("Projectile"))
                return;

            ColliderDistance2D colliderDistance = hit.Distance(boxCollider);

            // Ensure that we are still overlapping this collider
            // The overlap may no longer exist due to another intersected collider pushing us out of this one
            if (colliderDistance.isOverlapped)
            {
                transform.Translate(colliderDistance.pointA - colliderDistance.pointB);
                velocity = Vector2.zero;
                anchored = true;
            }
        }
    }

    public void PickedUp()
    {
        Destroy(gameObject);
        print("picked up");
    }
}
