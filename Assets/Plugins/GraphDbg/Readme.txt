Hello and thank you for choosing GraphDbg as one of the debug and data visualization tools in your belt.

GraphDbg let's you plot values into a graph you can view in the editor.
You can add samples to GraphDbg every frame or only when an event occured, or as many times as you would like in a frame.

 Overview
===========

The scripts that should interese you as a developer are GraphDbg.cs, which has only global static functions to add samples to your graphs.
And GraphDbgComponent, which lets you add graph editor visualizers as components.
You can also view your graphs under Window->GraphDbg.

The folders structre is as follows:

- GraphDbg\
--- Demo\
----- DemoCustomScript.cs
----- GraphDbgDemo.unity
----- SliderDemoScript.cs
--- Editor\
----- GraphDbgInspector.cs
----- GraphDbgWindow.cs
--- Scripts\
----- GraphDbg.cs
----- GraphDbgComponent.cs
--- Readme.txt

 Demo
======
In order to try the demo you need to load the GraphDbgDemo scene.
You can then run the scene and open Window->GraphDbg.
In the GraphDbgWind you will see a visualizer of the default graph. You can then drag the Default Graph Slider in the GUI to see how it affects the graph.
You can also go in your scene structure and look for the slider under Canvas->Slider. It has a component that present the same graph visualizer.

In order to see the custom graph you can either view it from the component under Cavnas->ButtonArray. Or you can view it from the editor Window for GraphDbg and type the name of the graph which is CustomGraph.

 API
=====
The API is mostly inside GraphDbg.cs as static function calls.
The most important API calls are:
1) void Log(float value)
   This function lets you add samples to the default graph. Call it as many times as you like, the graph does not advance on it's own, it only advance every time you call this log.
   If you want the graph to advance every frame, you need to call this function every frame.
   example: GraphDbg.Log(12.5f);

2) void Log(string graphName, float value, int logLimit=logLimitDefault, bool isFitX = fixXDefault)
   This function is an overload of the Log function. If you specify a name in the graphName string, it will create and log the values for that specific graph from every function call that specify it's identifying name.
   value is the sample value.
   logLimit let's you decide what is the maximum samples your graph may store. The default value is 2000. Remember, you are limited by your available memory. It is possible that you will put a number that will cause the graph to consume a lot of memory.
   The entire graph is stored in memory.
   isFitX, default is fales. When set to false this will normalize the graph on the x axis according to the maximum samples specified in logLimit.
   If isFitX is true, it will normalize the x-axis according to the current amount of samples.

If you want a graph visualizer as a component, you need to add the GraphDbgComponent to your object. 
If you leave the graphName empty, it will visualize the first graph it finds. You can enter the name of the specific graph you want to visuzlie.