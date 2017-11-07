using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmplitudeUpdater : MonoBehaviour {
    [SerializeField]
    GenerateSound soundGenerator;
    [SerializeField]
    Text UiIndicator;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnAmplitudeSliderChange(float f)
    {
        UiIndicator.text = f.ToString();
        soundGenerator.amplitude = f;
    }
}
