using System;
using System.Collections;
using System.Collections.Generic;
using FMOD;
using UnityEngine;

public class LowPassFilter : MonoBehaviour {
    public int CutOffFrequency { get; set; }
    public float QValue { get; set; }
    FMOD.DSP lowPassFilterDSP;
    FMOD.DSP_DESCRIPTION description;
    [SerializeField]
    private GenerateSound musicCube;
    [SerializeField]
    float[] buffer = new float[1];

     //probki
    [SerializeField]
    double xn, yn, xn1, xn2, yn1, yn2;
    float s, c, alpha, r, a0, a1, a2, b1, b2;

    // Use this for initialization
    void Start() {
        CutOffFrequency = 1000;
        xn = 0;
        yn = 0;
        xn1 = 0;
        xn2 = 0;
        yn1 = 0;
        yn2 = 0;
        QValue = 0.5f;

        description = new FMOD.DSP_DESCRIPTION();
        description.read = myDSPCallback;
        FMODUnity.RuntimeManager.LowlevelSystem.createDSP(ref description, out lowPassFilterDSP);
        musicCube.getChannelGroup().addDSP(FMOD.CHANNELCONTROL_DSP_INDEX.HEAD, lowPassFilterDSP);
    }

    public double processSample(double sample, int frequency)
    {
        //przesuwamy poprzednie probki o 1 do tylu
        xn2 = xn1;
        xn1 = xn;

        yn2 = yn1;
        yn1 = yn;

        //obecne probki
        xn = sample;

        if (frequency > CutOffFrequency || xn2 == 0)
        {
            s = Mathf.Sin((float)(2 * Mathf.PI * CutOffFrequency) / (float)(frequency));
            c = Mathf.Cos((float)(2 * Mathf.PI * CutOffFrequency) / (float)(frequency));
            alpha = s / (float)(2 * QValue);
            r = (float)(1 / (1 + alpha));

            a0 = 0.5f * (1 - c) * r;
            a1 = (1 - c) * r;
            a2 = a0;
            b1 = -2 * c * r;
            b2 = (1 - alpha) * r;

            yn = (a0 * xn) + (a1 * xn1) + (a2 * xn2) - (b1 * yn1) - (b2 * yn2);

        } else
        {
            yn = sample;
        }

        return yn;
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
    }

    private RESULT myDSPCallback(ref DSP_STATE dsp_state, IntPtr inbuffer, IntPtr outbuffer, uint length, int inchannels, ref int outchannels)
    {
        buffer = new float[length / sizeof(float)];
        int i = 0;

        for (i = 0; i < length / sizeof(float); i++)
        {
            try
            {
                //test dostepu pamieci
                var currentPtr = new IntPtr(inbuffer.ToInt32() + (i * sizeof(float)));
                buffer[i] = (float)System.Runtime.InteropServices.Marshal.PtrToStructure(
                currentPtr, typeof(float));
                //buffer[i] *= 0.5f;

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
        }

        //kopiujemy caly bufor do wskaznika data
        System.Runtime.InteropServices.Marshal.Copy(buffer, 0, outbuffer, (int)(length / sizeof(float)));

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
}
