using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD;
using System;
using UnityEngine.UI;
public class Instrument : MonoBehaviour
{
    [SerializeField]
    public SortedList<int, FilterBase> filters;

    FMOD.DSP DSPfilter;
    FMOD.DSP_DESCRIPTION description;
    FMOD.DSP DSPfilter2;
    FMOD.DSP_DESCRIPTION description2;
    [SerializeField]
    public GenerateSound musicCube;
    [SerializeField]
    float[] buffer = new float[1];
    float[] buffer2 = new float[1];
    [SerializeField]
    float currentSampleRate;
    [SerializeField]
    float singleSampleRange; //jaki zakres Hz ma w sobie jedna probka

    WahWahFilter wahwah;
    public EnvelopGenerator envelope;
    LowPassFilter lowpass;
    HighPassFilter highpass;
    BandPassFilter bandpass;
    VibratoFilter vibrato;
    public Button btn;

    bool initialized = false;

    private void Awake()
    {
    }

    private void Start()
    {
        //jeden kanal
        wahwah = musicCube.GetComponent<WahWahFilter>();
        wahwah.QValue = 0.5f;
        wahwah.CutOffFrequency = 300.0f;
        wahwah.FrequencyRange = 00.0f;
        lowpass = musicCube.GetComponent<LowPassFilter>();
        lowpass.CutOffFrequency = 400.0f;
        lowpass.QValue = 0.5f;


        //drugi kanal
        envelope = musicCube.GetComponent<EnvelopGenerator>();
        envelope.AttackRate = 0.15f * 44100.0f;
        envelope.DecayRate = 0.15f * 44100.0f;
        envelope.SustainLevel = 0.6f;
        envelope.ReleaseRate = 0.1f * 44100.0f;
        highpass = musicCube.GetComponent<HighPassFilter>();
        highpass.QValue = 0.5f;
        highpass.CutOffFrequency = 300.0f;
        bandpass = musicCube.GetComponent<BandPassFilter>();
        bandpass.centreFrequency = 350.0f;
        bandpass.q = 0.4f;
        bandpass.BandPassFilterConstantPeakGain();
        //vibrato = musicCube.GetComponent<VibratoFilter>();
        //vibrato.delay = 0.1f;
    }


    public void InitFilterApplier()
    {
        if (initialized)
            return;

        //musimy zainicjalizowac tutaj, po rozpoczeciu odgrywania dzwieku w channelGroupie
        //inaczej blad
        description = new FMOD.DSP_DESCRIPTION();
        description.read = myDSPCallback;
        FMODUnity.RuntimeManager.LowlevelSystem.createDSP(ref description, out DSPfilter);
        DSPfilter.setBypass(false);
        FMOD.RESULT r = musicCube.getChannelGroup().addDSP(FMOD.CHANNELCONTROL_DSP_INDEX.HEAD, DSPfilter);
        UnityEngine.Debug.Log(r);
        Channel c;
        musicCube.getChannelGroup().getChannel(0, out c);
        c.getFrequency(out currentSampleRate);

        description2 = new FMOD.DSP_DESCRIPTION();
        description2.read = myDSPCallback2;
        FMODUnity.RuntimeManager.LowlevelSystem.createDSP(ref description2, out DSPfilter2);
        DSPfilter2.setBypass(false);
        FMOD.RESULT r2 = musicCube.getChannelGroupForInstrument().addDSP(FMOD.CHANNELCONTROL_DSP_INDEX.HEAD, DSPfilter2);
        UnityEngine.Debug.Log(r2);
        Channel c2;
        musicCube.getChannelGroupForInstrument().getChannel(0, out c2);
        c2.getFrequency(out currentSampleRate);

        initialized = true;
    }

    private RESULT myDSPCallback(ref DSP_STATE dsp_state, IntPtr inbuffer, IntPtr outbuffer, uint length, int inchannels, ref int outchannels)
    {
        buffer = new float[length * inchannels];
        singleSampleRange = ((currentSampleRate / 2) / length); // nyquist
        float currentSample;

        int i = 0;
        try
        {
            for (i = 0; i < length * inchannels; i++)
            {
                //test dostepu pamieci
                var currentPtr = new IntPtr(inbuffer.ToInt32() + (i * sizeof(float)));
                currentSample = (float)System.Runtime.InteropServices.Marshal.PtrToStructure(
                currentPtr, typeof(float));

                float sampleOutput;

                sampleOutput = wahwah.ProcessSample(currentSample, (i / inchannels), (i % inchannels), currentSampleRate, inchannels);
                sampleOutput = lowpass.ProcessSample(sampleOutput, (i / inchannels), (i % inchannels), currentSampleRate, inchannels);

                buffer[i] = sampleOutput;
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

        return RESULT.OK;
    }


    private RESULT myDSPCallback2(ref DSP_STATE dsp_state, IntPtr inbuffer, IntPtr outbuffer, uint length, int inchannels, ref int outchannels)
    {
        buffer = new float[length * inchannels];
        singleSampleRange = ((currentSampleRate / 2) / length); // nyquist
        float currentSample;

        int i = 0;
        try
        {
            for (i = 0; i < length * inchannels; i++)
            {
                //test dostepu pamieci
                var currentPtr = new IntPtr(inbuffer.ToInt32() + (i * sizeof(float)));
                currentSample = (float)System.Runtime.InteropServices.Marshal.PtrToStructure(
                currentPtr, typeof(float));

                float sampleOutput;

                sampleOutput = bandpass.ProcessSample(
                            currentSample, (i / inchannels), (i % inchannels), currentSampleRate, inchannels);
                sampleOutput = highpass.ProcessSample(
                            sampleOutput, (i / inchannels), (i % inchannels), currentSampleRate, inchannels);
                //sampleOutput = vibrato.ProcessSample(
                //            sampleOutput, (i / inchannels), (i % inchannels), currentSampleRate, inchannels);


                buffer[i] = sampleOutput;
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

        return RESULT.OK;
    }

}
