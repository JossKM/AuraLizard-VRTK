using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//[CreateAssetMenu(fileName = "Graph", menuName = "AuraLizard/Graph")]
public class Graph : MonoBehaviour
{
    [SerializeField]
    public List<Node> nodes = new List<Node>();
    public HashSet<Edge> edges = new HashSet<Edge>();
    private Dictionary<string, Node> nodeNames = new Dictionary<string, Node>();

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

    [SerializeField]
    public UnityEvent<Edge> eventOnEdgeAdded;
    public UnityEvent<Edge> eventOnEdgeRemoved;
    //public UnityEvent eventOnOutEdgeAdded;
    //public UnityEvent eventOnEdgeRemoved;

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
        outNode.label.text = name;
        nodes.Add(outNode);
        return true;
    }

    public void AddEdge(string source, string destination, float weight = 1.0f)
    {
        AddEdge(nodeNames[source], nodeNames[destination]);
    }
    public void RemoveEdge(string source, string destination)
    {
        RemoveEdge(nodeNames[source], nodeNames[destination]);
    }

    //Will add edge to node and to the Graph
    public void AddEdge(Node source, Node destination, float weight = 1.0f)
    {
        GameObject newEdgeObject = Instantiate(edgePrefab);
        newEdgeObject.name = destination.name;
        Edge edgeComponent = newEdgeObject.GetComponent<Edge>();
        edgeComponent.source = source;
        edgeComponent.destination = destination;
        edgeComponent.weight = weight;

        //Connect it
        edges.Add(edgeComponent);
        source.outEdges.Add(edgeComponent);
        destination.inEdges.Add(edgeComponent);
    
        edgeComponent.UpdateVisual(edgeWidth);
        eventOnEdgeAdded.Invoke(edgeComponent);
    }

    public void RemoveEdge(Node source, Node destination, bool bothWays = false)
    {
        HashSet<Edge> allSharedEdges = source.outEdges;
        allSharedEdges.IntersectWith(destination.inEdges);

        foreach (Edge edge in allSharedEdges)
        {
            Destroy(edge.gameObject);
            edges.Remove(edge);
        }
        allSharedEdges.Clear();

        if (bothWays)
        {
            RemoveEdge(destination, source, false);
        }
    }

    public void RemoveEdge(Edge edge)
    {
        edge.source.outEdges.Remove(edge);
        edge.destination.inEdges.Remove(edge);
        Destroy(edge.gameObject);
        edges.Remove(edge);
    }
    public void RemoveAllEdges()
    {
        foreach (Edge edge in edges)
        {
            edge.source.outEdges.Remove(edge);
            edge.destination.inEdges.Remove(edge);
            Destroy(edge.gameObject);
        }
        edges.Clear();
    }

    public void RemoveInEdgesFromNode(Node node)
    {
        foreach (Edge edge in node.inEdges)
        {
            edge.source.outEdges.Clear();
            Destroy(edge.gameObject);
        }

        node.inEdges.Clear();
    }

    public void RemoveOutEdgesFromNode(Node node)
    {
        foreach (Edge edge in node.outEdges)
        {
            edge.destination.inEdges.Clear();
            Destroy(edge.gameObject);
        }
        node.outEdges.Clear();
    }

    public void RemoveEdgesFromNode(Node node)
    {

        RemoveInEdgesFromNode(node);
        RemoveOutEdgesFromNode(node);
    }

    public void RemoveNode(Node node)
    {
        RemoveEdgesFromNode(node);
        Destroy(node.gameObject);
        nodes.Remove(node);
        nodeNames.Remove(node.name);
    }

    public void Clear()
    {
        foreach (Node node in nodes)
        {
            Destroy(node.gameObject);
        }
        foreach(Edge edge in edges)
        {
            Destroy(edge.gameObject);
        }
        nodes.Clear();
        edges.Clear();
        nodeNames.Clear();
    }

    public void RandomizeEdges(float probability = 0.3f)
    {
        RemoveAllEdges();

        for (int i = 0; i < nodes.Count; i++)
        {
            Node source = nodes[i];
            source.outEdges.Clear();
            for (int j = i + 1; j < nodes.Count; j++)
            {
                Node destination = nodes[j];
                if (Random.value <= probability)
                {
                    AddEdge(source, destination);
                }
            }
        }
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
                if(node.outEdges.Count == 0)
                {
                    node.data[PAGE_RANK_IN] = node.data[PAGE_RANK]; // If no edges give back to itself
                } else
                {
                    foreach (Edge edge in node.outEdges)
                    {
                        Node outNode = edge.destination;
                        double data = (double)node.data[PAGE_RANK];
                        double newValue = ((double)outNode.data[PAGE_RANK_IN] + (data / node.outEdges.Count));
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
