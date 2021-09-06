using UnityEngine;

// Contain user data

[System.Serializable]
public class SaveData
{
    // User name
    public string UserName { get; set; }

    // Constructor
    public SaveData()
    {
        UserName = "null";
    }
}
