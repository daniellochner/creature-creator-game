using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapConfigData
{
    public string SDKVersion;
    public string ItemId;

    public string Name;
    public string Description;
    public string Author;
    public List<string> BodyPartIds;
    public List<string> PatternIds;
}