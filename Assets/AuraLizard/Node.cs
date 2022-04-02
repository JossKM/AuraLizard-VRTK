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
}
