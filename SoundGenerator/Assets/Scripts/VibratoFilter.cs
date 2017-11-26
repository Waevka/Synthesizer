using FMOD;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VibratoFilter : FilterBase {
    public float delay; // o tyle sekund chcemy opoznic dzwiek
    public float depth;
    public float frequency; //LFO frequency
    [SerializeField]
    private GenerateSound musicCube;
    float[] currentDelayBuffer;
    int currentDelayBufferIndex;
    int maxDelayBufferIndex;
    float delayBufferReader; //modulowany przez LFO
    public bool delayBufferInitialized = false;
    float currentSampleRate;
    public int totalchannelofset;

    int totalSamples = 0;
    float position;
    [SerializeField]
    float offset;
    [SerializeField]
    float firstInterpolatedSample;
    int firstInterpolatedSampleIndex;
    [SerializeField]
    float secondInterpolatedSample;
    int secondInterpolatedSampleIndex;

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

        float sampleToReturn = sample;

        
        position = frequency * totalSamples / currentSampleRate;
        if(channelIndex == totalChannels-1) totalSamples++;
        offset = ((delay / 2.0f) * (1 + (float)GenerateSineSample(position - Math.Floor(position)) * depth)) * currentSampleRate;

        //wyrownujemy offset do odpowiedniego channelu - jesli jestesmy na 2 to nie mozemy wziac probki z 5!
        totalchannelofset = (int)offset%totalChannels;
        offset = offset - (offset % totalChannels) + channelIndex;
        

        if (offset >= maxDelayBufferIndex)
        {
            offset = maxDelayBufferIndex - 1;
        } else if (offset < 0)
        {
            offset = 0;
        }


        delayBufferReader = currentDelayBufferIndex - offset;

        //sprawdzamy poprawnosc indeksu, czy na pewno miesci sie w indeksach tablicy
        if(delayBufferReader >= 0)
        {
            if(delayBufferReader >= maxDelayBufferIndex)
            {
                delayBufferReader = delayBufferReader - maxDelayBufferIndex;
            }
        } else if (delayBufferReader < 0)
        {
            delayBufferReader = delayBufferReader + maxDelayBufferIndex;
        }

        //float sampleToReturn = sample;

        try
        {
            firstInterpolatedSampleIndex = (int)delayBufferReader;
            firstInterpolatedSample = currentDelayBuffer[firstInterpolatedSampleIndex];

            secondInterpolatedSampleIndex = ((int)delayBufferReader >= maxDelayBufferIndex - 1 - totalChannels ?
                0 + sampleIndex % totalChannels
                : (int)delayBufferReader + totalChannels);

            secondInterpolatedSample = currentDelayBuffer[secondInterpolatedSampleIndex];

            //interpolacja pomiedzy 2ma probkami
            float percent = delayBufferReader - (int)delayBufferReader;

            sampleToReturn = Mathf.Clamp(firstInterpolatedSample + ((secondInterpolatedSample-firstInterpolatedSample) * percent),
                -1.0f, 1.0f);
        } catch (NullReferenceException nre)
        {
            UnityEngine.Debug.Log(nre.Message);
        }
        currentDelayBuffer[currentDelayBufferIndex] = sample;
        currentDelayBufferIndex++;
        

        return sampleToReturn;
    }
    private void Awake()
    {
        filterIndex = 4;
        delay = 0.5f;
        depth = 0.5f;
        frequency = 3.0f;
    }
    // Use this for initialization
    void Start () {
		
	}
    private double GenerateSineSample(double position)
    {
        return (Math.Sin(position * Math.PI * 2));
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
