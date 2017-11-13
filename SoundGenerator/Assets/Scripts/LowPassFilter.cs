using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LowPassFilter : MonoBehaviour {
    public bool IsFilterActive { get; set; }
    public int CutOffFrequency { get; set; }
    public float QValue { get; set; }

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
}
