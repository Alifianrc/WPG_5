﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    // Player name
    public string playerName;

    // Player default speed
    private float playerDefaultSpeed = 3f;
    [HideInInspector] public float playerSpeed { get; set; }

    // Position in arena (0 - 4)
    public int rowPos { get; set; }

    // Main control
    private GameManager manager;
    private Client network;

    // UI 
    private Slider mySlider;
    private Transform finishPoint;
    private Vector2 startPos;

    // Swipe control
    private Vector2 startTouchPos;
    private Vector2 endTouchPos;
    private Vector2 currentTouchPos;
    private bool touchStopped;
    private bool touchIsOn;
    private bool rowChanged;

    // Swipe control range
    private float swipeRange = 50;
    private float tabRange = 10;

    void Start()
    {
        // Game manager
        manager = FindObjectOfType<GameManager>();
        network = FindObjectOfType<Client>();
        finishPoint = manager.FinishPoint;

        // Set camera
        if (playerName == network.MyName)
        {
            FindObjectOfType<CameraFollow>().playerPos = gameObject.transform;
            mySlider = manager.PlayerSlider;
            mySlider.value = 0;
            startPos = new Vector2(transform.position.x, transform.position.y);
            finishPoint = manager.FinishPoint;
        }

        // Set player speed
        playerSpeed = playerDefaultSpeed;

        // Set Start position
        transform.position = new Vector3(manager.rowXPos[rowPos], -2, 0);
    }

    
    void Update()
    {
        // If the game is started
        if (manager.GameIsStarted)
        {
            // Player start running
            transform.position = new Vector2(transform.position.x, transform.position.y + (playerSpeed * Time.deltaTime));

            // Check changing row
            if (rowChanged)
            {
                // Send massage
                MovePositionRow();
            }

            // If this player is mine
            if (playerName == network.MyName)
            {
                // Detect swipe screen
                SwipeControl();

                // UI
                mySlider.value = (transform.position.y - startPos.y) / (finishPoint.transform.position.y - startPos.y);

                // Check if finish
                if (transform.position.y > finishPoint.position.y)
                {
                    // Tell server

                    // Some UI

                }
            }
        }
    }

    // Start Sync position
    public void BeginSyncPos()
    {
        StartCoroutine(SyncPos());
    }
    private IEnumerator SyncPos()
    {
        while (manager.GameIsStarted)
        {
            // Sync position
            string[] massage = new string[] { "SyncPlr", transform.position.x.ToString(), transform.position.y.ToString() };
            network.SendMassageClient("AllES", massage);

            yield return new WaitForSeconds(5f);
        }
    }

    private void SwipeControl()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPos = Input.mousePosition;
            touchIsOn = true;
        }
        if(touchIsOn)
        {
            currentTouchPos = Input.mousePosition;
            Vector2 distance = currentTouchPos - startTouchPos;
            
            if (!touchStopped)
            {
                if (distance.x < -swipeRange)
                {
                    if(rowPos != 0)
                    {
                        rowPos--;
                        string[] mas = new string[] { "ChangeRow", rowPos.ToString() };
                        network.SendMassageClient("All", mas);
                    }
                    touchStopped = true;
                }
                else if (distance.x > swipeRange)
                {
                    if (rowPos != 4)
                    {
                        rowPos++;
                        string[] mas = new string[] { "ChangeRow", rowPos.ToString() };
                        network.SendMassageClient("All", mas);
                    }
                    touchStopped = true;
                }
                else if (distance.y > swipeRange)
                {
                    Debug.Log("Up");
                    touchStopped = true;
                }
                else if (distance.y < -swipeRange)
                {
                    Debug.Log("Down");
                    touchStopped = true;
                }
            }
        }

        if(Input.GetMouseButtonUp(0))
        {
            touchIsOn = false;
            touchStopped = false;
            endTouchPos = Input.mousePosition;
            Vector2 distance = endTouchPos - startTouchPos;

            if(Mathf.Abs(distance.x) < tabRange && Mathf.Abs(distance.y) < tabRange)
            {
                Debug.Log("Tab");
            }
        }
    }
    private void MovePositionRow()
    {
        float beginPos = transform.position.x;
        Vector2 newPos = new Vector2(Mathf.Lerp(beginPos, manager.rowXPos[rowPos], .1f), transform.position.y);
        transform.position = newPos;

        if(transform.position.x == manager.rowXPos[rowPos])
        {
            rowChanged = false;
        }
    }

    public void SetBoolRowChange(int newRow)
    {
        rowPos = newRow;
        rowChanged = true;
    }
}