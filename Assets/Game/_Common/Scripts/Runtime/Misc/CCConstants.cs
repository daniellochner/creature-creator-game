using System.IO;
using UnityEngine;

public static class CCConstants
{
    public static string CreaturesDir => Path.Combine(Application.persistentDataPath, "creature");
    public static string MapsDir => Path.Combine(Application.persistentDataPath, "maps");
    public static string BodyPartsDir => Path.Combine(Application.persistentDataPath, "bodyParts");
    public static string PatternsDir => Path.Combine(Application.persistentDataPath, "patterns");
    public static uint AppId => 1990050;
}
