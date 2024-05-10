using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderDemoScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	float lastSliderValue = 0;
	// Update is called once per frame
	void FixedUpdate () {
		GraphDbg.Log(lastSliderValue);
	}

	public void AddSample(Slider slider)
	{
		lastSliderValue = slider.value;
	}
}
