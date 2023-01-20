using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using Unity.Mathematics;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Rigidbody2D rbProjectile;
    public PlayerController playerController;

    public float speed = 100;
    public Vector2 direction;
    private Vector2 velocity;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetDirection(Vector2 _direction)
    {
        direction = _direction;
    }

    // Update is called once per frame

    private void FixedUpdate()
    {
        velocity = direction * speed;
        transform.Translate(velocity * Time.deltaTime);
    }
}
