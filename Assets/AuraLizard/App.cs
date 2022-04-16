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
            } else
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

    [SerializeField]
    GameObject selectionSphere;
    [SerializeField]
    GameObject hoverSphere;

    [SerializeField]
    bool isEditModeOn;
    public UnityEvent<bool> eventToggleEditMode;
    public UnityEvent<float> eventAnimationUpdate;

    public UnityEvent eventPageRank;

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
    }

    public void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            graph.RandomizeEdges(0.5f);
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
        int beginIndex = 1;
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

            try
            {
                LoadAllFilesInFolder(ofn.file, true);
            }
            catch (Exception ef)
            {
                Debug.LogError("Failed to open file: " + ofn.file + " -- " + ef.Message);
            }
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
        yield return StartCoroutine(ApplyDeltaToGraphCoroutine(new AdjecencyListDelta(adjecencyAnim[animTimeIndex], adjecencyAnim[startTime])));
        animTimeIndex = startTime;
        eventAnimationUpdate.Invoke(GetNormalizedTime());
        while (animTimeIndex < adjecencyDeltaAnim.Count)
        {
            yield return new WaitForSeconds(timeDelay);
            yield return StartCoroutine(ApplyDeltaToGraphCoroutine(adjecencyDeltaAnim[animTimeIndex]));
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
        foreach (var subtractionTable in delta.edgeSubtractions)
        {
            string sourceName = subtractionTable.Key;
            foreach (var edge in subtractionTable.Value)
            {
                graph.RemoveEdge(sourceName, edge);
                yield return new WaitForSeconds(timeDelayBetweenChanges);
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

                yield return new WaitForSeconds(timeDelayBetweenChanges);
            }
        }
    }
}
