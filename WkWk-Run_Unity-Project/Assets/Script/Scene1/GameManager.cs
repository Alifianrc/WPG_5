using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Panels
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject waitingPlayerPanel;

    // Start is called before the first frame update
    void Start()
    {
        // Wait for others player
        waitingPlayerPanel.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
