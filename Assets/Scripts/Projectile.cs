using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using Unity.Mathematics;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Rigidbody2D rbProjectile;
    public PlayerController playerController;

    public float speed;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame

    private void FixedUpdate()
    {
        rbProjectile.AddForce(Vector2.left* speed, ForceMode2D.Impulse);
    }
}
