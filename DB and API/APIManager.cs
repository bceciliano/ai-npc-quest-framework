using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;


//Classes should be serialized and deserialized from JSON
[Serializable]
public class ChatGPTResponse
{
    public Response[] choices;
}
[Serializable]
public class Response
{
    public ChatMessage message;
}
[Serializable]
public class ChatMessage
{
    public string content;
    public string role;
}
[Serializable]
public class ChatRequest
{
    public string model;
    public float temperature;
    public int max_tokens;
    public List<ChatMessage> messages;
}

public enum ChatRole
{
    user, system, assistant, function
}
public class APIManager : MonoBehaviour
{
    //Define parameters for API requests 
    private string apiKey;
    private string apiEndpoint = "https://api.openai.com/v1/chat/completions";
    private string model = "gpt-3.5-turbo";
    public float temperature = 0.7f;
    private int maxTokens = 100;
    
    //Define information for database retrieval on NPC Characteristics and playerMessage
    NPCData currentNPC;
    private DatabaseManager databaseManager;
    private NPCManager npcManager;
    private string userMessage;

    //Store the latest ChatGPT response 
    List<string> latestResponses;

    //Tracking response time 
    private DateTime startTime; 
    private DateTime endTime;
    private float gameStartTime;
    void Start()
    {
        // Load from OS environment variable
        apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("OPENAI_API_KEY is not set. Set it in your OS environment variables before running.");
        }

        databaseManager = GetComponent<DatabaseManager>();
        npcManager = GetComponent<NPCManager>();
        gameStartTime = Time.time;

