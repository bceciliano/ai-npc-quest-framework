using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    [SerializeField] private List<NPCData> allNPCs = new();

    public List<NPCData> GetAllNPCData()
    {
        return allNPCs;
    }

}
