// NetworkStatsHUD.cs
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class NetworkStatsHUD : MonoBehaviour
{
    [Header("Display")]
    public TextMeshProUGUI uiText; // optional: use TextMeshPro if available
    public Vector2 anchor = new Vector2(10, 10); // position for OnGUI fallback
    public int fontSize = 20;

    [Header("Sampling")]
    [Tooltip("Sample interval in seconds for rates (packets/sec, bytes/sec)")]
    public float sampleInterval = 1.0f;

    [Header("Options")]
    public bool showSent = true;
    public bool showRecv = true;
    public bool showKB = true; // if true show KB/s else B/s
    public bool useOnGUIFallback = true;

    // Internal (thread-safe) accumulators
    static long s_sentBytes = 0;
    static long s_recvBytes = 0;
    static long s_sentPackets = 0;
    static long s_recvPackets = 0;

    // Snapshot values updated on main thread
    float lastSampleTime = 0f;
    int framesInInterval = 0;
    float lastFPS = 0f;
    float sentBytesPerSec = 0f;
    float recvBytesPerSec = 0f;
    float sentPacketsPerSec = 0f;
    float recvPacketsPerSec = 0f;

    // Ping & loss & jitter (updated by NetworkPingPong)
    float lastPingMs = 0f;
    float packetLossPercent = 0f;
    float lastJitterMs = 0f;

    // ---------- Public API used by NetworkPingPong ----------
    public static void AddSentBytes(long bytes) => Interlocked.Add(ref s_sentBytes, bytes);
    public static void AddRecvBytes(long bytes) => Interlocked.Add(ref s_recvBytes, bytes);
    public static void AddSentPacket(long count = 1) => Interlocked.Add(ref s_sentPackets, count);
    public static void AddRecvPacket(long count = 1) => Interlocked.Add(ref s_recvPackets, count);

    /// <summary>Call this from NetworkPingPong (on main thread) to update ping shown on HUD (ms)</summary>
    public void SetPingMs(float ms)
    {
        lastPingMs = ms;
    }

    /// <summary>Call this from NetworkPingPong (on main thread) to update loss percent (0..100)</summary>
    public void SetPacketLossPercent(float percent)
    {
        packetLossPercent = percent;
    }

    /// <summary>Call this from NetworkPingPong (on main thread) to update jitter (ms)</summary>
    public void SetJitterMs(float ms)
    {
        lastJitterMs = ms;
    }

    // For nicer formatting
    string FormatBytes(float bytesPerSec)
    {
        if (showKB)
        {
            return (bytesPerSec / 1024f).ToString("F2") + " KB/s";
        }
        else
        {
            return bytesPerSec.ToString("F0") + " B/s";
        }
    }

    void Awake()
    {
        lastSampleTime = Time.unscaledTime;
    }

    void Update()
    {
        // Count frames
        framesInInterval++;

        float now = Time.unscaledTime;
        float elapsed = now - lastSampleTime;
        if (elapsed >= sampleInterval)
        {
            // Swap accumulators atomically and zero them for next interval
            long sentBytes = Interlocked.Exchange(ref s_sentBytes, 0);
            long recvBytes = Interlocked.Exchange(ref s_recvBytes, 0);
            long sentPkts  = Interlocked.Exchange(ref s_sentPackets, 0);
            long recvPkts  = Interlocked.Exchange(ref s_recvPackets, 0);

            // Compute per-second rates
            sentBytesPerSec = sentBytes / elapsed;
            recvBytesPerSec = recvBytes / elapsed;
            sentPacketsPerSec = sentPkts / elapsed;
            recvPacketsPerSec = recvPkts / elapsed;

            // FPS computed as frames / elapsed
            lastFPS = framesInInterval / Mathf.Max(0.0001f, elapsed);

            // reset
            framesInInterval = 0;
            lastSampleTime = now;

            // Update visible text
            RefreshText();
        }
    }

    void RefreshText()
    {
        float totalThroughput = sentBytesPerSec + recvBytesPerSec;

        string s = $"FPS: {lastFPS:F1}\n";
        s += $"Ping: {lastPingMs:F0} ms   Jitter: {lastJitterMs:F1} ms\n";
        s += $"Loss: {packetLossPercent:F1}%   Throughput: {FormatBytes(totalThroughput)}\n";

        if (showSent)
        {
            s += $"Sent: {sentPacketsPerSec:F1} pkt/s, {FormatBytes(sentBytesPerSec)}\n";
        }
        if (showRecv)
        {
            s += $"Recv: {recvPacketsPerSec:F1} pkt/s, {FormatBytes(recvBytesPerSec)}\n";
        }

        if (uiText != null)
        {
            uiText.text = s;
            return;
        }

        // else OnGUI will render (fallback)
    }

    void OnGUI()
    {
        if (!useOnGUIFallback) return;
        if (uiText != null) return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = fontSize;
        style.normal.textColor = Color.white;
        Rect r = new Rect(anchor.x, anchor.y, 520, 240);
        string s = $"FPS: {lastFPS:F1}\n";
        s += $"Ping: {lastPingMs:F0} ms   Jitter: {lastJitterMs:F1} ms\n";
        s += $"Loss: {packetLossPercent:F1}%   Throughput: {FormatBytes(sentBytesPerSec + recvBytesPerSec)}\n";
        if (showSent) s += $"Sent: {sentPacketsPerSec:F1} pkt/s, {FormatBytes(sentBytesPerSec)}\n";
        if (showRecv) s += $"Recv: {recvPacketsPerSec:F1} pkt/s, {FormatBytes(recvBytesPerSec)}\n";
        GUI.Label(r, s, style);
    }
}

