using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PowerUpShield : MonoBehaviour
{ 
    public GameObject shield;
    public PlayerController playerController;

    

    public void Activate()
        {
        Debug.Log("PowerUp!");
        GameObject _shield = Instantiate(shield, playerController.transform.position, playerController.transform.rotation);
            _shield.transform.SetParent(playerController.transform);
      
            Destroy(gameObject);
        }

    
}
