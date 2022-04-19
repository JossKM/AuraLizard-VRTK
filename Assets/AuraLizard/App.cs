using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;

public class AdjecencyListDelta
{
    public Dictionary<string, List<string>> edgeAdditions;
    public Dictionary<string, List<string>> edgeSubtractions;
    public AdjecencyListDelta(Dictionary<string, List<string>> oldAdjList, Dictionary<string, List<string>> newAdjList)
    {
        edgeAdditions = WhatIsNewFrom(oldAdjList, newAdjList);
        edgeSubtractions = WhatIsNewFrom(newAdjList, oldAdjList);
    }

    // Returns only an adjecency list containing what is in B that was not in A. In other words B-A. In other words what must be added to A to have everything B has.
    public Dictionary<string, List<string>> WhatIsNewFrom(Dictionary<string, List<string>> a, Dictionary<string, List<string>> b)
    {
        Dictionary<string, List<string>> additions = new Dictionary<string, List<string>>();

        foreach (KeyValuePair<string, List<string>> newAdjecency in b)
        {
            List<string> edgesAdded = new List<string>();

            if (!a.ContainsKey(newAdjecency.Key))
            {
                a.Add(newAdjecency.Key, newAdjecency.Value); // An entire node was added
            }
            else
            {
                foreach (string edge in newAdjecency.Value)
                {
                    if (!a[newAdjecency.Key].Contains(edge)) // assumes old and new all have the same nodes for now...
                    {
                        //New edge was added!
                        edgesAdded.Add(edge);
                    }
                }
            }

            additions.Add(newAdjecency.Key, edgesAdded);
        }

        return additions;
    }
}

public class App : MonoBehaviour
{
    public string timeSeriesFilenameBaseToLoad;
    public int numFramesInSeries;

    public Graph graph;

    private Node selectedNode = null;
    private Node hoveredNode = null;

    [SerializeField]
    NodeVisualizationSettings settings;

    [SerializeField]
    GameObject selectionSphere;
    [SerializeField]
    GameObject hoverSphere;

    [SerializeField]
    public bool isEditModeOn;
    public UnityEvent<bool> eventToggleEditMode;
    public UnityEvent<float> eventAnimationUpdate;

    [Header("Time series graph")]
    [SerializeField]
    List<Dictionary<string, List<string>>> adjecencyAnim = new List<Dictionary<string, List<string>>>();
    [SerializeField]
    List<AdjecencyListDelta> adjecencyDeltaAnim = new List<AdjecencyListDelta>();

    [SerializeField]
    float animTimeIntervalBetweenGraphs = 0.5f;
    //[SerializeField]
    //float animTime = 0.0f;
    [SerializeField]
    int animTimeIndex = 0;

