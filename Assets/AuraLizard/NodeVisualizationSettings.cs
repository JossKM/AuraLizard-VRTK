using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ClipType
{
    NodeCreate,
    NodeHover,
    NodePing,
    NodeNotif,
    NodeDestroy,
    EdgeCreate,
    EdgePing,
    EdgeNotif,
    EdgeDestroy
}


[CreateAssetMenu(fileName = "Node Visualizer Settings")]
public class NodeVisualizationSettings : ScriptableObject
{
    public Color nodeColor;
    public Color edgeColor;
    public Color selectEdgeColor;
    public Color selectNodeColor;

    //public AudioClip node_PingSound;
    //public AudioClip node_NotifSound;
    //public AudioClip edge_PingSound;
    //public AudioClip edge_NotifSound;

    public List<AudioClip> audioClips = new List<AudioClip>();
    public List<float[]> audioSamples;

    //static float[] node_pingSamples = null;
    //static float[] node_notifSamples = null;
    //static float[] edge_pingSamples = null;
    //static float[] edge_notifSamples = null;

    public float PING_DELAY = 0.25f;
    [Range(0.1f,1.0f)]
    public float SIGNAL_LOSS = 0.25f;
    public float SIGNAL_PING_THRESH = 0.01f;

    public float PING_SCALE_SAMPLE = 1.0f;
    public float PING_SCALE_BASE = 0.25f;

    public float restBetweenNotes = 1.0f / 32.0f;

    public string[] valuesToWatch;

    public void Initialize()
    {
        audioSamples = new List<float[]>();
        for (int i = 0; i < audioClips.Count; i++)
        {
            float[] samples = new float[audioClips[i].samples * audioClips[i].channels];
            audioClips[i].GetData(samples, 0);
            audioSamples.Add(samples);
        }
    }

    public void ApplyToNode(Node target)
    {
        target.GetComponent<Material>().color = nodeColor;
    }

    public void CheckForUpdates(Node target)
    {
        foreach(string key in valuesToWatch)
        {
            if (target.data.ContainsKey(key))
            {

            }
        }
    }
}