        if (databaseManager == null)
        {
            Debug.LogError("DatabaseManager component not found.");
        }  
    }

    public List<string> GetLatestChatResponse()
    {
        return latestResponses;
    }

    public void ProcessPlayerMessage(int npcID, Action <List<string>> responseCallback)
    {
        startTime = DateTime.Now; //record the start time 
        currentNPC = databaseManager.GetNPCData(npcID);
        userMessage = databaseManager.GetLastPlayerMessage(npcID);

        StartCoroutine(SendChatRequest(responseCallback));
    }

    public string GetSystemPrompt()
    {
        if (currentNPC != null)
        {
            //Retrive NPC characteristics from the NPCManager
            List<NPCData> otherNPCs = npcManager.GetAllNPCData();

            //Retreive the available quests for the NPC
            List<QuestData> quests = databaseManager.GetQuestsForNPC(currentNPC.id);
            int availableQuestsCount = quests.Count;

            //Create lists to store quest-related data 
            List<string> questTitles = new();
            List<string> questDescriptions = new();
            List<string> loreDescriptions = new();

            //Add information about completed quests and rewards
            foreach(QuestData quest in quests)
            {
                //Check if the quest is completed
                bool isQuestCompleted = databaseManager.isQuestCompleted(quest.questID, currentNPC.id);
                if(isQuestCompleted)
                {
                    //Include quest data in the system prompt
                    questTitles.Add(quest.questName);
                    questDescriptions.Add(quest.questDescription);
                    availableQuestsCount--;

                    foreach(QuestReward reward in quest.rewards)
                    {
                        loreDescriptions.Add(reward.loreDescription);
                    }
                }
            }

            StringBuilder systemPromptBuilder = new();

            //Construct system prompt
            systemPromptBuilder.Append($"You are a(n) {currentNPC.species} named {currentNPC.npcName}. You are known for your {currentNPC.characteristics} attitude." +
                                          $"You find yourself in a world called Surf and Turf where people love to go on adventures in both sea and land");                                

            //Add information about other NPCs
            if(otherNPCs.Count > 0)
            {
                systemPromptBuilder.Append("You are aware of the following NPCs: ");
                foreach (var npc in otherNPCs)
                {
                    systemPromptBuilder.Append($" {npc.npcName}, a {npc.species} known for their {npc.characteristics} attitude. ");
                }

            }

            //Add information about completed quests 
            if(questTitles.Count > 0)
            {
                systemPromptBuilder.Append("The player has completed the following quests and have added lore about your life: ");
                for (int i =0; i < questTitles.Count; i++)
                {
                    systemPromptBuilder.Append($"{questTitles[i]}: {questDescriptions[i]}\n");
                    systemPromptBuilder.Append($"Lore: {loreDescriptions[i]}\n ");
                }
            }
            
            //Retrieve the entire dialogue history for the npc
            List<string> dialogueHistory = databaseManager.GetDialogueHistoryForNPC(currentNPC.id);
            if (dialogueHistory.Count > 0)
            {
                systemPromptBuilder.Append("Here is what you have said before: ");
                foreach (var dialogue in dialogueHistory)
                {
                    systemPromptBuilder.Append($"{dialogue}");
                }
            }
            if(availableQuestsCount > 0)
            {
                systemPromptBuilder.Append($" You have {availableQuestsCount} quest(s) available.");
            }

            //Add the last player messages, quest descriptions and dialogue history 
            systemPromptBuilder.Append($"Interact with the player , who is a crab, in your {currentNPC.characteristics} manner. ");

            systemPromptBuilder.Append($"Do not use any paragraph breaks in your message and say all you have to say in 4 sentences (your dialogue only)");
            return systemPromptBuilder.ToString();
        }
        else
        {
            Debug.LogError("NPC data not found");
            return null;
        }
    }
    private IEnumerator SendChatRequest(Action<List<string>> responseCallback)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("Cannot send chat request: OPENAI_API_KEY not set.");
            yield break;
        }

        // Construct the chat request object
        ChatRequest chatRequest = new ChatRequest
        {
            model = model,
            temperature = temperature,
            max_tokens = maxTokens,
            messages = new List<ChatMessage>
            {
                // Add the system message
                new ChatMessage
                {
                    role = ChatRole.system.ToString(),
                    content = GetSystemPrompt(),
                },

                // Add the user message
                new ChatMessage
                {
                    role = ChatRole.user.ToString(),
                    content = userMessage,
                }
            }
        };

        // Serialize the request object
        string requestJson = JsonConvert.SerializeObject(chatRequest);

        //Create a UnityWebRequest for the API request 
        using (UnityWebRequest request = new UnityWebRequest(apiEndpoint, "POST"))
        {
            //Set request headers
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            request.SetRequestHeader("Content-Type", "application/json");

            //Set request data 
            var requestBodyRaw = Encoding.UTF8.GetBytes(requestJson);
            request.uploadHandler = new UploadHandlerRaw(requestBodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            //Send the request 
            yield return request.SendWebRequest();
            
            //Record the end time when the response is received
            endTime = DateTime.Now;

            //Check for errors
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("API Request Error: " + request.error);
            }
            else
            {
                //Parse and log the response 
                string responseJson = request.downloadHandler.text;

                //Deserialize the JSON response into an object 
                ChatGPTResponse chatResponse = JsonUtility.FromJson<ChatGPTResponse>(responseJson);

                //Get the chat message content from the response 
                string fullResponse = chatResponse.choices[0].message.content;

                //Define the maximum tokens per chunk
                int maxTokensPerChunk = 25;

                //Split the response into different chunks 
                latestResponses = SplitResponse(fullResponse, maxTokensPerChunk);

                //Call the callback with the response
                responseCallback?.Invoke(latestResponses);
            }
        }
        TimeSpan responseTime = endTime - startTime;
        string responseTimeFormatted = responseTime.ToString(@"hh\:mm\:ss\:fff");
        Debug.Log("Game Time Since Start: " + gameStartTime);
        Debug.Log("Response Time: " + responseTimeFormatted);
    }
    private List<string> SplitResponse(string fullResponse, int maxTokensPerChunk)
    {
        List<string> chunks = new List<string>();
        string[] sentences = fullResponse.Split(new[] {'.', '!', '?'}, StringSplitOptions.RemoveEmptyEntries);
        string currentChunk = "";

        foreach (string sentence in sentences)
        {
            //Check if adding this sentence exceeds the maximum token count for the current chunk
            if (CountTokens(currentChunk + sentence) > maxTokensPerChunk)
            {
                //Start a new chunk
                chunks.Add(currentChunk.Trim()); //Trim any trailing sentences
                currentChunk = "";
            }

            //add the sentence to the current chunk
            currentChunk += sentence + ".";
        }

        //Add the last chunk
        chunks.Add(currentChunk.Trim());

        //Insert the completed chunks into the database
        foreach(string chunk in chunks)
        {
            databaseManager.InsertDialogueHistory(currentNPC.id, chunk);
        }
        return chunks;
    }

    //Pseudo Tokenizer - Future Project to implement tiktoken
    private int CountTokens(string text)
    {
        //Split the text into words by spaces 
        string[] words = text.Split(' ');

        //Count the words that are not special tokens
        int tokenCount = words.Count();

        return tokenCount;
    }
}
