using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//Find Node from edge X
//Find out Edge from node
//Find in Edge from node
//Find all Nodes connected to node

[System.Serializable]
public class Node : MonoBehaviour
{
    [SerializeField]
    public HashSet<Edge> outEdges = new HashSet<Edge>();
    public HashSet<Edge> inEdges = new HashSet<Edge>();

    [SerializeField]
    public Dictionary<string, object> data = new Dictionary<string, object>();
 

    [Header("Game stuff")]

    [SerializeField]
    public TMPro.TextMeshPro label;

    [SerializeField]
    public NodeAudioResponse audio;

    static float PING_DELAY = 0.25f;
    static float SIGNAL_RANGE = 4.0f;
    static float SIGNAL_LOSS = 1.0f/SIGNAL_RANGE;
    static float SIGNAL_PING_THRESH = 0.01f;
    IEnumerator pingCoroutine = null;

    private Collider collider;

    public UnityEvent eventOnPositionChanged;

    public Vector3 GetDimensions()
    {
        return collider.bounds.size;
    }
    public float GetRadius()
    {
        Vector3 dimensions = GetDimensions();
        float maxRadius = Mathf.Max(dimensions.x, Mathf.Max(dimensions.y, dimensions.z));
        return maxRadius;
    }

    private void Awake()
    {
        collider = GetComponentInChildren<Collider>();
        eventOnPositionChanged.AddListener(OnPositionChangedListener);
    }

    public void Ping(float signal, float delay)
    {
        if (signal > SIGNAL_PING_THRESH)
        {
            audio.Ping(signal, delay);
            foreach (Edge connection in outEdges)
            {
                connection.destination.Ping(signal - SIGNAL_LOSS, delay + PING_DELAY);
            }
        }
    }

    private void OnPositionChangedListener()
    {
        foreach(Edge edge in outEdges)
        {
            edge.UpdateVisual();
        }

        foreach (Edge edge in inEdges)
        {
            edge.UpdateVisual();
        }
    }
}
