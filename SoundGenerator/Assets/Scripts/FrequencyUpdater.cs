using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrequencyUpdater : MonoBehaviour {
    [SerializeField]
    private GenerateSound soundGenerator;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetUserFrequency(string frequency)
    {
        Debug.Log(frequency);
        int freqToSet;
        if (!Int32.TryParse(frequency, out freqToSet))
        {
            freqToSet = 800;
        }
        if(freqToSet <= 0)
        {
            freqToSet = 1;
        } else if(freqToSet > 20000)
        {
            freqToSet = 20000;
        }
        soundGenerator.frequency = freqToSet;
    }

    public int GetUserFrequency()
    {
        return soundGenerator.frequency;
    }
}