    //[SerializeField]
    //List<QueuableCommand> editQueue;

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
            if (isEditModeOn && selectedNode != null && selectedNode != node)
            {
                Edge added = graph.AddEdge(selectedNode, node);
                added.Notif(ClipType.EdgeCreate, 1.0f, 1.0f, Color.green);
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
            hoveredNode = null;
        }
        else
        {
            if (node != hoveredNode)
            {
                node.Notif(ClipType.NodeHover, 0.05f, 2.0f, Color.blue);
                hoveredNode = node;
            }

            hoverSphere.SetActive(true);
            hoverSphere.transform.position = node.gameObject.transform.position;
            hoverSphere.transform.localScale = node.GetDimensions() * 2.5f;
        }
    }

    void LoadGraphTimeSeries(string filenameBase, int start, int end)
    {
        adjecencyAnim.Clear();
        for (int i = start; i <= end; i++)
        {
            AppendGraphTimeSeries((filenameBase + i.ToString()));
        }
    }

    void LoadGraph(string filePath, bool isFIleOutsideGame = true)
    {
        adjecencyAnim.Clear();
        Dictionary<string, List<string>> csvData;
        csvData = GraphReader.LoadAdjecencyList(filePath, isFIleOutsideGame);
        SetGraphFromAdjecencyList(csvData);

    }

    void AppendGraphTimeSeries(string filePath, bool isFIleOutsideGame = true)
    {
        Dictionary<string, List<string>> csvData;
        csvData = GraphReader.LoadAdjecencyList(filePath, isFIleOutsideGame);
        adjecencyAnim.Add(csvData);
    }

    void UpdateGraphEdgesFromAdjecencyList(Dictionary<string, List<string>> adjecencyList)
    {
        graph.RemoveAllEdges();
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

    void SetGraphFromAdjecencyList(Dictionary<string, List<string>> adjecencyList)
    {
        graph.Clear();

        int numNodesCreated = 0;

        foreach (KeyValuePair<string, List<string>> adjecencyEntry in adjecencyList)
        {
            string nameOfSourceNode = adjecencyEntry.Key;

            if (graph.AddAndGetNode(nameOfSourceNode, out Node sourceNode))
            {
                PingNodeCreated(sourceNode, numNodesCreated, numNodesCreated * settings.restBetweenNotes);
                numNodesCreated++;
            }

            for (int edgeIndex = 0; edgeIndex < adjecencyEntry.Value.Count; edgeIndex++)
            {
                string nameOfDestinationNode = adjecencyEntry.Value[edgeIndex];

                if (graph.AddAndGetNode(nameOfDestinationNode, out Node destinationNode))
                {
                    PingNodeCreated(destinationNode, numNodesCreated, numNodesCreated * settings.restBetweenNotes);
                    numNodesCreated++;
                }

                graph.AddEdge(sourceNode, destinationNode);
            }
        }
    }

    private void Awake()
    {
        settings.Initialize();
    }

    // Start is called before the first frame update
    void Start()
    {
        eventToggleEditMode.AddListener(ToggleEditModeListener);
    }

    public void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            Randomize();
        }

        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ToggleEditMode(!isEditModeOn);
        }

        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            OpenGraphSelectMenu();
        }

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            PlayAnim();
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            LoadExampleData();
        }
    }

    public void Randomize()
    {
        graph.RandomizeEdges(0.5f);
    }

    public void CalculateDeltas()
    {
        adjecencyDeltaAnim.Clear();
        for (int timeIndex = 1; timeIndex < adjecencyAnim.Count; timeIndex++)
        {
            AdjecencyListDelta delta = new AdjecencyListDelta(adjecencyAnim[timeIndex - 1], adjecencyAnim[timeIndex]);
            adjecencyDeltaAnim.Add(delta);
        }
    }

    public void LoadExampleData()
    {
        string basename = "example/day_";
        int beginIndex = 0;
        int endIndex = 30;

        for (int i = beginIndex; i <= endIndex; i++)
        {
            try
            {
                string file = basename + i.ToString();
                AppendGraphTimeSeries(file, false);
            }
            catch (Exception lol)
            {
            }
        }
        CalculateDeltas();
        SetGraphFromAdjecencyList(adjecencyAnim[0]);
        animTimeIndex = 0;
    }

    private void LoadAllFilesInFolder(string folderPath, bool isExternal)
    {
        string[] files = Directory.GetFiles(folderPath);
        adjecencyAnim.Clear();
        foreach (string file in files)
        {
            try
            {
                AppendGraphTimeSeries(file);
            }
            catch (Exception lol)
            {
            }
        }
        SetGraphFromAdjecencyList(adjecencyAnim[0]);
        animTimeIndex = 0;
        CalculateDeltas();
    }

    public void OpenGraphSelectMenu()
    {
        OpenFileName ofn = new OpenFileName();
        ofn.defExt = ".csv";
        if (!OpenFileDialog.OpenFile(ofn))
        {
            return;
        }
        Debug.Log("Opening: " + ofn.file);

        try
        {
            LoadGraph(ofn.file);
        }
        catch (Exception e)
        {
            //  Debug.LogError("Failed to open file: " + ofn.file + " -- " + e.Message);

            //  try
            //  {
            LoadAllFilesInFolder(ofn.file, true);
            //   }
            // catch (Exception ef)
            // {
            //     Debug.LogError("Failed to open file: " + ofn.file + " -- " + ef.Message);
            // }
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

    public void RunPageRank()
    {
        graph.RunScaledPageRank(50, 0.1, 0.00f);
    }

    public void PlayAnim()
    {
        if (adjecencyAnim.Count > 0)
        {
            StartCoroutine(AnimateTimeSeries(animTimeIntervalBetweenGraphs));
        }
    }

    public float GetNormalizedTime()
    {
        return ((float)animTimeIndex / (float)(adjecencyAnim.Count - 1));
    }

    public void SetNormalizedTimeTo(float time)
    {
        int newTime = Mathf.FloorToInt(time * (float)(adjecencyAnim.Count - 1));
        ApplyDeltaToGraph(new AdjecencyListDelta(adjecencyAnim[animTimeIndex], adjecencyAnim[newTime]));
    }

    IEnumerator AnimateTimeSeries(float timeDelay, int startTime = 0)
    {
        //Go from whatever frame we are on to the starting frame
        yield return StartCoroutine(ApplyDeltaToGraphCoroutine(new AdjecencyListDelta(adjecencyAnim[animTimeIndex], adjecencyAnim[startTime]), settings.restBetweenNotes));
        animTimeIndex = startTime;
        eventAnimationUpdate.Invoke(GetNormalizedTime());
        while (animTimeIndex < adjecencyDeltaAnim.Count)
        {
            yield return StartCoroutine(ApplyDeltaToGraphCoroutine(adjecencyDeltaAnim[animTimeIndex], settings.restBetweenNotes));
            yield return new WaitForSeconds(timeDelay);
            animTimeIndex++;//= Mathf.Min(animTimeIndex + 1, adjecencyAnim.Count - 1);
            eventAnimationUpdate.Invoke(GetNormalizedTime());
        }
    }

    void ApplyDeltaToGraph(AdjecencyListDelta delta)
    {
        foreach (var subtractionTable in delta.edgeSubtractions)
        {
            string sourceName = subtractionTable.Key;
            foreach (var edge in subtractionTable.Value)
            {
                graph.RemoveEdge(sourceName, edge);
            }
        }

        foreach (var additionTable in delta.edgeAdditions)
        {
            string sourceName = additionTable.Key;

            Node source;
            graph.AddAndGetNode(sourceName, out source);

            foreach (var edge in additionTable.Value)
            {
                Node destination;
                graph.AddAndGetNode(edge, out destination);
                graph.AddEdge(source, destination);
            }
        }
    }

    IEnumerator ApplyDeltaToGraphCoroutine(AdjecencyListDelta delta, float timeDelayBetweenChanges = 0.016f)
    {
        int numSubtractions = 0;
        foreach (var subtractionTable in delta.edgeSubtractions)
        {
            string sourceName = subtractionTable.Key;
            foreach (var destinationNode in subtractionTable.Value)
            {
                graph.RemoveEdgeWithSound(sourceName, destinationNode, numSubtractions);
                numSubtractions++;
                yield return new WaitForSeconds(timeDelayBetweenChanges);
            }
        }

        int numNodesAdded = 0;
        int numEdgesAdded = 0;
        foreach (var additionTable in delta.edgeAdditions)
        {
            string sourceName = additionTable.Key;

            Node source;
            if (graph.AddAndGetNode(sourceName, out source))
            {
                PingNodeCreated(source, numNodesAdded, 0.0f);
                numNodesAdded++;
            }

            foreach (var edge in additionTable.Value)
            {
                Node destination;
                if (graph.AddAndGetNode(edge, out destination))
                {
                    PingNodeCreated(destination, numNodesAdded, 0.0f);
                    numNodesAdded++;
                }

                Edge newEdge = graph.AddEdge(source, destination);
                NotifEdgeCreated(newEdge, numEdgesAdded);
                numEdgesAdded++;
                yield return new WaitForSeconds(timeDelayBetweenChanges);
            }
        }
    }

    void PingNodeCreated(Node node, int numCreated, float delay)
    {
        node.audioResponse.Ping(ClipType.NodeCreate, 0.5f, AudioResponsiveElement.PitchToRaiseByNotes(numCreated), delay, Color.green);
    }
    void NotifEdgeCreated(Edge edge, int numCreated)
    {
        edge.Notif(ClipType.EdgeCreate, 0.5f, AudioResponsiveElement.PitchToRaiseByNotes(numCreated), Color.green);
    }
}
