using UnityEngine;
using TMPro;
using UnityEngine.Profiling;

public class Performance : MonoBehaviour
{
    public TextMeshProUGUI statsText;
    private float totalFPS = 0.0f;
    private int frameCount = 0;
    private float averageRam = 0.0f;
    private float timeElapsed = 0.0f;
    private float updateInterval = 10f;
    private int ramSampleCount = 0;
    private float totalRAM;
    private float maxRAM = 0;
    private float MPF = 0;
    private float maxMPF = 0;
    private float VRAM = 0;
    private float maxVRAM = 0;

    void Update()
    {
        // Track FPS
        float currentFPS = 1.0f / Time.deltaTime;
        totalFPS += currentFPS;
        frameCount++;

        timeElapsed += Time.deltaTime;
        if (timeElapsed >= updateInterval)
        {

            // Calculate averages and reset stats
            float averageFPS = totalFPS / frameCount;
            long allocatedMemory = Profiler.GetTotalReservedMemoryLong();
            float convertedMemory = allocatedMemory / (1024f * 1024f);

            totalRAM += convertedMemory;
            ramSampleCount++;
            averageRam = totalRAM / ramSampleCount;

            // Update statsText safely
            UpdateStatsText(averageFPS, averageRam);

            // Reset counters for the next interval
            timeElapsed = 0;
            totalFPS = 0;
            frameCount = 0;
        }
    }

    private void UpdateStatsText(float averageFPS, float averageRAM)
    {
        if (statsText != null)
        {
            statsText.text = $"Average FPS: {averageFPS:F1}\n" + 
                             $"Avg. RAM: {averageRAM:F1} MB";
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
