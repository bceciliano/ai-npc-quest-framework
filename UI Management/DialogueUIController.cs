using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueUIController : MonoBehaviour
{
    [Header("UI Elements and SFX")]
    public TMP_Text panelText;
    public TMP_Text pendingAnimation;
    public AudioSource dialogueAudioSource;

    private UIManager uiManager;

    //Typewriter functionality
    private float typewriteSpeed = 0.05f;
    private Coroutine typingCoroutine;
    private bool spacePressed = false;
    private bool spacePressedTwice = false;
    private float spaceSpeed = 2.0f;

    //Dialogue lines functionality
    private List<string> dialogueLines = new List<string>();
    private int currentLineIndex = 0;

    //API Pending Animation
    private bool isAnimating = false;

    private void Start()
    {
        uiManager = GetComponent<UIManager>();
    }
    void Update()
    {
        if (typingCoroutine != null)
        {
            //The NPC is typing, handle space and escape presses
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                typewriteSpeed /= spaceSpeed;

                if (!spacePressed)
                {
                    spacePressed = true;
                }
                else
                {
                    spacePressedTwice = true;
                }
            }
        }
    }

    //Set the text for dialogue - used for introduction lines
    public void SetPanelText(List<string> lines)
    {
        dialogueAudioSource.Play();
        if (panelText != null)
        {
            dialogueLines.Clear();
            dialogueLines.AddRange(lines);
            panelText.text = "";

            //Start displaying the first line
            currentLineIndex = 0;
            DisplayNextLine();
        }
    }

    private void DisplayNextLine()
    {
        if (currentLineIndex < dialogueLines.Count)
        {
            typewriteSpeed = 0.05f; //Reset to default speed
            typingCoroutine = StartCoroutine(TypeText(dialogueLines[currentLineIndex]));
            currentLineIndex++;
        }
        else
        {
            //Have the panel show the last line
            panelText.text = dialogueLines[currentLineIndex - 1];
            uiManager.EnableInputField();
        }
    }

    //Coroutine to display text one character at a time
    private IEnumerator TypeText(string text)
    {
        //Disable the input field
        uiManager.DisableInputField();

        foreach (char letter in text)
        {
            panelText.text += letter; //add the next letter
            yield return new WaitForSeconds(typewriteSpeed);
        }

        //Displayed the line now wait for space
        while (true)
        {
            if (spacePressedTwice)
            {
                CompleteTyping();
                spacePressedTwice = false;
                DisplayNextLine();
                yield break;
            }
            yield return null; //wait for player to press space
        }
    }

    public void CompleteTyping()
    {
        //Complete effect immediately
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        panelText.text = "";
        dialogueAudioSource.Stop();
    }

    public void SetChatMessage(APIManager apiManager, int currentNPCID)
    {
        StartPendingAnimation();

        //Call ProcessPlayerMessage and pass a callback to handle the response
        apiManager.ProcessPlayerMessage(currentNPCID, (response) =>
        {
            typewriteSpeed = 0.05f; //Reset to default speed
            SetPanelText(response);

            //Stop the pending animation
            StopPendingAnimation();
        });
    }
    public void StartPendingAnimation()
    {
        if (pendingAnimation != null && !isAnimating)
        {
            isAnimating = true;
            StartCoroutine(PendingAnimation());
        }
    }

    public void StopPendingAnimation()
    {
        if (pendingAnimation != null && isAnimating)
        {
            isAnimating = false;
            pendingAnimation.text = "";
            StopCoroutine("PendingAnimation");
        }
    }
    private IEnumerator PendingAnimation()
    {
        while (isAnimating)
        {
            pendingAnimation.text = ".";
            yield return new WaitForSeconds(0.5f);
            pendingAnimation.text = "..";
            yield return new WaitForSeconds(0.5f);
            pendingAnimation.text = "...";
            yield return new WaitForSeconds(0.5f);

        }
    }
}
