using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField] private string PlatformName;
    private Transform destroyPoint;

    private GameManager manager;

    // Start is called before the first frame update
    void Start()
    {
        manager = FindObjectOfType<GameManager>();
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
            Debug.Log("Player Dead");
        }
        else
        {
            Debug.Log("Collision Obstacle : " + collision.collider.tag);
        }
    }
}
