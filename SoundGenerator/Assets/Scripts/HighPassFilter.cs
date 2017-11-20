using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighPassFilter : FilterBase {
    public float CutOffFrequency { get; set; }
    public float QValue { get; set; }

    [SerializeField]
    private GenerateSound musicCube;
    [SerializeField]
    float totalMax, totalMin;
    //probki
    [SerializeField]
    bool coefficientsInitialized = false;
    float[] xn, yn, xn1, xn2, yn1, yn2;
    float s, c, alpha, r, a0, a1, a2, b1, b2;

    private void Awake()
    {
        filterIndex = 1;
    }

    public override float ProcessSample(float sample, int sampleIndex, int channelIndex, float currentSampleRate, int totalChannels)
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

        a0 = 0.5f * (1 + c) * r;
        a1 = -(1 + c) * r;
        a2 = a0;
        b1 = -2 * c * r;
        b2 = (1 - alpha) * r;

        yn[channelIndex] = (a0 * xn[channelIndex]) + (a1 * xn1[channelIndex]) + (a2 * xn2[channelIndex])
            - (b1 * yn1[channelIndex]) - (b2 * yn2[channelIndex]);
        yn[channelIndex] = Mathf.Clamp(yn[channelIndex], -1.0f, 1.0f);
        if (yn[channelIndex] <= totalMin) totalMin = yn[channelIndex];
        if (yn[channelIndex] >= totalMax) totalMax = yn[channelIndex];

        return yn[channelIndex];
    }

    // Use this for initialization
    void Start () {
        CutOffFrequency = 1000;
        QValue = 0.5f;
        SetIsFilterActive(false);
    }

    // Update is called once per frame
    void Update()
    {

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
        coefficientsInitialized = true;
    }
}
