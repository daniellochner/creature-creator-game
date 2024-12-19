using System.Collections;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public static class SDKCopier
{
	[MenuItem("Tools/Creature Creator/Copy SDK Files")]
	public static void CopyFiles()
	{
		string sourcePath = Path.Combine(Application.dataPath, "Creature Creator SDK");
		string destinationPath = "D:\\Development\\Unity\\Workspace\\creature-creator-sdk\\Assets\\Creature Creator SDK"; //EditorUtility.OpenFolderPanel("Find Creature Creator SDK", Application.dataPath, "Creature Creator SDK");

        if (string.IsNullOrEmpty(destinationPath))
        {
            return;
        }

        Copy(sourcePath, destinationPath);
	}

    private static void Copy(string sourceDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);

        foreach (var file in Directory.GetFiles(sourceDir))
            File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);

        foreach (var directory in Directory.GetDirectories(sourceDir))
            Copy(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
    }
}
