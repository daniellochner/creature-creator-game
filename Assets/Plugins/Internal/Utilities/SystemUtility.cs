using System.IO;
using UnityEngine;

namespace DanielLochner.Assets
{
    public static class SystemUtility
    {
        private static readonly int LOW_END_MEMORY_THRESHOLD = 512; // MB

        public static DeviceType DeviceType
        {
            get
            {
#if UNITY_IOS || UNITY_ANDROID
                return DeviceType.Handheld;
#elif UNITY_EDITOR
                if (UnityEditor.EditorApplication.isRemoteConnected)
                {
                    return DeviceType.Handheld;
                }
#else
                return SystemInfo.deviceType;
#endif
            }
        }
        public static bool IsDevice(DeviceType type)
        {
            return type == DeviceType;
        }

        public static bool IsLowEndDevice
        {
            get
            {
                return SystemInfo.systemMemorySize < LOW_END_MEMORY_THRESHOLD;
            }
        }

        public static void TryCreateDirectory(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        public static void CopyFile(string srcFile, string dstFile)
        {
            if (!File.Exists(srcFile))
            {
                throw new FileNotFoundException($"Source file not found: {srcFile}");
            }

            if (!File.Exists(dstFile))
            {
                File.Copy(srcFile, dstFile);
            }
        }

        public static void CopyDirectory(string srcDir, string dstDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(srcDir);

            // Check if the source directory exists
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
            }

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(dstDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(dstDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(dstDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }
    }
}