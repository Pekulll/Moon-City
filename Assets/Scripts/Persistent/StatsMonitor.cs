using System.Collections;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class StatsMonitor : MonoBehaviour
{
    private GameObject fpsBackground;
    private GameObject memoryBackground;

    private Text fpsMonitor;
    private Text memoryMonitor;

    private void Start()
    {
        fpsBackground = transform.Find("STATS_Monitor/FPS_Background").gameObject;
        memoryBackground = transform.Find("STATS_Monitor/MEMORY_Background").gameObject;
        fpsMonitor = transform.Find("STATS_Monitor/FPS_Background/FPS_Monitor").GetComponent<Text>();
        memoryMonitor = transform.Find("STATS_Monitor/MEMORY_Background/MEMORY_Monitor").GetComponent<Text>();

        StartCoroutine(UpdateStats());
    }

    private IEnumerator UpdateStats()
    {
        while (true)
        {
            if (SettingsData.instance.settings.showFPS)
            {
                fpsBackground.SetActive(true);
                int fps = (int) (1f / Time.unscaledDeltaTime);
                fpsMonitor.text = (fps >= 0) ? fps + " FPS" : "0 FPS";
            }
            else
            {
                fpsBackground.SetActive(false);
            }
        
            if (SettingsData.instance.settings.showMemoryUsage)
            {
                memoryBackground.SetActive(true);
                memoryMonitor.text = (int)(Profiler.GetMonoHeapSizeLong() / 1000000) + " Mo";
            }
            else
            {
                memoryBackground.SetActive(false);
            }

            yield return new WaitForSecondsRealtime(.3f);
        }
    }
}
