using UnityEngine;
using TMPro;

public class PlayerInputManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private DatabaseManager databaseManager;
    [SerializeField] private APIManager apiManager;
    [SerializeField] private UIManager uiManager;

    public static bool isTyping;
    private int currentNPCID;
    private NPCController currentNPCController;

    void Update()
    {
        UpdateTypingStatus();

        //Check if the player pressed Enter in the InputField and confirm the input is valid 
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && isInputValid(inputField.text))
        {
            ProcessPlayerInput();
        }
        else if (!isInputValid(inputField.text))
        {
            HandleInvalidInput();
        }
    }

    private void UpdateTypingStatus()
    {
        isTyping = inputField.isFocused;
    }

    private void ProcessPlayerInput()
    {
        //Retrieve player msg for input field
        string playerMsg = inputField.text;

        //Retrieve the current NPCID
        currentNPCID = currentNPCController.npcData.id;

        if (currentNPCID != -1)
        {
            //Save player message and pass the callback 
            databaseManager.SavePlayerMsg(currentNPCID, playerMsg);

            //Call the SetChat to handle the rest
            uiManager.SetChatMessage();

            //Clear inputfield 
            inputField.text = "";
        }
    }
    private void HandleInvalidInput()
    {
        //Input is invalid, show the message 
        inputField.text = "";
        inputField.Select();

        if (inputField.isFocused)
        {
            //Reset the input field if its focused 
            inputField.text = "";
        }
    }
    private bool isInputValid(string inputTxt)
    {
        //Trim the input to remove leading and trailing spaces 
        string trimmedInputTxt = inputTxt.Trim();

        //Check if the trimmed text is empty
        if (string.IsNullOrEmpty(trimmedInputTxt))
        {
            //Invalid Input 
            return false;
        }
        return true;
    }

    public void SetNPCController(NPCController npcData)
    {
        currentNPCController = npcData;
    }
}
