using FMOD;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelicopterFilter : FilterBase
{

    public float CutOffFrequency { get; set; }
    enum HelicopterRotor { First, Second }
    public float QValue { get; set; }
    [SerializeField]
    GenerateSound musicCube;
    private System.Random rand;
    public float firstRotorPosition;
    public float secondRotorPosition;
    private float totalSamples;
    private float frequency; //LFO
    private bool coefficientsInitialized = false;
    public float currentSampleRate;
    public float delay;
    float[] currentDelayBuffer;
    public int currentDelayBufferIndex;
    public int maxDelayBufferIndex;
    public float delayBufferReader;
    public float offset;
    float depth;

    float[] xn, yn, xn1, xn2, yn1, yn2;
    float s, c, alpha, r, a0, a1, a2, b1, b2;

    public override float ProcessSample(float sample, int sampleIndex, int channelIndex, float currentSampleRate, int totalChannels)
    {   
        //if(channelIndex == 0 || channelIndex == 1)
        {
            firstRotorPosition = frequency * totalSamples / currentSampleRate;
            secondRotorPosition = firstRotorPosition + 0.5f;

            if (channelIndex == totalChannels - 1) totalSamples++;
            float s = WhiteNoise() * 0.3f;
            s = LowPassFilter(s, sampleIndex, channelIndex, currentSampleRate, totalChannels);
            float s1 = Delay(s, HelicopterRotor.First, sampleIndex, channelIndex, currentSampleRate, totalChannels);
            float s2 = Delay(s, HelicopterRotor.Second, sampleIndex, channelIndex, currentSampleRate, totalChannels);

            return (s1 * 0.5f) + (s2 * 0.5f);
        }
        //else { return sample; }
    }
    private double GenerateSineSample(double position)
    {
        return (Math.Sin(position * Math.PI * 2));
    }

    private float WhiteNoise()
    {
        return (float)(rand.NextDouble() * 2.0d - 1.0d);
    }

    private float LowPassFilter(float sample, int sampleIndex, int channelIndex, float currentSampleRate, int totalChannels)
    {
        if (!coefficientsInitialized)
        {
            initCoefficients(totalChannels);
        }
        //przesuwamy poprzednie probki o 1 do tylu
        xn2[channelIndex] = xn1[channelIndex];
        xn1[channelIndex] = xn[channelIndex];

        yn2[channelIndex] = yn1[channelIndex];
        yn1[channelIndex] = yn[channelIndex];

        //obecne probki
        xn[channelIndex] = sample;

        s = Mathf.Sin((2 * Mathf.PI * CutOffFrequency) / (currentSampleRate));
        c = Mathf.Cos((2 * Mathf.PI * CutOffFrequency) / (currentSampleRate));
        alpha = s / (2 * QValue);
        r = (1 / (1 + alpha));

        a0 = 0.5f * (1 - c) * r;
        a1 = (1 - c) * r;
        a2 = a0;
        b1 = -2 * c * r;
        b2 = (1 - alpha) * r;

        yn[channelIndex] = (a0 * xn[channelIndex]) + (a1 * xn1[channelIndex]) + (a2 * xn2[channelIndex])
            - (b1 * yn1[channelIndex]) - (b2 * yn2[channelIndex]);
        yn[channelIndex] = Mathf.Clamp(yn[channelIndex], -1.0f, 1.0f);
        //yn[channelIndex] *= 32767.0f;
        //yn[channelIndex] = sample * 0.2f;

        return yn[channelIndex];
    }

    private float Delay(float sample, HelicopterRotor rotor, int sampleIndex, int channelIndex, float currentSampleRate, int totalChannels)
    {
        if (currentDelayBufferIndex >= maxDelayBufferIndex)
        {
            currentDelayBufferIndex = 0;
        }

        if(rotor == HelicopterRotor.First)
        {
            offset = (delay / 2.0f) * (1 + (float)GenerateSineSample(firstRotorPosition - Math.Floor(firstRotorPosition)) * depth) * currentSampleRate;
        } 
        else if (rotor == HelicopterRotor.Second)
        {
            offset = (delay / 2.0f) * (1 + (float)GenerateSineSample(secondRotorPosition - Math.Floor(secondRotorPosition)) * depth) * currentSampleRate;
        }


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
        if (delayBufferReader >= 0)
        {
            if (delayBufferReader >= maxDelayBufferIndex)
            {
                delayBufferReader = delayBufferReader - maxDelayBufferIndex;
            }
        }
        else if (delayBufferReader < 0)
        {
            delayBufferReader = delayBufferReader + maxDelayBufferIndex;
        }

        float sampleToReturn = sample;

        try
        {
            int firstInterpolatedSampleIndex = (int)delayBufferReader;
            float firstInterpolatedSample = currentDelayBuffer[firstInterpolatedSampleIndex];

            int secondInterpolatedSampleIndex = ((int)delayBufferReader >= maxDelayBufferIndex - 1 - totalChannels ?
                0 + sampleIndex % totalChannels
                : (int)delayBufferReader + totalChannels);

            float secondInterpolatedSample = currentDelayBuffer[secondInterpolatedSampleIndex];

            //interpolacja pomiedzy 2ma probkami
            float percent = delayBufferReader - (int)delayBufferReader;

            sampleToReturn = Mathf.Clamp(firstInterpolatedSample + (secondInterpolatedSample - firstInterpolatedSample) * percent,
                -1.0f, 1.0f);
        }
        catch (NullReferenceException nre)
        {
            UnityEngine.Debug.Log(nre.Message);
        }
        currentDelayBuffer[currentDelayBufferIndex] = sample;
        if(rotor == HelicopterRotor.Second) currentDelayBufferIndex++;

        return sampleToReturn;
    }

    // Use this for initialization
    void Start () {
		
	}
    private void Awake()
    {
        filterIndex = 8;
        rand = new System.Random();
        frequency = 5.0f;
        CutOffFrequency = 300;
        QValue = 5.0f;
        delay = 0.5f;
        depth = 0.8f;
    }

    // Update is called once per frame
    void Update () {
		
	}
    public override void SetIsFilterActive(bool enabled)
    {
        base.SetIsFilterActive(enabled);
        if (!enabled)
        {
            coefficientsInitialized = false;
        }
    }
    private void initCoefficients(int inchannels)
    {
        //inicjalizujemy tablice
        xn = new float[inchannels];
        yn = new float[inchannels];
        xn1 = new float[inchannels];
        xn2 = new float[inchannels];
        yn1 = new float[inchannels];
        yn2 = new float[inchannels];

        Channel c;
        musicCube.getChannelGroup().getChannel(0, out c);
        c.getFrequency(out currentSampleRate);

        currentDelayBufferIndex = 0;
        maxDelayBufferIndex = Mathf.RoundToInt(Mathf.Ceil(currentSampleRate * delay));
        currentDelayBuffer = new float[maxDelayBufferIndex];
        UnityEngine.Debug.Log("Size of vibrato delay buffer in samples: " + maxDelayBufferIndex);
        
        coefficientsInitialized = true;
    }
}
