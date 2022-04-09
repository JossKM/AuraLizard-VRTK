using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class App : MonoBehaviour
{
    public string fileToLoad;

    public Graph graph;

    private Node selectedNode = null;

    [SerializeField]
    GameObject selectionSphere;
    [SerializeField]
    GameObject hoverSphere;

    //[SerializeField]
    //Color highlightColor;
    //[SerializeField]
    //Color selectColor;

    public void SelectNode(Node node)
    {
        if (node == null)
        {
            selectedNode = null;
            selectionSphere.SetActive(false);
        }
        else
        {
            selectedNode = node;
            selectionSphere.SetActive(true);
            selectionSphere.transform.position = selectedNode.gameObject.transform.position;
            selectionSphere.transform.localScale = selectedNode.gameObject.GetComponentInChildren<Collider>().bounds.size * 1.25f;
        }
    }
    public void HoverNode(Node node)
    {
        if (node == null)
        {
            hoverSphere.SetActive(false);
        }
        else
        {
            hoverSphere.SetActive(true);
            hoverSphere.transform.position = node.gameObject.transform.position;
            hoverSphere.transform.localScale = node.gameObject.GetComponentInChildren<Collider>().bounds.size * 2.5f;
        }
    }

    void LoadGraph()
    {
        Dictionary<string, List<string>> csvData;
        csvData = GraphReader.LoadAdjecencyList(fileToLoad);

        foreach (KeyValuePair<string, List<string>> adjecencyEntry in csvData)
        {
            string nameOfSourceNode = adjecencyEntry.Key;

            graph.AddAndGetNode(nameOfSourceNode, out Node sourceNode);

            for (int edgeIndex = 0; edgeIndex < adjecencyEntry.Value.Count; edgeIndex++)
            {
                string nameOfDestinationNode = adjecencyEntry.Value[edgeIndex];

                graph.AddAndGetNode(nameOfDestinationNode, out Node destinationNode);

                sourceNode.connections.Add(destinationNode);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // selectionSphere.GetComponent<Renderer>().material.SetColor("Tint", selectColor);
        // hoverSphere.GetComponent<Renderer>().material.SetColor("Tint", highlightColor);

        if (graph)
        {
            LoadGraph();
        }

        graph.GenerateEdges();
        // graph.RunScaledPageRank(3000, 0.9, 0.01f);
    }
}
