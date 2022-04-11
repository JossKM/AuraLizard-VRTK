using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.InputSystem;
using UnityEngine.Events;
public class App : MonoBehaviour
{
    public string fileToLoad;

    public Graph graph;

    private Node selectedNode = null;

    [SerializeField]
    GameObject selectionSphere;
    [SerializeField]
    GameObject hoverSphere;

    [SerializeField]
    bool isEditModeOn;
    public UnityEvent<bool> eventToggleEditMode;
    public UnityEvent eventPageRank;

    //Equivalent to calling SelectNode(null)
    public void DeselectNode()
    {
        SelectNode(null);
    }

    public void SelectNode(Node node)
    {
        if (node == null)
        {
            selectedNode = null;
            selectionSphere.SetActive(false);
        }
        else
        {
            //If selected a second node after this one?
            if(isEditModeOn && selectedNode != null && selectedNode != node)
            {
                graph.AddEdge(selectedNode, node);
            }

            selectedNode = node;
            selectionSphere.SetActive(true);

            //selectionSphere.transform.parent = node.transform;
            selectionSphere.transform.position = selectedNode.gameObject.transform.position;
            selectionSphere.transform.localScale = selectedNode.GetDimensions() * 2.0f;
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
            hoverSphere.transform.localScale = node.GetDimensions() * 2.5f;
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
                graph.AddEdge(sourceNode, destinationNode);
                //sourceNode.connections.Add(destinationNode);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        eventToggleEditMode.AddListener(ToggleEditModeListener);
        eventPageRank.AddListener(RunPageRankListener);

        if (graph)
        {
            LoadGraph();
        }

        // graph.RunScaledPageRank(3000, 0.9, 0.01f);
    }
    public void Update()
    {
        if(Keyboard.current.rKey.wasPressedThisFrame)
        {
            graph.RandomizeEdges(0.5f);
        }

        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ToggleEditMode(!isEditModeOn);
        }
    }

    public void ToggleEditMode(bool on)
    {
        eventToggleEditMode.Invoke(on);
    }

    private void ToggleEditModeListener(bool on)
    {
        isEditModeOn = on;
    }

    private void RunPageRankListener()
    {
        graph.RunScaledPageRank(3000, 0.9, 0.01f);
    }
}
