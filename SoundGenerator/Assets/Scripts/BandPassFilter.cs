using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BandPassFilter : FilterBase
{
    public float sampleRate = 44100;      // The overall amplitude.
    public float centreFrequency { get; set; }
    public float q { get; set; }
    double a0, a1, a2, a3, a4;
    // state
    private float x1;
    private float x2;
    private float y1;
    private float y2;

    private void Awake()
    {
        centreFrequency = 1000;
        q = 0.5f;
        filterIndex = 6;
        a0 = 0.0;
        a1 = 0.0;
        a2 = 0.0;
        a3 = 0.0;
        a4 = 0.0;
        x1 = 0.0f;
        x2 = 0.0f;
        y1 = 0.0f;
        y2 = 0.0f;
    }

    public override float ProcessSample(float sample, int sampleIndex, int channelIndex, float currentSampleRate, int totalChannels)
    {
        // compute result
        var result = a0 * sample + a1 * x1 + a2 * x2 - a3 * y1 - a4 * y2;

        // shift x1 to x2, sample to x1 
        x2 = x1;
        x1 = sample;

        // shift y1 to y2, result to y1 
        y2 = y1;
        y1 = (float)result;

        return y1;
    }

    public void BandPassFilterConstantPeakGain()
    {
        // H(s) = (s/Q) / (s^2 + s/Q + 1)      (constant 0 dB peak gain)
        var w0 = 2.0d * System.Math.PI * centreFrequency / sampleRate;
        var cosw0 = System.Math.Cos(w0);
        var sinw0 = System.Math.Sin(w0);
        var alpha = sinw0 / (2 * q);

        var b0 = alpha;
        var b1 = 0.0d;
        var b2 = -alpha;
        var a0 = 1.0d + alpha;
        var a1 = -2.0d * cosw0;
        var a2 = 1.0d - alpha;

        SetCoefficients(a0, a1, a2, b0, b1, b2);
    }

    private void SetCoefficients(double aa0, double aa1, double aa2, double b0, double b1, double b2)
    {
        // precompute the coefficients
        a0 = b0 / aa0;
        a1 = b1 / aa0;
        a2 = b2 / aa0;
        a3 = aa1 / aa0;
        a4 = aa2 / aa0;
    }
}
