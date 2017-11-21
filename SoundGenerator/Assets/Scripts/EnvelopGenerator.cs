using UnityEngine;
using UnityEngine.UI;

public class EnvelopGenerator : FilterBase
{
    public Button generator;
    // Used as a scaler value to ensure output is in the range of [0, 1].
    private const float Ceiling = 0.63212f;
    // Used as a scaler to scale up time parameters.
    private const int TimeScaler = 1;//3;
    // Determines how the AdsrEnvelope responds to velocity.
    private float velocitySensitivity = 0;
    // Coefficients for calculating the AdsrEnvelope's output.
    private float a0, b1;

    private enum State
    {
        Attack,
        Decay,
        Release,
        Completed
    }

    // The AdsrEnvelope's current state.
    private State state = State.Completed;

    public float AValue { get; set; }        // The attack time.
    public float DValue { get; set; }        // The decay time.
    public float SValue { get; set; }        // The sustain level.
    public float RValue { get; set; }        // The release time
    public float amplitude { get { return musicCube.amplitude; } }       // The overall amplitude.
    public float sampleRate = 44100;      // The overall amplitude.

    [SerializeField]
    private GenerateSound musicCube;

    float output = 1.0f;

    void Start()
    {
        generator.onClick.AddListener(EnterRelease);
        SetIsFilterActive(false);
        AValue = 0.0f;
        DValue = 0.25f;
        SValue = 0.0f;
        RValue = 0.25f;
    }

    void Update()
    {
        Debug.Log("State: " + state);
    }

    public void StartAttack()
    {
        Debug.Log("Started Attack");
        StartAttack(0.0f);
    }

    void EnterRelease()
    {
        Debug.Log("Entered Release");
        Release(0.0f);
    }

    public override float ProcessSample(float sample, int sampleIndex, int channelIndex, float currentSampleRate, int totalChannels)
    {
        switch(state)
        {
            case State.Attack:
                output = a0 + b1 * output;

                // If the end of the attack segment has been reached.
                if (output >= Ceiling + 1.0f)
                {
                    //
                    // Calculate coefficients for decay segment.
                    //
                    float d = DValue * TimeScaler * currentSampleRate + 1.0f;
                    float x = (float)System.Math.Exp(-1.0f / d);

                    a0 = 1.0f - x;
                    b1 = x;

                    output = Ceiling + 1.0f;

                    // Enter decay segment.
                    state = State.Decay;
                }
                break;
            case State.Decay:
                output = a0 * SValue + b1 * output;
                break;
            case State.Release:
                output = a0 + b1 * output;

                // If the end of the release segment has been reached.
                if (output < 1.0f)
                {
                    output = 1.0f;

                    state = State.Completed;
                }
                break;
            case State.Completed:
                break;
            default:
                break;
        };

        return (output - 1.0f) / Ceiling * amplitude;
    }

    public void StartAttack(float velocity)
    {
        if ((state == State.Attack || state == State.Decay))
        {
            return;
        }

        //
        // Calculate coefficients for the attack segment.
        //
        float d = AValue * TimeScaler * sampleRate + 1.0f;
        float x = (float)System.Math.Exp(-1 / (AValue * TimeScaler * sampleRate));

        a0 = (1.0f - x) * 2.0f;
        b1 = x;

        //amplitude = (float)System.Math.Pow((1 - velocitySensitivity) + velocity * velocitySensitivity, 2);

        state = State.Attack;
    }

    public void Release(float velocity)
    {
        if (state == State.Release || state == State.Completed)
        {
            return;
        }

        //
        // Calculate coefficients for the release segment.
        //
        float d = RValue * TimeScaler * sampleRate + 1.0f;
        float x = (float)System.Math.Exp(-1 / d);

        a0 = (1.0f - x) * 0.9f;
        b1 = x;

        // Indicate that the AdsrEnvelope is in its release state.
        state = State.Release;
    }
}
