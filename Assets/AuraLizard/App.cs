using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.InputSystem;
using UnityEngine.Events;
public class App : MonoBehaviour
{
    public string timeSeriesFilenameBaseToLoad;
    public int numFramesInSeries;

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

    [Header("Time series graph")]
    [SerializeField]
    List<Dictionary<string, List<string>>> adjecencyDataTimeSeries = new List<Dictionary<string, List<string>>>();
    [SerializeField]
    float animTimeIntervalBetweenGraphs = 0.0f;
    //[SerializeField]
    //float animTime = 0.0f;
    [SerializeField]
    int animTimeIndex = 0;

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

    void LoadGraphTimeSeries(string filenameBase, int start, int end)
    {
        for(int i = start; i <= end; i++)
        {
            AppendGraphTimeSeries((filenameBase + i.ToString()));
        }
    }

    void AppendGraphTimeSeries(string fileName)
    {
        Dictionary<string, List<string>> csvData;
        csvData = GraphReader.LoadAdjecencyList(fileName);
        adjecencyDataTimeSeries.Add(csvData);
    }

    void SetGraphFromAdjecencyList(Dictionary<string, List<string>> adjecencyList)
    {
        graph.Clear();

        foreach (KeyValuePair<string, List<string>> adjecencyEntry in adjecencyList)
        {
            string nameOfSourceNode = adjecencyEntry.Key;

            graph.AddAndGetNode(nameOfSourceNode, out Node sourceNode);

            for (int edgeIndex = 0; edgeIndex < adjecencyEntry.Value.Count; edgeIndex++)
            {
                string nameOfDestinationNode = adjecencyEntry.Value[edgeIndex];

                graph.AddAndGetNode(nameOfDestinationNode, out Node destinationNode);
                graph.AddEdge(sourceNode, destinationNode);
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
            LoadGraphTimeSeries(timeSeriesFilenameBaseToLoad, 1, numFramesInSeries);
            //AppendGraphTimeSeries(fileToLoad);
            SetGraphFromAdjecencyList(adjecencyDataTimeSeries[animTimeIndex]);
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

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            StartCoroutine(AnimateTimeSeriesOnce(animTimeIntervalBetweenGraphs));
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

    IEnumerator AnimateTimeSeriesOnce(float timeDelay)
    {
        for(animTimeIndex = 0; animTimeIndex < adjecencyDataTimeSeries.Count; animTimeIndex++)
        {
            SetGraphFromAdjecencyList(adjecencyDataTimeSeries[animTimeIndex]);
            yield return new WaitForSeconds(timeDelay);
        }
       // animTimeIndex = (animTimeIndex + 1) % adjecencyDataTimeSeries.Count;
    }
}
