using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks
{
    // Panels
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject waitingPlayerPanel;

    // Boolean
    public bool GameIsStarted { get; set; }
    public bool GameIsFinished { get; set; }

    // Screen size
    private float screenWidth;
    private float rowCount = 5;
    private float rowDist;

    // Platform Spawn Point
    [SerializeField] private Transform platformSpawnPoint;

    // Platform
    [SerializeField] private GameObject platformGround;
    [SerializeField] private GameObject platformWater;

    // Trap
    [SerializeField] private GameObject trapLava;
    [SerializeField] private GameObject trapBomb;

    // Scaling
    private float scaleFix;

    // Row position
    [SerializeField] private Transform[] rowPos;

    // Start is called before the first frame update
    private void Start()
    {
        // Panels
        startPanel.SetActive(true);
        waitingPlayerPanel.SetActive(true);

        // Set screen size
        Vector2 minPosCamera = Camera.main.ScreenToWorldPoint(new Vector2(0, 0));
        Vector2 maxPosCamera = Camera.main.ScreenToWorldPoint(new Vector2(Camera.main.pixelWidth, Camera.main.pixelHeight));

        // Screen width size
        screenWidth = maxPosCamera.x - minPosCamera.x;

        // Calculating row position
        rowDist = screenWidth / rowCount;
        for(int i = 0; i < rowPos.Length; i++)
        {
            rowPos[i].position = new Vector3((minPosCamera.x + (rowDist * 0.5f)) + (rowDist * i), minPosCamera.y + (rowDist * 0.5f), 0);
        }

        // Calculating scale
        float width = Camera.main.orthographicSize * 2.0f * Screen.width / Screen.height;
        scaleFix = width / rowCount;

        // Spawn Player
        

        // Creating start map
        StartMapSpawn();
    }

    // Update is called once per frame
    void Update()
    {
        

    }

    // Creating map for start
    private void StartMapSpawn()
    {
        while(platformSpawnPoint.position.y > rowPos[0].position.y)
        {
            for (int i = 0; i < rowPos.Length; i++)
            {
                // Instantiate new platform
                GameObject temp = Instantiate(platformGround, new Vector3(rowPos[i].position.x, rowPos[i].position.y, 10), Quaternion.identity);
                temp.transform.localScale = new Vector3(scaleFix, scaleFix, scaleFix);

                // relocate row position
                rowPos[i].position = new Vector3(rowPos[i].position.x, rowPos[i].position.y + rowDist, rowPos[i].position.z);
            } 
        }
    }
}
