using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResolutionCycler : MonoBehaviour
{
    private readonly Resolution[] resolutions = new Resolution[]
    {
        new Resolution { width = 1920, height = 1080, refreshRate = 60 },
        new Resolution { width = 1600, height = 900, refreshRate = 60 },
        new Resolution { width = 1280, height = 720, refreshRate = 60 },
        new Resolution { width = 800, height = 600, refreshRate = 60 }
    };
    [SerializeField] private TextMeshProUGUI resolutionText;
    private int currentIndex = 0;

    private void Start()
    {
        resolutionText.text = resolutions[currentIndex].width + "x" + resolutions[currentIndex].height;
    }
    
    public void CycleResolution()
    {
        currentIndex = (currentIndex + 1) % resolutions.Length;
        Resolution res = resolutions[currentIndex];
        resolutionText.text = resolutions[currentIndex].width + "x" + resolutions[currentIndex].height;
        Screen.SetResolution(res.width, res.height, FullScreenMode.Windowed, res.refreshRate);
        Debug.Log($"Changed resolution to {res.width}x{res.height}@{res.refreshRate}Hz");
    }
}

