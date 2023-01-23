using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject[] powerUp;

    private float spawnRangeX = 33f;
    private float spawnRangeY = -9;
    private float spawnInTime = 2;
    private float repeatTime = 10;

    //audio
    public AudioSource audioSourcePowerUp;

    // Start is called before the first frame update
    private void Start()
    {
        InvokeRepeating(nameof(SpawnPowerUp), spawnInTime, repeatTime);
    }
    private Vector2 RandomPos()
    {
        float spawnX = Random.Range(-spawnRangeX, spawnRangeX);
        float spawnY = spawnRangeY;

        Vector2 randomPos = new Vector2(spawnX, spawnY);
        return randomPos;
    }
    private void SpawnPowerUp()
    {
        int randomIndex = Random.Range(0, powerUp.Length);
        Instantiate(powerUp[randomIndex], RandomPos(), Quaternion.identity);
        audioSourcePowerUp.Play();
    }
}
