using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SerializableFavorability
{
    public string npcRole;
    public int favorability;
}

[System.Serializable]
public class SaveData
{
    public int taskNumber;
    public Vector3 playerPosition;
    public List<Letter> letters;
    public string taskStateJson;
    public bool isIndoors;
    public int currentHouseIndex;
    public Vector3 lastPlayerMapPosition;
    public List<SerializableFavorability> npcFavorabilityList; // 好感度数据
}