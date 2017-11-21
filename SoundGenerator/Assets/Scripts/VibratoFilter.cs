using FMOD;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VibratoFilter : FilterBase {
    public float delay; // o tyle sekund chcemy opoznic dzwiek
    [SerializeField]
    private GenerateSound musicCube;
    float[] currentDelayBuffer;
    int currentDelayBufferIndex;
    int maxDelayBufferIndex;
    public bool delayBufferInitialized = false;
    float currentSampleRate;

    public override float ProcessSample(float sample, int sampleIndex, int channelIndex, float currentSampleRate, int totalChannels)
    {
        if (!delayBufferInitialized)
        {
            initCoefficients(totalChannels);
        }

        //dodajemy probki do bufora az osiagniemy pewne opoznienie
        if(currentDelayBufferIndex >= maxDelayBufferIndex)
        {
            currentDelayBufferIndex = 0;
        }

        float sampleToReturn = Mathf.Clamp(sample + currentDelayBuffer[currentDelayBufferIndex], -1.0f, 1.0f);
        currentDelayBuffer[currentDelayBufferIndex] = sample;
        currentDelayBufferIndex++;

        return sampleToReturn;
    }
    private void Awake()
    {
        filterIndex = 4;
        delay = 0.5f;
    }
    // Use this for initialization
    void Start () {
		
	}
    private double GenerateSineSample(double position)
    {
        return (Math.Sin(position * Math.PI * 2) + 1.0f) / 2.0f;
    }

    // Update is called once per frame
    void Update () {

    }
    public override void SetIsFilterActive(bool enabled)
    {
        base.SetIsFilterActive(enabled);
        if (!enabled)
        {
            delayBufferInitialized = false;
        }
    }

    private void initCoefficients(int inchannels)
    {
        Channel c;
        musicCube.getChannelGroup().getChannel(0, out c);
        c.getFrequency(out currentSampleRate);

        currentDelayBufferIndex = 0;
        maxDelayBufferIndex = Mathf.RoundToInt(Mathf.Ceil(currentSampleRate * delay));
        currentDelayBuffer = new float[maxDelayBufferIndex];
        UnityEngine.Debug.Log("Size of vibrato delay buffer in samples: " + maxDelayBufferIndex);

        delayBufferInitialized = true;
    }
}
