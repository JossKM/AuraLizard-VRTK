using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Node Visualizer Settings")]
public class NodeVisualizationSettings : ScriptableObject
{
    public Color nodeColor;
    public Color edgeColor;
    public Color selectEdgeColor;
    public Color selectNodeColor;

    public AudioClip nodePingSound;
    public AudioClip edgePingSound;

    public float PING_DELAY = 0.25f;
    public float SIGNAL_RANGE = 4.0f;
    public float SIGNAL_LOSS = 0.25f;
    public float SIGNAL_PING_THRESH = 0.01f;

    public string[] valuesToWatch;

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
