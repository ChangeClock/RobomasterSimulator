using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoCustomScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void AddSample(float value)
	{
		GraphDbg.Log("CustomGraph", value, 100, true);
	}
}
