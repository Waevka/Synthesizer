using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD;
using System;

public class FilterApplier : MonoBehaviour {

    private SortedList<int, FilterBase> filters;

    FMOD.DSP DSPfilter;
    FMOD.DSP_DESCRIPTION description;
    [SerializeField]
    private GenerateSound musicCube;
    [SerializeField]
    float[] buffer = new float[1];
    [SerializeField]
    float currentSampleRate;
    [SerializeField]
    float singleSampleRange; //jaki zakres Hz ma w sobie jedna probka

    [SerializeField]
    float totalMaxBuff, totalMinBuff; //debug fields

    // Use this for initialization
    void Start () {
    }

    public void InitFilterApplier()
    {
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

                //Dla tylko jednego filtru - przerabiamy probke i oddajemy
                if (filters.Count > 0 && filters[0].isActive)
                {
                    sampleOutput = filters[0].ProcessSample(
                        currentSample, (i / inchannels), (i % inchannels), currentSampleRate, inchannels);
                } else
                {
                    sampleOutput = currentSample;
                }
                //Gdy filtrow jest wiecej, nakladamy kazdy po kolei
                if(filters.Count > 1)
                {
                    foreach(KeyValuePair<int, FilterBase> filter in filters){

                        sampleOutput = (filter.Value.isActive ? filter.Value.ProcessSample(
                            sampleOutput, (i / inchannels), (i % inchannels), currentSampleRate, inchannels)
                            : sampleOutput);
                    }
                }

                buffer[i] = sampleOutput;

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

    // Update is called once per frame
    void Update () {
		
	}

    public void AddFilter(FilterBase filter)
    {
        if (filters == null)
        {
            filters = new SortedList<int, FilterBase>();
        }
        filters.Add(filter.filterIndex, filter);
    }

}
