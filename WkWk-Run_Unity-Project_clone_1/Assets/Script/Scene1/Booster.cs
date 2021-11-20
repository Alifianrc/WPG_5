using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Booster : MonoBehaviour
{
    [SerializeField] private string PlatformName;
    private Transform destroyPoint;

    private GameManager manager;
    private Client network;

    // Start is called before the first frame update
    void Start()
    {
        manager = FindObjectOfType<GameManager>();
        network = FindObjectOfType<Client>();
        destroyPoint = FindObjectOfType<CameraFollow>().PlatformDestroyerPoint;
    }

    // Update is called once per frame
    void Update()
    {
        if (manager.GameIsStarted)
        {
            // Destroy this object if it wasn't needed
            if (transform.position.y < destroyPoint.position.y)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (PlatformName == "Coin")
            {
                // Add coin to player
                PlayerManager player = collision.GetComponent<PlayerManager>();
                if (player.playerName == network.MyName)
                {
                    player.GetCoin(10);
                }
            }

            // Destroy coin
            Destroy(gameObject);
        }
        else if (collision.tag == "Obstacle")
        {
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Obstacle Booster Collision : " + collision.tag);
        }
    }
}
