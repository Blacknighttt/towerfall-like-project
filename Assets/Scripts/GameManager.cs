using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject[] powerUp;
    public Transform[] spawnPos;

    private float spawnInTime = 2;
    private float repeatTime = 5;

    //audio
    public AudioSource audioSourcePowerUp;

    // Start is called before the first frame update
    private void Start()
    {
        InvokeRepeating("SpawnPowerUp", spawnInTime, repeatTime);
    }
    private void SpawnPowerUp()
    {
        int randomIndex = Random.Range(0, powerUp.Length);
        int randomSpawn = Random.Range(0, spawnPos.Length);
        Instantiate(powerUp[randomIndex], spawnPos[randomSpawn].transform.position, Quaternion.identity);
        audioSourcePowerUp.Play();
    }
}
