using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkBuilder : MonoBehaviour
{
    [SerializeField] private GameObject NetworkClient;

    private void Awake()
    {
        if (!FindObjectOfType<Client>())
        {
            Instantiate(NetworkClient);
        }
    }
}
