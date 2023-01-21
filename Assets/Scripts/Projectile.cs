using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using Unity.Mathematics;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public PlayerController playerController;
    private Transform spriteTransform;

    public float speed = 100;
    public Vector2 direction;
    public float waitGravity = 0.5f;
    private float gravity = 0f;
    public float projectileGravity = 0.05f;
    private Vector2 velocity;
    
    void Awake()
    {
        spriteTransform = transform.GetChild(0).GetComponent<Transform>();
    }

    public void SetDirection(Vector2 _direction)
    {
        direction = _direction;
        StartCoroutine(WaitForGravity());
    }

    private void SetSpriteOrientation()
    {
        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        spriteTransform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }


    private void FixedUpdate()
    {
        velocity = new Vector2(direction.x, direction.y -= gravity) * speed;
        transform.Translate(velocity * Time.deltaTime);
        print(gravity);
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
}
