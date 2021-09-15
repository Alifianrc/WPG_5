using UnityEngine;

// Contain user data

[System.Serializable]
public class SaveData
{
    // User name
    public string UserName;
    // User coin
    public long Coin;
    // User unlocked skin
    public bool[] SkinIsUnlock = new bool[5];

    // Constructor
    public SaveData()
    {
        UserName = "null";
        Coin = 0;
    }
}
