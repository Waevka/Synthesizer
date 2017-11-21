using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WahWahFilterUpdater : MonoBehaviour {
    [SerializeField]
    WahWahFilter wahWahFilter;
    [SerializeField]
    Text FreqRangeText;
    [SerializeField]
    Text FreqText;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public void WahWahBaseFreqUpdater(string s)
    {
        Debug.Log(s);
        int freqToSet;
        if (!Int32.TryParse(s, out freqToSet))
        {
            freqToSet = 300;
        }
        if (freqToSet <= 0)
        {
            freqToSet = 1;
        }
        else if (freqToSet > 20000)
        {
            freqToSet = 20000;
        }
        wahWahFilter.CutOffFrequency = freqToSet;
    }
    public void WahWahFreqRangeUpdater(string s)
    {   
        int freqToSet;
        if (!Int32.TryParse(s, out freqToSet))
        {
            freqToSet = 300;
        }
        if (freqToSet <= 0)
        {
            freqToSet = 1;
            FreqRangeText.text = s;
        }
        else if (freqToSet > 20000 ||
            wahWahFilter.CutOffFrequency + freqToSet >= 20000)
        {
            freqToSet = 19999 - (int)wahWahFilter.CutOffFrequency;
            FreqRangeText.text = freqToSet.ToString();
        }
        Debug.Log(freqToSet);
        wahWahFilter.FrequencyRange = freqToSet;
    }

    public void WahWahIsEnabledUpdater(bool b)
    {
        Debug.Log("Updating WahWah filter state, is enabled: " + b);
        wahWahFilter.SetIsFilterActive(b);
    }
    public void FrequencyValueUpdater(float q)
    {
        FreqText.text = q.ToString();
        wahWahFilter.Frequency = q;
    }
}
