using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "Graph", menuName = "AuraLizard/Graph")]
public class Graph : MonoBehaviour
{
    [SerializeField]
    public List<Node> nodes = new List<Node>();
    public Dictionary<string, Node> nodeNames = new Dictionary<string, Node>();

    [SerializeField]
    GameObject edgePrefab;

    [SerializeField]
    GameObject nodePrefab;

    [SerializeField]
    public Vector3 dimensions = new Vector3(10, 10, 10);

    [SerializeField]
    float edgeWidth = 0.1f;

    //out param returns the Node with this name. If a Node was newly created then returns true
    public bool AddAndGetNode(string name, out Node outNode)
    {
        if (nodeNames.ContainsKey(name)) // if node does not exist...
        {
            outNode = nodeNames[name];
            return false;
        }

        // create it!
        GameObject newNodeObject = Instantiate(nodePrefab, transform);
        Vector3 newPos = new Vector3(
               (Random.value * (dimensions.x)) - dimensions.x * 0.5f,
               (Random.value * (dimensions.y)) - dimensions.y * 0.5f,
               (Random.value * (dimensions.z)) - dimensions.z * 0.5f
              );
        newNodeObject.transform.position = newPos;
        outNode = newNodeObject.GetComponent<Node>();
        outNode.name = name;
        nodeNames[name] = outNode;
        nodes.Add(outNode);
        return true;
    }

    public void GenerateEdges()
    {
        foreach (Node node in nodes)
        {
            foreach(Node destination in node.connections)
            {
                GameObject newEdgeObject = Instantiate(edgePrefab, node.transform);
                Vector3 toTarget = destination.transform.position - node.transform.position;
                float distance = toTarget.magnitude;
                newEdgeObject.transform.localScale = new Vector3(edgeWidth, edgeWidth, distance);
                newEdgeObject.transform.position = node.transform.position;
                newEdgeObject.transform.rotation = Quaternion.LookRotation(toTarget, Vector3.up);
                newEdgeObject.name = destination.name;
            }
        }
    }

    public void UpdateEdges()
    {
    }
}
