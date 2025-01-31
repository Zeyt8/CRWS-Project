using UnityEngine;
using TMPro;
using UnityEngine.Profiling;
using System.Collections.Generic;

public class Performance : MonoBehaviour
{
    public TextMeshProUGUI statsText;
    private Queue<float> frameTimes = new Queue<float>();
    private Queue<float> memorySamples = new Queue<float>();
    private Queue<float> vramSamples = new Queue<float>();
    private float totalFrameTime = 0.0f;
    private float totalMemory = 0.0f;
    private float totalVRAM = 0.0f;
    private float maxFrameTime = 0.0f;
    private float maxRAM = 0.0f;
    private float maxVRAM = 0.0f;
    private float updateInterval = 1f; // Update stats every second

    void Update()
    {
        // Track frame time
        float currentFrameTime = Time.deltaTime * 1000.0f; // Convert to milliseconds
        frameTimes.Enqueue(currentFrameTime);
        totalFrameTime += currentFrameTime;

        if (currentFrameTime > maxFrameTime)
        {
            maxFrameTime = currentFrameTime;
        }

        // Track memory usage
        long allocatedMemory = Profiler.GetTotalReservedMemoryLong();
        float convertedMemory = allocatedMemory / (1024f * 1024f);
        memorySamples.Enqueue(convertedMemory);
        totalMemory += convertedMemory;

        if (convertedMemory > maxRAM)
        {
            maxRAM = convertedMemory;
        }

        // Track VRAM usage
        long allocatedVRAM = Profiler.GetAllocatedMemoryForGraphicsDriver();
        float convertedVRAM = allocatedVRAM / (1024f * 1024f); // Convert to MB
        vramSamples.Enqueue(convertedVRAM);
        totalVRAM += convertedVRAM;

        if (convertedVRAM > maxVRAM)
        {
            maxVRAM = convertedVRAM;
        }

        // Remove old samples
        while (frameTimes.Count > 0 && frameTimes.Count > 60 / Time.deltaTime)
        {
            float oldFrameTime = frameTimes.Dequeue();
            totalFrameTime -= oldFrameTime;
            if (oldFrameTime == maxFrameTime)
            {
                maxFrameTime = Mathf.Max(frameTimes.ToArray());
            }
        }

        while (memorySamples.Count > 0 && memorySamples.Count > 60 / Time.deltaTime)
        {
            float oldMemory = memorySamples.Dequeue();
            totalMemory -= oldMemory;
            if (oldMemory == maxRAM)
            {
                maxRAM = Mathf.Max(memorySamples.ToArray());
            }
        }

        while (vramSamples.Count > 0 && vramSamples.Count > 60 / Time.deltaTime)
        {
            float oldVRAM = vramSamples.Dequeue();
            totalVRAM -= oldVRAM;
            if (oldVRAM == maxVRAM)
            {
                maxVRAM = Mathf.Max(vramSamples.ToArray());
            }
        }

        // Update stats text every second
        if (Time.time % updateInterval < Time.deltaTime)
        {
            float averageFPS = frameTimes.Count / (totalFrameTime / 1000.0f);
            float averageFrameTime = totalFrameTime / frameTimes.Count;
            float averageRAM = totalMemory / memorySamples.Count;
            float averageVRAM = totalVRAM / vramSamples.Count;

            UpdateStatsText(averageFPS, averageRAM, averageVRAM, averageFrameTime, maxFrameTime, 0, 0);
        }
    }

    private void UpdateStatsText(float averageFPS, float averageRAM, float maxRAM, float averageFrameTime, float maxFrameTime, float averageVRAM, float maxVRAM)
    {
        if (statsText != null)
        {
            statsText.text = $"Average FPS (last minute): {averageFPS:F1}\n" +
                             $"Avg. Frame Time (last minute): {averageFrameTime:F1} ms\n" +
                             $"Max Frame Time (last minute): {maxFrameTime:F1} ms\n";
        }
    }

    public void ToggleStats()
    {
        if (statsText != null)
        {
            statsText.enabled = !statsText.enabled;
        }
    }
}
