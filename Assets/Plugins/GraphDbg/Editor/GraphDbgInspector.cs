using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(GraphDbgComponent))]
public class GraphDbgInspector : Editor {
	GUIStyle style;
	Vector3[] lines;

	public override void OnInspectorGUI() {
		string name = null;
		EditorStyles.label.wordWrap = true;
		GraphDbgComponent targetPlayer = (GraphDbgComponent)target;
		if (targetPlayer.graphName!=null)
		{
			if (targetPlayer.graphName.Length>0)
				name = targetPlayer.graphName;
		}
		EditorGUILayout.BeginVertical(new GUILayoutOption[]{});
//		EditorGUILayout.BeginHorizontal(new GUILayoutOption[]{});
//		EditorGUILayout.LabelField("Graph Name: ", new GUILayoutOption[]{});
//		EditorGUILayout.TextField("", new GUILayoutOption[]{});
//		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal(new GUILayoutOption[]{});
		EditorGUILayout.LabelField("Samples:", new GUILayoutOption[]{});
		EditorGUILayout.TextField(""+GraphDbg.SamplesAmount(name), new GUILayoutOption[]{});
//		EditorGUILayout.LabelField("Min: ", new GUILayoutOption[]{});
//		EditorGUILayout.TextField(GraphDbg.MinValue(), new GUILayoutOption[]{});
//		EditorGUILayout.LabelField("Max: ", new GUILayoutOption[]{});
//		EditorGUILayout.TextField(GraphDbg.MaxValue(), new GUILayoutOption[]{});
		EditorGUILayout.LabelField("Average:", new GUILayoutOption[]{});
		EditorGUILayout.TextField(""+GraphDbg.AverageValue(name), new GUILayoutOption[]{});
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();

		if (style==null)
			style = new GUIStyle();
		style.margin.bottom = 5;
		style.margin.top = 5;
		style.margin.left = 5;
		style.margin.right = 5;
		style.border.bottom = 2;
		style.border.top = 2;
		style.border.left = 2;
		style.border.right = 2;

		Rect r = GUILayoutUtility.GetRect(0, 0, 50, 50);

		DrawDefaultInspector ();
		Handles.DrawSolidRectangleWithOutline(new Vector3[]{new Vector3(r.x, r.y, 0), new Vector3(r.x, r.y+r.height, 0), new Vector3(r.x+r.width, r.y+r.height, 0), new Vector3(r.x+r.width, r.y, 0)}, Color.gray, Color.black);
		lines = GraphDbg.GraphToSegments(name);
		GraphDbg.GraphArguments args = GraphDbg.GetGraphArguments(name);
		if (lines.Length>=2)
		{
			Vector3 minData, maxData;
			minData = lines[0];
			maxData = lines[0];
			for (int i=1; i<lines.Length; i++)
			{
				minData.x = Mathf.Min(lines[i].x, minData.x);
				minData.y = Mathf.Min(lines[i].y, minData.y);
				maxData.x = Mathf.Max(lines[i].x, maxData.x);
				maxData.y = Mathf.Max(lines[i].y, maxData.y);
			}
			float xDivider = 1;
			float yDivider = 1;
			if (maxData.x-minData.x>0)
				xDivider = maxData.x-minData.x;
			if (!args.fitX)
				xDivider = Mathf.Max(xDivider, args.logLimit);
			if (maxData.y-minData.y>0)
				yDivider = maxData.y-minData.y;
			bool isIncludeZero = true;
			if (isIncludeZero)
			{
				minData.y = Mathf.Min(minData.y, -yDivider*0.1f);
				maxData.y = Mathf.Max(maxData.y, yDivider*0.1f);
				if (maxData.y-minData.y>0)
					yDivider = maxData.y-minData.y;
				if (minData.y<=0 && maxData.y>=0)
				{
					float yZero = (1-((0-minData.y)/yDivider))*r.height+r.y;
					Handles.color = Color.black;
					Handles.DrawLine(new Vector2(r.x, yZero), new Vector2(r.width+r.x, yZero));
				}
			}
			//			xDivider = Mathf.Max(r.width*0.5f, xDivider);
			for (int i=0; i<lines.Length; i++)
			{
				Vector3 v = lines[i];
				v.x = ((v.x-minData.x)/xDivider)*r.width+r.x;
				v.y = (1-((v.y-minData.y)/yDivider))*r.height+r.y;
				lines[i] = v;
			}
			Handles.color = Color.green;
			Handles.DrawLines(lines);
		}
		EditorUtility.SetDirty( target );
		// Show default inspector property editor
	}
}