using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveGame : MonoBehaviour
{
    // For saving game
    public static void SaveProgress(SaveData data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/wkwk.txt";
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, data);
        stream.Close();

        // Debug.Log("File Saved");
    }

    // For load game
    public static SaveData LoadData()
    {
        string path = Application.persistentDataPath + "/wkwk.txt";

        // Debugging
        // Debug.Log(Application.persistentDataPath + "/adventure.txt");


        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            SaveData data = formatter.Deserialize(stream) as SaveData;
            stream.Close();

            // Debug.Log("File Loaded");

            return data;
        }
        else
        {
            Debug.Log("File Not Found");
            SaveData temp = new SaveData();
            return temp;
        }
    }
}
