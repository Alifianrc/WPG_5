using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
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
            if(transform.position.y < destroyPoint.position.y)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Player")
        {
            PlayerManager player = collision.gameObject.GetComponent<PlayerManager>();
            if(player.playerName == network.MyName)
            {
                player.Dead();
            }
        }
        else
        {
            Debug.Log("Collision Obstacle : " + collision.collider.tag);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player" && PlatformName == "Water")
        {
            Debug.Log("Swimming");
        }
    }
}
