using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BandPassFilterUpdater : MonoBehaviour
{
    public BandPassFilter filter;
    public Text qText;
    // Use this for initialization
    void Start()
    {
        filter.centreFrequency = 1000;
        filter.q = 0.5f;
        qText.text = 0.5f.ToString("F2");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void IsEnabled(bool enable)
    {
        Debug.Log("Updating all pass filter state, is enabled: " + enable);
        filter.SetIsFilterActive(enable);
        filter.BandPassFilterConstantPeakGain();
    }

    public void FrequencyUpdateer(string s)
    {
        Debug.Log(s);
        int freqToSet;
        if (!System.Int32.TryParse(s, out freqToSet))
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
        filter.centreFrequency = freqToSet;
        filter.BandPassFilterConstantPeakGain();
    }

    public void QValueUpdater(float q)
    {
        qText.text = q.ToString("F2");
        filter.q = q;
        filter.BandPassFilterConstantPeakGain();
    }
}
