using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighPassFilterUpdater : MonoBehaviour {
    [SerializeField]
    HighPassFilter highPassFilter;
    [SerializeField]
    Text QValueText;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void highPassFreqUpdater(string s)
    {
        Debug.Log(s);
        int freqToSet;
        if (!Int32.TryParse(s, out freqToSet))
        {
            freqToSet = 800;
        }
        if (freqToSet <= 0)
        {
            freqToSet = 1;
        }
        else if (freqToSet > 20000)
        {
            freqToSet = 20000;
        }
        highPassFilter.CutOffFrequency = freqToSet;
    }

    public void highPassIsEnabledUpdater(bool b)
    {
        Debug.Log("Updating high pass filter state, is enabled: " + b);
        highPassFilter.SetIsFilterActive(b);
    }

    public void QValueUpdater(float q)
    {
        QValueText.text = q.ToString();
        highPassFilter.QValue = q;
    }
}
