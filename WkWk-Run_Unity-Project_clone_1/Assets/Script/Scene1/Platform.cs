using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if(PlatformName == "Coin")
            {
                // Add coin to player

                // Destroy coin
                Destroy(gameObject);
            }
        }
    }
}
