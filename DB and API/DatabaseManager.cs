using UnityEngine;
using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class DatabaseManager : MonoBehaviour
{
    private readonly string dbName = "URI=file:NPChat.db";

    void Start()
    {
        //Create a table
        CreateDB();
    }

    public void CreateDB()
    {
        //Create the database connection
        using var connection = new SqliteConnection(dbName);
        connection.Open();

        //Set up an object (called "command") to allow db Control 
        using (var command = connection.CreateCommand())
        {
            // Create a table for NPC Characteristics 
            command.CommandText = "CREATE TABLE IF NOT EXISTS NPCharacteristics (id INT PRIMARY KEY, name TEXT, species TEXT, characteristics TEXT );";
            command.ExecuteNonQuery();

            // Create a table for dialogue history with column for player responses
            command.CommandText = "CREATE TABLE IF NOT EXISTS dialogueHistory (npcID INT, dialogue TEXT, playerMessage TEXT);";
            command.ExecuteNonQuery();

            // Create a table for quests information 
            command.CommandText = "CREATE TABLE IF NOT EXISTS Quests (questID INT PRIMARY KEY, npcID INT, questName TEXT, questDescription TEXT, isCompleted INT, objectives TEXT, rewards TEXT);";
            command.ExecuteNonQuery();
        }
        connection.Close();
    }

    //Call this function to insert NPC 
    public void InsertNPC(int id, string name, string species, string characteristics)
    {
        using var connection = new SqliteConnection(dbName);
        connection.Open();

        using (var command = connection.CreateCommand())
        {
            // Check if the NPC with the same ID exists before inserting
            command.CommandText = "SELECT COUNT(*) FROM NPCharacteristics WHERE id = @id;";
            command.Parameters.AddWithValue("@id", id);

            int existingCount = Convert.ToInt32(command.ExecuteScalar());

            if (existingCount == 0)
            {
                // NPC with the same ID doesn't exist, so insert it
                command.CommandText = "INSERT INTO NPCharacteristics (id, name, species, characteristics) VALUES (@id, @name, @species, @characteristics);";
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@species", species);
                command.Parameters.AddWithValue("@characteristics", characteristics);
                command.ExecuteNonQuery();

            }
        }
        connection.Close();
    }

    //Call this function to insert dialogue history 
    public void InsertDialogueHistory(int npcID, string dialogue)
    {
        using var connection = new SqliteConnection(dbName);
        connection.Open();

        using (var command = connection.CreateCommand())
        {
            // Check if dialogue history with the same NPC ID and dialogue exists before inserting
            command.CommandText = "SELECT COUNT(*) FROM dialogueHistory WHERE npcID = @npcID AND dialogue = @dialogue;";
            command.Parameters.AddWithValue("@npcID", npcID);
            command.Parameters.AddWithValue("@dialogue", dialogue);

            int existingCount = Convert.ToInt32(command.ExecuteScalar());

            if (existingCount == 0)
            {
                // Dialogue history with the same NPC ID and dialogue doesn't exist, so insert it
                command.CommandText = "INSERT INTO dialogueHistory (npcID, dialogue) VALUES (@npcID, @dialogue);";
                command.Parameters.AddWithValue("@npcID", npcID);
                command.Parameters.AddWithValue("@dialogue", dialogue);
                command.ExecuteNonQuery();
            }
        }
        connection.Close();
    }

    // Call this function to insert a new quest in the database 
    public void InsertQuest(int questID, int npcID, string questName, string questDescription,bool isCompleted, List<QuestObjective> objectives, List<QuestReward> rewards)
    {
        using var connection = new SqliteConnection(dbName);
        connection.Open();
        using (var command = connection.CreateCommand())
        {
            // Check if quest with the same quest ID exists before inserting
            command.CommandText = "SELECT COUNT(*) FROM Quests WHERE questID = @questID;";
            command.Parameters.AddWithValue("@questID", questID);

            int existingCount = Convert.ToInt32(command.ExecuteScalar());

            if (existingCount == 0)
            {
                //Serialize objective and rewards to Json String
                string objectiveJson = JsonConvert.SerializeObject(objectives);
                string rewardsJson = JsonConvert.SerializeObject(rewards);

                //Insert the quest data into the quests table with associated NPCID
                command.CommandText = "INSERT INTO Quests (questID, npcID, questName, questDescription, isCompleted, objectives, rewards) VALUES (@questID, @npcID, @questName, @questDescription, @isCompleted, @objectives, @rewards);";
                command.Parameters.AddWithValue("@questID", questID);
                command.Parameters.AddWithValue("@npcID", npcID);
                command.Parameters.AddWithValue("@questName", questName);
                command.Parameters.AddWithValue("@questDescription", questDescription);
                command.Parameters.AddWithValue("@isCompleted", isCompleted);
                command.Parameters.AddWithValue("@objectives", objectiveJson);
                command.Parameters.AddWithValue("@rewards", rewardsJson);
                command.ExecuteNonQuery();

            }
        }
        connection.Close();
    }

    public void SavePlayerMsg(int npcID, string  msg)
    {
        using var connection = new SqliteConnection(dbName);
        connection.Open();
        using (var command = connection.CreateCommand())
        {
            // Insert the player's message into the database, associating it with the NPC 
            command.CommandText = "INSERT INTO dialogueHistory (npcID, dialogue, playerMessage) VALUES (@npcID, @dialogue, @playerMessage);";
            command.Parameters.AddWithValue("@npcID", npcID);
            command.Parameters.AddWithValue("@dialogue", "");
            command.Parameters.AddWithValue("@playerMessage", msg);
            command.ExecuteNonQuery();
        }
        connection.Close();
    }

    public string GetLastPlayerMessage(int npcID)
    {
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                // Retrieve the last player message and npcID from the dialogue history table 
                command.CommandText = "SELECT playerMessage FROM dialogueHistory WHERE npcID = @npcID ORDER BY ROWID DESC LIMIT 1;";
                command.Parameters.AddWithValue("@npcID", npcID);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    string playerMessage = reader.GetString(0);
                    return playerMessage;
                }
            }
            connection.Close();
        }
        return (string.Empty);
    }

    public NPCData GetNPCData(int npcID)
    {
        using var connection = new SqliteConnection(dbName);
        connection.Open();
        using (var command = connection.CreateCommand())
        {
            // Select the NPC data based on the NPCID 
            command.CommandText = "SELECT id, name, species, characteristics FROM NPCharacteristics WHERE id = @npcID;";
            command.Parameters.AddWithValue("@npcID", npcID);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                //Read the data from the database and create an NPCData object 
                int id = reader.GetInt32(0);
                string name = reader.GetString(1);
                string species = reader.GetString(2);
                string characteristics = reader.GetString(3);

                NPCData npcData = ScriptableObject.CreateInstance<NPCData>();
                npcData.id = id;
                npcData.npcName = name;
                npcData.species = species;
                npcData.characteristics = characteristics;
                return npcData;
            }
        }
        connection.Close();
        return null;
    }
    public List<NPCData> GetNPCDataExcept(int npcID)
    {
        List<NPCData> otherNPCs = new();
        using var connection = new SqliteConnection(dbName);
        connection.Open();
        using (var command = connection.CreateCommand())
        {
            // Select the NPC data based on the NPCID 
            command.CommandText = "SELECT id, name, species, characteristics FROM NPCharacteristics WHERE id != @npcID;";
            command.Parameters.AddWithValue("@npcID", npcID);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                //Read the data from the database and create an NPCData object 
                int id = reader.GetInt32(0);
                string name = reader.GetString(1);
                string species = reader.GetString(2);
                string characteristics = reader.GetString(3);

                NPCData npcData = ScriptableObject.CreateInstance<NPCData>();
                npcData.id = id;
                npcData.npcName = name;
                npcData.species = species;
                npcData.characteristics = characteristics;
                otherNPCs.Add(npcData);
            }
        }
        connection.Close();
        return otherNPCs;
    }

    public List<string> GetDialogueHistoryForNPC(int npcID)
    {
        List<string> dialogueHistory = new();
        using var connection = new SqliteConnection(dbName);
        connection.Open();
        using (var command = connection.CreateCommand())
        {
            //Retreive all dialogue entries for the npc
            command.CommandText = "SELECT dialogue FROM dialogueHistory WHERE npcID = @npcID;";
            command.Parameters.AddWithValue("@npcID", npcID);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                string dialogue = reader.GetString(0);
                dialogueHistory.Add(dialogue);
            }
        }
        connection.Close();
        return dialogueHistory;
    }

    public List<string> GetPlayerResponsesForNPC(int npcID)
    {
        List<string> playerMessages = new();
        using var connection = new SqliteConnection(dbName);
        connection.Open();
        using (var command = connection.CreateCommand())
        {
            //Retreive all dialogue entries for the npc
            command.CommandText = "SELECT playerMessage FROM dialogueHistory WHERE npcID = @npcID;";
            command.Parameters.AddWithValue("@npcID", npcID);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                string dialogue = reader.GetString(0);
                playerMessages.Add(dialogue);
            }
        }
        connection.Close();
        return playerMessages;
    }

    public List<QuestData> GetQuestsForNPC(int npcID)
    {
        List<QuestData> availableQuests = new();
        using (var connection = new SqliteConnection(dbName))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                // Select the quest data based on the NPCID 
                command.CommandText = "SELECT * FROM Quests WHERE npcID = @npcID;";
                command.Parameters.AddWithValue("@npcID", npcID);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    //Create and populate QuestData objects for each quest
                    int questID = reader.GetInt32(0);
                    string questName = reader.GetString(2);
                    string questDescription = reader.GetString(3);
                    bool isCompleted = reader.GetInt32(4) != 0; // 0 - false, 1 - true
                    string objectivesJson = reader.GetString(5);
                    string rewardsJson = reader.GetString(6);

                    //Deserialize objective and rewards
                    List<QuestObjective> objectives = JsonConvert.DeserializeObject<List<QuestObjective>>(objectivesJson);
                    List<QuestReward> rewards = JsonConvert.DeserializeObject<List<QuestReward>>(rewardsJson);

                    //Create a new QuestData object and initialize it with the above information 
                    QuestData questData = ScriptableObject.CreateInstance<QuestData>();
                    questData.questID = questID;
                    questData.questName = questName;
                    questData.questDescription = questDescription;
                    questData.isCompleted = isCompleted;
                    questData.objectives = objectives;
                    questData.rewards = rewards;
                    availableQuests.Add(questData);
                }
            }
            connection.Close();
        }
        return availableQuests;
    }
    public bool isQuestCompleted(int questID, int npcID)
    {
        using var connection = new SqliteConnection(dbName);
        connection.Open();
        using (var command = connection.CreateCommand())
        {
            //Update the quests isCompleted field to true 
            command.CommandText = "SELECT isCompleted FROM Quests WHERE questID = @questID AND npcID = @npcID;";
            command.Parameters.AddWithValue("@questID", questID);
            command.Parameters.AddWithValue("@npcID", npcID);
            
            using var reader = command.ExecuteReader();
            if(reader.Read())
            {
                //Read the isCompleted value from the database where 0 is false and 1 is true
                bool isCompleted = reader.GetInt32(0) != 0;
                return isCompleted;
            }
        }
        connection.Close();
        return false;
    }
    public void SetQuestCompleted(int questID, int npcID)
    {
        using var connection = new SqliteConnection(dbName);
        connection.Open();
        using (var command = connection.CreateCommand())
        {
            //Update the quests isCompleted field to true 
            command.CommandText = "UPDATE Quests SET isCompleted = 1 WHERE questID = @questID AND npcID = @npcID;";
            command.Parameters.AddWithValue("@questID", questID);
            command.Parameters.AddWithValue("@npcID", npcID);
            command.ExecuteNonQuery();
        }
        connection.Close();
    }
    public void ClearDatabase()
    {
        using var connection = new SqliteConnection(dbName);
        connection.Open();
        using (var command = connection.CreateCommand())
        {
            //Drop the quests table if it exists
            command.CommandText = "DELETE FROM Quests;";
            command.ExecuteNonQuery();

            //Drop the dialogueHistory table if it exists
            command.CommandText = "DELETE FROM dialogueHistory;";
            command.ExecuteNonQuery();
            
            //Drop the NPCharacteristics table if it exists
            command.CommandText = "DELETE FROM NPCharacteristics;";
            command.ExecuteNonQuery();
        }
        connection.Close();
    }
}
