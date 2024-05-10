using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphDbg : MonoBehaviour {
	static public void Log(float value)
	{
		GraphDbg.Log(defaultGraphName, value);
	}

	static public void Log(string graphName, float value, int logLimit=logLimitDefault, bool isFitX = fixXDefault)
	{
		GraphData graphList;
		if (graphs==null)
			graphs = new Dictionary<string, GraphData>();
		if (!graphs.TryGetValue(graphName, out graphList))
		{
			graphList = new GraphData();
			graphList.samples = new LinkedList<float>();
			graphList.arguments = new GraphArguments();
			graphs.Add(graphName, graphList);
		}
		GraphArguments args = new GraphArguments(logLimit, isFitX);
		graphList.arguments = args;
		graphList.totalSum = graphList.totalSum+value;
		graphList.samples.AddLast(value);
		if (logLimit>0)
		{
			int n = System.Math.Max(graphList.samples.Count-logLimit, 0);
			float sum = 0;
			int countBefore = graphList.samples.Count;
			for (int i=0; i<n; i++)
			{
				sum+=graphList.samples.First.Value;
				graphList.samples.RemoveFirst();
			}
			if (graphList.samples.Count==0)
				graphList.totalSum = 0;
			else
				graphList.totalSum = graphList.totalSum-sum;
		}
	}

	static public float AverageValue(string graphName = null)
	{
		if (graphs==null)
			return 0;
		GraphData graphList;
		if (graphName!=null)
		{
			if (!graphs.TryGetValue(graphName, out graphList))
				return 0;
		}
		else
		{
			if (graphs.Values.Count==0)
				return 0;
			Dictionary<string, GraphData>.ValueCollection.Enumerator enumr = graphs.Values.GetEnumerator();
			enumr.MoveNext();
			graphList = enumr.Current;
			if (graphList.samples==null)
				return 0;			
		}
		if (graphList.samples.Count==0)
			return 0;
		return graphList.totalSum/graphList.samples.Count;
	}

	static public float SamplesAmount(string graphName = null)
	{
		if (graphs==null)
			return 0;
		GraphData graphList;
		if (graphName!=null)
		{
			if (!graphs.TryGetValue(graphName, out graphList))
				return 0;
		}
		else
		{
			if (graphs.Values.Count==0)
				return 0;
			Dictionary<string, GraphData>.ValueCollection.Enumerator enumr = graphs.Values.GetEnumerator();
			enumr.MoveNext();
			graphList = enumr.Current;
			if (graphList.samples==null)
				return 0;			
		}
		return graphList.samples.Count;
	}

	static public Vector3[] GraphToSegments(string graphName = null)
	{
		if (graphs==null)
			return new Vector3[0];
		GraphData graphList;
		if (graphName!=null)
		{
			if (!graphs.TryGetValue(graphName, out graphList))
				return new Vector3[0];
		}
		else
		{
			if (graphs.Values.Count==0)
				return new Vector3[0];
			Dictionary<string, GraphData>.ValueCollection.Enumerator enumr = graphs.Values.GetEnumerator();
			enumr.MoveNext();
			graphList = enumr.Current;
			if (graphList.samples==null)
				return new Vector3[0];			
		}
		int n = graphList.samples.Count;
		if (n==0)
			return new Vector3[0];
		int segmentsAmount = n-1;
		Vector3[] lines = new Vector3[segmentsAmount*2];
		int i = 0;
		Vector3 prev = new Vector3(0, graphList.samples.First.Value, 0);
		foreach (float value in graphList.samples)
		{
			if (i==0)
			{
				i++;
				continue;
			}
			Vector3 current = new Vector3(i, value, 0);
			lines[2*(i-1)] = prev;
			lines[2*(i-1)+1] = current;
			prev = current;
			i++;
		}

		return lines;
	}

	static string defaultGraphName = "DefaultGraphDbg";
	static private Dictionary<string, GraphData> graphs;

	static public GraphArguments GetGraphArguments(string graphName = null)
	{
		GraphArguments arguments;
		arguments = new GraphArguments();
		arguments.fitX = false;
		arguments.logLimit = 2000;
		if (graphs==null)
			return arguments;
		GraphData graphList;
		if (graphName!=null)
		{
			if (!graphs.TryGetValue(graphName, out graphList))
				return arguments;
		}
		else
		{
			if (graphs.Values.Count==0)
				return arguments;
			Dictionary<string, GraphData>.ValueCollection.Enumerator enumr = graphs.Values.GetEnumerator();
			enumr.MoveNext();
			graphList = enumr.Current;
		}
		return graphList.arguments;
	}

	const int logLimitDefault = 2000;
	const bool fixXDefault = false;

	public struct GraphArguments {
		public int logLimit;
		public bool fitX;
		public GraphArguments(int logLimitArg, bool fitXArg)
		{
			logLimit = logLimitArg;
			fitX = fitXArg;
		}
	};

	class GraphData {
		public GraphArguments arguments;
		public LinkedList<float> samples;
		public float totalSum = 0;
	};

	// Use this for initialization
	void Start () {
		graphs = null;
	}

	// Update is called once per frame
	void Update () {

	}

}
