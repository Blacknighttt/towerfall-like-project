using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    public GameObject[] powerUp;
    public Transform[] spawnPos;
    public Collider2D spawnCollider;
    private ContactFilter2D filter;


    private float spawnInTime = 2;
    private float repeatTime = 5;

    //audio
    public AudioSource audioSourcePowerUp;

    // Start is called before the first frame update
    private void Start()
    {
        filter = new ContactFilter2D().NoFilter();
        InvokeRepeating("SpawnPowerUp", spawnInTime, repeatTime);
    }

    private void SpawnPowerUp()
    {
        List<Collider2D> results = new List<Collider2D>();
        int spawnCount = spawnPos.Length;
        int randomIndex;
        int randomSpawn;

        do
        {
            results.Clear();
            randomIndex = Random.Range(0, powerUp.Length);
            randomSpawn = Random.Range(0, spawnPos.Length);
            spawnPos[randomSpawn].GetComponent<Collider2D>().OverlapCollider(filter, results);
            spawnCount--;
        } while (results.Count != 0 && spawnCount > 0);

        if (results.Count == 0)
        {
            Instantiate(powerUp[randomIndex], spawnPos[randomSpawn].transform.position, Quaternion.identity);
            audioSourcePowerUp.Play();
            print("spawn powerup");
        }
    }
}
