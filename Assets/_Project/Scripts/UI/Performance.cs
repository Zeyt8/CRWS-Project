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

    void Update()
    {
        // Track FPS
        float currentFPS = 1.0f / Time.deltaTime;
        totalFPS += currentFPS;
        frameCount++;
        MPF = Time.deltaTime * 1000.0f;

        timeElapsed += Time.deltaTime;
        float averageFPS = totalFPS / frameCount;
        
        if (timeElapsed >= updateInterval)
        {
            long allocatedMemory = Profiler.GetTotalReservedMemoryLong();
            float convertedMemory = allocatedMemory / (1024f * 1024f);

            if (convertedMemory > maxRAM)
                maxRAM = convertedMemory;

            totalRAM += convertedMemory;
            ramSampleCount++;
            averageRam = totalRAM / ramSampleCount;

            timeElapsed = 0;
        }
        
        UpdateStatsText(averageFPS, averageRam, maxRAM, MPF);
        totalFPS = 0;
        frameCount = 0;
    }

    private void UpdateStatsText(float averageFPS, float averageRAM, float maxRAM, float MPF)
    {
        if (statsText != null)
        {
            statsText.text = $"Average FPS: {averageFPS:F1}\n" +
                             $"MPF: {MPF:F1}\n" +
                             $"Max RAM: {maxRAM:F1}\n" +
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
