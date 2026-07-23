using System.IO;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;

public static class SoundManager
{
    public static string soundsFolderPath;
    public static List<string> soundFilePaths = new List<string>();
    public static List<string> soundFileNames = new List<string>(); // Just the names for the buttons

    public static void Initialize()
    {
        // 1. Define the folder path
        string pluginsDirectory = Paths.PluginPath;
        string myModFolder = Path.Combine(pluginsDirectory, "PhillipMenu");
        soundsFolderPath = Path.Combine(myModFolder, "Sounds");

        // 2. Create folders if they don't exist
        if (!Directory.Exists(myModFolder)) Directory.CreateDirectory(myModFolder);
        if (!Directory.Exists(soundsFolderPath)) Directory.CreateDirectory(soundsFolderPath);

        // 3. Load the sounds
        LoadAvailableSounds();
    }

    public static void LoadAvailableSounds()
    {
        soundFilePaths.Clear();
        soundFileNames.Clear();

        // Get all mp3 and wav files in the folder
        string[] files = Directory.GetFiles(soundsFolderPath);

        foreach (string file in files)
        {
            if (file.EndsWith(".mp3") || file.EndsWith(".wav"))
            {
                soundFilePaths.Add(file);

                // Get just the name without the extension (e.g., "Fart" instead of "Fart.mp3")
                soundFileNames.Add(Path.GetFileNameWithoutExtension(file));
            }
        }

        Debug.Log($"Loaded {soundFilePaths.Count} sounds for the soundboard!");
    }
}