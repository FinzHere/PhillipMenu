using NAudio.Wave;
using System;
using UnityEngine;

public static class VirtualMicPlayer
{
    // 1. Move these to class level so the whole script can access them
    private static WaveOutEvent activeOutputDevice;
    private static AudioFileReader activeAudioFile;

    public static void PlaySound(string filePath)
    {
        // 2. Cut off any currently playing sound before starting a new one
        // This stops sounds from overlapping and deep-frying your virtual mic!
        StopAll();

        int cableDeviceId = -1;

        // Find the VB-Audio Cable
        for (int i = 0; i < WaveOut.DeviceCount; i++)
        {
            var capabilities = WaveOut.GetCapabilities(i);
            if (capabilities.ProductName.Contains("CABLE Input"))
            {
                cableDeviceId = i;
                break;
            }
        }

        if (cableDeviceId == -1)
        {
            Debug.LogError("VirtualMicPlayer: VB-Audio Virtual Cable not found! Please install it.");
            return;
        }

        try
        {
            // 3. Assign our class-level static fields
            activeOutputDevice = new WaveOutEvent { DeviceNumber = cableDeviceId };
            activeAudioFile = new AudioFileReader(filePath);

            // Set your sweet-spot volume here! (e.g., 0.5f)
            activeAudioFile.Volume = 0.5f;

            activeOutputDevice.Init(activeAudioFile);
            activeOutputDevice.Play();

            // 4. Asynchronous Safety: Capture local copies for the natural ending loop
            // This prevents a naturally finishing sound from accidentally wiping out a newly started sound.
            var deviceToClean = activeOutputDevice;
            var fileToClean = activeAudioFile;

            deviceToClean.PlaybackStopped += (sender, args) =>
            {
                if (deviceToClean != null) deviceToClean.Dispose();
                if (fileToClean != null) fileToClean.Dispose();
                Debug.Log("Audio track finished naturally and cleaned up.");
            };
        }
        catch (Exception ex)
        {
            Debug.LogError("VirtualMicPlayer Error playing sound: " + ex.Message);
        }
    }

    // 5. Your brand new Stop All method!
    public static void StopAll()
    {
        try
        {
            // Stop the hardware device and immediately release the audio lock
            if (activeOutputDevice != null)
            {
                activeOutputDevice.Stop();
                activeOutputDevice.Dispose();
                activeOutputDevice = null;
            }

            // Close the actual audio file stream
            if (activeAudioFile != null)
            {
                activeAudioFile.Dispose();
                activeAudioFile = null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("VirtualMicPlayer Error manually stopping sound: " + ex.Message);
        }
    }
}