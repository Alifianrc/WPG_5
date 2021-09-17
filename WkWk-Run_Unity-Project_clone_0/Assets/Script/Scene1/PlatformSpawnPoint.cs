using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformSpawnPoint : MonoBehaviour
{
    private GameManager manager;

    private float speed = 5f;
    private float maxSpeed = 10f;
    private float accSpeed = .2f;

    void Start()
    {
        manager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        // Only move when game is on
        if(manager.GameIsStarted && !manager.GameIsFinished)
        {
            transform.position = new Vector2(transform.position.x, transform.position.y + (speed * Time.deltaTime));
            if(speed <= maxSpeed)
            {
                speed += accSpeed * Time.deltaTime;
            }
        }
    }
}
