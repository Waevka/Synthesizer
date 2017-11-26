using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VibratoFilterUpdater : MonoBehaviour {
    [SerializeField]
    VibratoFilter vibratoFilter;
    [SerializeField]
    Text delayText;
    [SerializeField]
    Text depthText;
    [SerializeField]
    Text freqText;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

    }

    public void VibratoIsEnabledUpdater(bool b)
    {
        Debug.Log("Updating Vibrato filter state, is enabled: " + b);
        vibratoFilter.SetIsFilterActive(b);
    }

    public void DelayValueUpdater(float d)
    {
        vibratoFilter.delay = d;
        vibratoFilter.delayBufferInitialized = false;
        if(delayText != null)
        {
            delayText.text = d.ToString();
        }
    }

    public void DepthValueUpdater(float d)
    {
        vibratoFilter.depth = d;
        if (depthText != null)
        {
            depthText.text = (d*100).ToString() + "%";
        }
    }
    public void FreqValueUpdater(float d)
    {
        vibratoFilter.frequency = d;
        if (freqText != null)
        {
            freqText.text = d.ToString();
        }
    }
}
