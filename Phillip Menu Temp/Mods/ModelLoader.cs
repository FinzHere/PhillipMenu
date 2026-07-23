using System;
using System.IO;
using System.Reflection;
using UnityEngine;

public static class ModelLoader
{
    // Tracks the currently spawned item so we can clean it up when swapping items
    public static GameObject currentSpawnedModel;

    /// <summary>
    /// Spawns any embedded asset bundle model and parents it to a designated hand/transform.
    /// </summary>
    /// <param name="bundleFileName">The exact name of your embedded asset bundle file (e.g. "sword1")</param>
    /// <param name="prefabName">The name of the prefab inside that bundle (e.g. "sword1textured")</param>
    /// <param name="targetParent">Optional: What to attach it to. Defaults to Right Hand if left null.</param>
    /// <param name="customScale">Optional: Set a custom scale if 0.1f is too small/large.</param>
    public static void SpawnModel(string bundleFileName, string prefabName, Transform targetParent = null, Vector3? customScale = null)
    {
        if (currentSpawnedModel != null) return;

        // 1. Automatically delete whatever item is already in your hand
        DespawnModel();

        // 2. Default to the Right Hand if no specific target parent is passed
        if (targetParent == null)
        {
            if (GorillaTagger.Instance != null && GorillaTagger.Instance.offlineVRRig != null)
            {
                targetParent = GorillaTagger.Instance.offlineVRRig.rightHandTransform;
            }
            else
            {
                Debug.LogError("ModelLoader: Could not find GorillaTagger right hand!");
                return;
            }
        }

        try
        {
            // 3. Open the current assembly stream
            Assembly assembly = Assembly.GetExecutingAssembly();

            // 4. Dynamically build your resource path string using the parameter
            string resourceName = $"Phillip_Menu_Temp.Resources.{bundleFileName}";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    Debug.LogError($"ModelLoader: Failed to find embedded asset bundle '{resourceName}'. Check your filename capitalization and Build Action!");
                    return;
                }

                // 5. Load the bundle from memory
                AssetBundle bundle = AssetBundle.LoadFromStream(stream);
                if (bundle == null)
                {
                    Debug.LogError($"ModelLoader: AssetBundle failed to initialize from stream for '{bundleFileName}'");
                    return;
                }

                // 6. Extract the specific prefab by parameter name
                GameObject prefab = bundle.LoadAsset<GameObject>(prefabName);
                if (prefab == null)
                {
                    Debug.LogError($"ModelLoader: Prefab '{prefabName}' was not found inside bundle '{bundleFileName}'!");
                    bundle.Unload(false);
                    return;
                }

                // 7. Instantiate and snap it to the hand
                currentSpawnedModel = GameObject.Instantiate(prefab);
                currentSpawnedModel.transform.SetParent(targetParent);

                // 8. Position resetting
                currentSpawnedModel.transform.localPosition = new Vector3(0.3f, -0.15f, 0.11f);
                currentSpawnedModel.transform.localRotation = Quaternion.identity;

                // Use custom scale if provided, otherwise fallback to your default 0.1f
                currentSpawnedModel.transform.localScale = customScale ?? new Vector3(0.3f, 0.3f, 0.3f);

                // 9. Free unmanaged memory
                bundle.Unload(false);
                Debug.Log($"ModelLoader: Successfully spawned {prefabName} on cosmetic bone target.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"ModelLoader Critical Error spawning '{prefabName}': " + ex.Message);
        }
    }

    /// <summary>
    /// Safely destroys whatever model is currently active.
    /// </summary>
    public static void DespawnModel()
    {
        if (currentSpawnedModel != null)
        {
            GameObject.Destroy(currentSpawnedModel);
            currentSpawnedModel = null;
        }
    }
}