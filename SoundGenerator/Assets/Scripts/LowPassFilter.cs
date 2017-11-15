using System;
using System.Collections;
using System.Collections.Generic;
using FMOD;
using UnityEngine;

public class LowPassFilter : MonoBehaviour {
    public float CutOffFrequency { get; set; }
    public float QValue { get; set; }
    FMOD.DSP lowPassFilterDSP;
    FMOD.DSP_DESCRIPTION description;
    [SerializeField]
    private GenerateSound musicCube;
    [SerializeField]
    float[] buffer = new float[1];
    [SerializeField]
    float currentSampleRate;
    [SerializeField]
    float singleSampleRange; //jaki zakres Hz ma w sobie jedna prowka

    //probki
    [SerializeField]
    bool coefficientsInitialized = false;
    float[] xn, yn, xn1, xn2, yn1, yn2;
    float s, c, alpha, r, a0, a1, a2, b1, b2;
    [SerializeField]
    float totalMax, totalMin;
    [SerializeField]
    float totalMaxBuff, totalMinBuff;

    // Use this for initialization
    void Start() {
        CutOffFrequency = 1000;
        QValue = 0.5f;

        description = new FMOD.DSP_DESCRIPTION();
        description.read = myDSPCallback;
        FMODUnity.RuntimeManager.LowlevelSystem.createDSP(ref description, out lowPassFilterDSP);
        musicCube.getChannelGroup().addDSP(FMOD.CHANNELCONTROL_DSP_INDEX.TAIL, lowPassFilterDSP);
        SetIsFilterActive(false);
    }

    public float processSample(float sample, int sampleIndex, int channelIndex)
    {   
        float sampleFrequencyRange = sampleIndex * singleSampleRange;

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
        if (yn[channelIndex] <= totalMin) totalMin = yn[channelIndex];
        if (yn[channelIndex] >= totalMax) totalMax = yn[channelIndex];
        //yn[channelIndex] *= 32767.0f;
        //yn[channelIndex] = sample * 0.2f;

        return yn[channelIndex];
    }

    // Update is called once per frame
    void Update () {
		
	}

    public bool IsFilterActive()
    {
        bool isBypassEnabled;
        lowPassFilterDSP.getBypass(out isBypassEnabled);
        return !isBypassEnabled;
    }

    public void SetIsFilterActive(bool enabled)
    {
        lowPassFilterDSP.setBypass(!enabled);
        if (enabled)
        {
            Channel c;
            musicCube.getChannelGroup().getChannel(0, out c);
            c.getFrequency(out currentSampleRate);
            coefficientsInitialized = false;
        }
    }

    private RESULT myDSPCallback(ref DSP_STATE dsp_state, IntPtr inbuffer, IntPtr outbuffer, uint length, int inchannels, ref int outchannels)
    {
        buffer = new float[length * inchannels];
        singleSampleRange = ((currentSampleRate / 2) / length); // nyquist
        float currentSample;

        if (!coefficientsInitialized)
        {
            initCoefficients(inchannels);
        }

        int i = 0;
        try
        {
            for (i = 0; i < length * inchannels; i++)
            {
            
                //test dostepu pamieci
                var currentPtr = new IntPtr(inbuffer.ToInt32() + (i * sizeof(float)));
                currentSample = (float)System.Runtime.InteropServices.Marshal.PtrToStructure(
                currentPtr, typeof(float));
                float bt = processSample(currentSample, (i/inchannels), (i%inchannels));
                buffer[i] = bt; // test - zmniejszamy głośność

                if (currentSample <= totalMinBuff) totalMinBuff = currentSample;
                if (currentSample >= totalMaxBuff) totalMaxBuff = currentSample;
            }

            //kopiujemy caly bufor do wskaznika data
            System.Runtime.InteropServices.Marshal.Copy(buffer, 0, outbuffer, (int)length * inchannels);
            outchannels = inchannels;
        }
        catch (NullReferenceException nre)
        {
            UnityEngine.Debug.Log("Samples broke at sample:" + i);
            throw new NullReferenceException(nre.Message);
        }

        catch (IndexOutOfRangeException ioore)
        {
            UnityEngine.Debug.Log("Samples broke at sample:" + i);
            throw new NullReferenceException(ioore.Message);
        }
        //test
        /*float[] BUFFTEST = new float[length / sizeof(float)]; //Do przegladania w visualu


        for (i = 0; i < length / sizeof(float); i++)
        {
            var currentPtr = new IntPtr(outbuffer.ToInt32() + (i * sizeof(float)));
            BUFFTEST[i] = (float)System.Runtime.InteropServices.Marshal.PtrToStructure(
                currentPtr, typeof(float));
        }*/

        return RESULT.OK;
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
        coefficientsInitialized = true;
    }
}
