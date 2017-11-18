using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FilterBase : MonoBehaviour {
    public bool isActive;
    public int filterIndex;

    public abstract float ProcessSample(float sample, int sampleIndex, int channelIndex,
        float currentSampleRate, int totalChannels);

    public virtual void SetIsFilterActive(bool enabled)
    {
        isActive = enabled;
    }
    public virtual bool IsFilterActive()
    {
        return isActive;
    }
}
