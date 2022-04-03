using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Node", menuName = "AuraLizard/Node")]
public class Node : MonoBehaviour
{
    [SerializeField]
    public List<Node> connections = new List<Node>();
    [SerializeField]
    public Dictionary<string, object> data = new Dictionary<string, object>();
    [SerializeField]
    public TMPro.TextMeshPro label;

    [SerializeField]
    public NodeAudioResponse audio;

    static float pingDelay = 0.5f;
    static float signalLoss = 0.2f;
    static float signalPingThreshold = 0.0f;

    IEnumerator pingCoroutine = null;
    IEnumerator audioPingCoroutine = null;

    public void Ping(float signal)
    {
        audio.Stop();
        if (signal > signalPingThreshold)
        {
            if (pingCoroutine != null)
            {
                StopCoroutine(pingCoroutine);
            }
            pingCoroutine = PingCoroutine(signal);
            StartCoroutine(pingCoroutine);
        }
    }

    private IEnumerator PingCoroutine(float signal)
    {
        audio.Ping(signal);

        yield return new WaitForSeconds(pingDelay);
        foreach(Node connection in connections)
        {
            connection.Ping(signal - signalLoss);
            yield return new WaitForSeconds(0.1f);
        }
        pingCoroutine = null;
    }
}
