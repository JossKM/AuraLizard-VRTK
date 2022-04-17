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
    public AudioResponsiveElement audioResponse;

    IEnumerator pingCoroutine = null;

    [SerializeField]
    private Collider collider;

    public UnityEvent eventOnPositionChanged;
    public UnityEvent eventOnPing;

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

    //public void UpdateRadius(float newRadius)
    //{
    //    audioResponse.Initialize();
    //}

    private void Awake()
    {
        eventOnPositionChanged.AddListener(OnPositionChangedListener);
    }

    public void Ping(float signal, float delay)
    {
        if (signal > audioResponse.settings.SIGNAL_PING_THRESH)
        {
            eventOnPing.Invoke();
            audioResponse.Ping(ClipType.NodePing, signal, delay);
            foreach (Edge connection in outEdges)
            {
                connection.Ping(signal - audioResponse.settings.SIGNAL_LOSS, delay + audioResponse.settings.PING_DELAY);
                //connection.destination.Ping(signal - settings.SIGNAL_LOSS, delay + settings.PING_DELAY);
            }
        }
    }

    public void Notif(ClipType type, float volume, float speed, Color glowColor)
    {
        audioResponse.Ping(type, volume, speed, 0.0f, glowColor);
    }

    public void Notif(float volume, float speed, Color glowColor)
    {
        audioResponse.Ping(ClipType.NodeNotif, volume, speed, 0.0f, glowColor);
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
