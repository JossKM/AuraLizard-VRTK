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
    [SerializeField]
    float nodeScale = 0.1f;

    //will either get and return or just return a node by name. out param returns the Node with this name. If a Node was newly created then returns true
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
        newNodeObject.transform.localScale = new Vector3(nodeScale, nodeScale, nodeScale);
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
                GameObject newEdgeObject = Instantiate(edgePrefab);
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

    public void RunScaledPageRank(int kIterations, double scalingFactor, float iterationDelaySeconds)
    {
        StartCoroutine(ScaledPageRank(kIterations, scalingFactor, iterationDelaySeconds));
    }

   public IEnumerator ScaledPageRank(int kIterations, double scalingFactor, float iterationDelaySeconds)
    {
        const string PAGE_RANK = "PageRank";
        const string PAGE_RANK_IN = "PageRankIn";
        int N = nodes.Count;
        double initialValue = 1.0 / N;
        foreach (Node node in nodes) // Initialize values
        {
            node.data.Add(PAGE_RANK, initialValue);
            node.data.Add(PAGE_RANK_IN, new double());
        }

        for(int i = 0; i < kIterations; i++) // PageRank update
        {
            foreach (Node node in nodes) // Initialize values
            {
                node.data[PAGE_RANK_IN] = 0.0;
            }

            foreach (Node node in nodes) //queue up next pageRank values
            {
                if(node.connections.Count == 0)
                {
                    node.data[PAGE_RANK_IN] = node.data[PAGE_RANK]; // If no edges give back to itself
                } else
                {
                    foreach (Node outNode in node.connections)
                    {
                        double data = (double)node.data[PAGE_RANK];
                        double newValue = ((double)outNode.data[PAGE_RANK_IN] + (data / node.connections.Count));
                        outNode.data[PAGE_RANK_IN] = newValue; //to give away
                    }
                }
            }

            double scalingReturn = ((1.0 - scalingFactor) / N); //to give back to all nodes after scaling down
            foreach (Node node in nodes) //assign new pageRank values
            {
                double dataIn = (double)node.data[PAGE_RANK_IN];
                double newPageRank = (scalingFactor * dataIn) + scalingReturn;
                node.data[PAGE_RANK] = newPageRank;
                node.label.text = node.name + ": " + newPageRank.ToString("N3");
            }

            yield return new WaitForSeconds(iterationDelaySeconds);
        }
    }
}
