using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LowPassFilterUpdater : MonoBehaviour {
    [SerializeField]
    LowPassFilter lowPassFilter;
    [SerializeField]
    Text QValueText;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void lowPassFreqUpdater(string s)
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
        lowPassFilter.CutOffFrequency = freqToSet;
    }

    public void lowPassIsEnabledUpdater(bool b)
    {
        Debug.Log("Updating low pass filter state, is enabled: " + b);
        lowPassFilter.SetIsFilterActive(b);
    }

    public void QValueUpdater(float q)
    {
        QValueText.text = q.ToString();
        lowPassFilter.QValue = q;
    }
}
