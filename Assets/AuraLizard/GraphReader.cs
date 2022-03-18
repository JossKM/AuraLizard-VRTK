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
	static char[] TRIM_CHARS = { '\"' };
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
			var values = Regex.Split(lines[lineIdx], SPLIT_RE);
			if (values.Length == 0 || values[0] == "") continue;

			List<string> edges = new List<string>();

			for (int i = 1; i < values.Length; i++) // Add edges
			{

				if (values[i] == "")
				{
					break;
				}

				edges.Add(values[i]);
			}

			graph[values[0]] = edges; // At source add list of connected edges

		}
		return graph;
	}
}