using UnityEngine;
using UnityEngine.UI;

public class SelectNamePanel : MonoBehaviour
{
    [SerializeField] private InputField inputName;
    [SerializeField] private GameObject confirmButton;

    private void Start()
    {
        confirmButton.SetActive(false);
    }

    // Save name
    public void SelectName()
    {
        FindObjectOfType<MainMenuManager>().theData.UserName = inputName.text;
        SaveGame.SaveProgress(FindObjectOfType<MainMenuManager>().theData);
        gameObject.SetActive(false);

        // Tell server
        Client client = FindObjectOfType<Client>();
        string[] massage = new string[] { "ChangeName", inputName.text };
        client.SendMassageClient("Server", massage);
    }

    // Check name character size
    public void MaxMinName()
    {
        if(inputName.text.Length < 3 || inputName.text.Length > 10)
        {
            confirmButton.SetActive(false);
        }
        else
        {
            confirmButton.SetActive(true);
        }
    }
}
