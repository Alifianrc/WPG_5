using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public string playerName { get; private set; }

    private float playerDefaultSpeed = 3f;
    [HideInInspector] public float playerSpeed { get; set; }

    public int rowPos { get; set; }

    private GameManager manager;

    void Start()
    {
        manager = FindObjectOfType<GameManager>();

        playerSpeed = playerDefaultSpeed;

        playerName = SaveGame.LoadData().UserName;

        // Set Start position
        transform.position = new Vector3(manager.rowXPos[rowPos], -2, 0);

        // Need to be deleted later
        FindObjectOfType<CameraFollow>().playerPos = gameObject.transform;
    }

    
    void Update()
    {
        // If the game is started
        if (manager.GameIsStarted)
        {
            // Player start running
            transform.position = new Vector2(transform.position.x, transform.position.y + (playerSpeed * Time.deltaTime));
        }
    }


}
