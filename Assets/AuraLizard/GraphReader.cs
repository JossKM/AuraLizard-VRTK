//By https://github.com/tiago-peres/blog/blob/master/csvreader/CSVReader.cs under MIT license

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
public class GraphReader
{
	static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
	static string LINE_SPLIT_RE = @"\n";
	static char[] TRIM_CHARS = { '\"', '\r', '\n', '.', ' '};
	public static Dictionary<string, List<string>> LoadAdjecencyList(string filePath)
	{
		Dictionary<string, List<string>> graph = null;

		TextAsset data = Resources.Load(filePath) as TextAsset;

		if (data == null)
		{
			throw new Exception("CSVReader: File failed to open: " + filePath);
		}

		var lines = Regex.Split(data.text, LINE_SPLIT_RE);
		if (lines.Length <= 1) return graph;

		graph = new Dictionary<string, List<string>>();

		for (var lineIdx = 0; lineIdx < lines.Length; lineIdx++)
		{
			string[] values = Regex.Split(lines[lineIdx], SPLIT_RE); // extract individual elements of a row

			string sourceNode = values[0].Trim(TRIM_CHARS);

			if (values.Length == 0 || sourceNode == "") continue;

			List<string> edges = new List<string>();

			for (int i = 1; i < values.Length; i++) // Add edges
			{
				var outEdge = values[i].Trim(TRIM_CHARS);
				if (outEdge == "")
				{
					continue;
				}

				edges.Add(outEdge);
			}

			graph[sourceNode] = edges; // At source add list of connected edges

		}
		return graph;
	}
}