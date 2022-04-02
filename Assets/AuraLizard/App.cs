using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class App : MonoBehaviour
{
    public string fileToLoad;
    public Dictionary<string, List<string>> csvData;
    public List<NodeBehavior> nodeBehaviors;

    public Graph graph;
    // Start is called before the first frame update
    void Start()
    {
        if(graph)
        {
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

        graph.GenerateEdges();
        graph.RunScaledPageRank(3000, 0.9, 0.01f);
    }
}
