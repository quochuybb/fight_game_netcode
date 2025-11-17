// NetworkPingPong.cs
// Attach this to a NetworkObject that the client owns (e.g. player prefab).
// It periodically sends a PingServerRpc and expects a Pong back from server.
// The script computes RTT and estimates packet loss over a sliding time window.

using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkPingPong : NetworkBehaviour
{
    [Header("Ping Settings")]
    public float pingInterval = 0.5f; // seconds between pings
    public float lossWindowSeconds = 10f; // measure loss over last N seconds

    // sequence number increment
    private int seq = 0;

    // pending sent pings (seq -> sentTime)
    private Dictionary<int, float> pending = new Dictionary<int, float>();

    // store recent history entries (sentTime, received)
    private List<PingEntry> history = new List<PingEntry>();

    // reference to HUD (optional) - will find in scene
    private NetworkStatsHUD hud;

    // struct for history
    private struct PingEntry
    {
        public int seq;
        public float sentTime;
        public bool received;
    }

    private void Start()
    {
        hud = FindObjectOfType<NetworkStatsHUD>();

        // only the owner/client should start pinging
        if (IsOwner)
        {
            InvokeRepeating(nameof(SendPing), 0.1f, pingInterval);
        }
    }

    private void OnDestroy()
    {
        CancelInvoke(nameof(SendPing));
    }

    private void SendPing()
    {
        if (!IsOwner) return;
        seq++;
        int currentSeq = seq;
        float t = Time.unscaledTime;
        pending[currentSeq] = t;

        // add to history
        history.Add(new PingEntry { seq = currentSeq, sentTime = t, received = false });

        // send server rpc (unreliable)
        PingServerRpc(currentSeq, t);
    }

    [ServerRpc(Delivery = RpcDelivery.Unreliable)]
    private void PingServerRpc(int sentSeq, float clientSentTime, ServerRpcParams rpcParams = default)
    {
        // server received ping; reply ONLY to the sender client
        ulong sender = rpcParams.Receive.SenderClientId;

        // Use ClientRpcParams to target the originating client
        var clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { sender } }
        };

        // reply with same seq and clientSentTime; server could also include serverTime if needed
        PongClientRpc(sentSeq, clientSentTime, clientRpcParams);
    }

    [ClientRpc]
    private void PongClientRpc(int sentSeq, float clientSentTime, ClientRpcParams clientRpcParams = default)
    {
        // This will be invoked on the client that owns this NetworkBehaviour (because server targeted it)
        if (!IsOwner) return;

        float now = Time.unscaledTime;
        // compute RTT
        if (pending.TryGetValue(sentSeq, out float sentTime))
        {
            float rtt = (now - sentTime) * 1000f; // ms
            pending.Remove(sentSeq);

            // find in history and mark received
            for (int i = history.Count - 1; i >= 0; --i)
            {
                if (history[i].seq == sentSeq)
                {
                    var e = history[i];
                    e.received = true;
                    history[i] = e;
                    break;
                }
            }

            // prune old history and compute stats
            PruneAndComputeStats(now, rtt);
        }
    }

    private void PruneAndComputeStats(float now, float lastRttMs)
    {
        // remove history older than window
        float cutoff = now - lossWindowSeconds;
        int keepIndex = 0;
        for (int i = 0; i < history.Count; ++i)
        {
            if (history[i].sentTime >= cutoff)
            {
                history[keepIndex++] = history[i];
            }
        }
        if (keepIndex != history.Count)
        {
            history.RemoveRange(keepIndex, history.Count - keepIndex);
        }

        // compute loss & average RTT from entries in window
        int totalSent = history.Count;
        int received = 0;
        float sumRtt = 0f;
        int rttCount = 0;
        foreach (var e in history)
        {
            if (e.received)
            {
                received++;
                // We don't store per-entry RTTs, but we can approximate:
                // For more accuracy, you'd store rtt per entry. For now estimate using lastRttMs for smoothing.
            }
        }

        float lossPercent = 0f;
        if (totalSent > 0)
        {
            lossPercent = 100f * (totalSent - received) / (float)totalSent;
        }

        // ask HUD to update (main thread) — already on main thread
        if (hud != null)
        {
            hud.SetPacketLossPercent(lossPercent);
            hud.SetPingMs(lastRttMs);
        }
    }
}
