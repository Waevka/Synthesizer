using UnityEngine;
using UnityEngine.UI;

public class EnvelopGenerator : FilterBase
{
    public Button generator;
    private float output;
    private float attackCoef;
    private float decayCoef;
    private float releaseCoef;
    private float targetRatioAttack;
    private float targetRatioDecayRelease;
    private float attackBase;
    private float decayBase;
    private float releaseBase;

    public enum EnvelopeState
    {
        Idle = 0,
        Attack,
        Decay,
        Sustain,
        Release
    };

    // The AdsrEnvelope's current state.
    private EnvelopeState state = EnvelopeState.Idle;

    public float attackRate { get; set; }        // The attack time.
    public float decayRate { get; set; }        // The decay time.
    public float sustainLevel { get; set; }        // The sustain level.
    public float releaseRate { get; set; }        // The release time
    public float amplitude { get { return musicCube.amplitude; } }       // The overall amplitude.
    public float sampleRate = 44100;      // The overall amplitude.

    [SerializeField]
    private GenerateSound musicCube;

    /// <summary>
    /// Attack Rate (seconds * SamplesPerSecond)
    /// </summary>
    public float AttackRate
    {
        get
        {
            return attackRate;
        }
        set
        {
            attackRate = value;
            attackCoef = CalcCoef(value, targetRatioAttack);
            attackBase = (1.0f + targetRatioAttack) * (1.0f - attackCoef);
        }
    }

    /// <summary>
    /// Decay Rate (seconds * SamplesPerSecond)
    /// </summary>
    public float DecayRate
    {
        get
        {
            return decayRate;
        }
        set
        {
            decayRate = value;
            decayCoef = CalcCoef(value, targetRatioDecayRelease);
            decayBase = (sustainLevel - targetRatioDecayRelease) * (1.0f - decayCoef);
        }
    }

    /// <summary>
    /// Release Rate (seconds * SamplesPerSecond)
    /// </summary>
    public float ReleaseRate
    {
        get
        {
            return releaseRate;
        }
        set
        {
            releaseRate = value;
            releaseCoef = CalcCoef(value, targetRatioDecayRelease);
            releaseBase = -targetRatioDecayRelease * (1.0f - releaseCoef);
        }
    }

    void Start()
    {
        generator.onClick.AddListener(Release);
        SetIsFilterActive(false);
        Reset();
        SetTargetRatioAttack(0.3f);
        SetTargetRatioDecayRelease(0.0001f);
    }

    private void Awake()
    {
        filterIndex = 2;
    }

    void Update()
    {
        //Debug.Log("output: " + output);
    }

    public override float ProcessSample(float sample, int sampleIndex, int channelIndex, float currentSampleRate, int totalChannels)
    {
        Process();
        if (state != EnvelopeState.Idle && output > 0.01f)
            return sample * output;
        else
            return sample;
    }

    public void StartAttack()
    {
        Gate(true);
        Debug.Log("Sate: " + state);
    }

    public void Release()
    {
        Gate(false);
        Debug.Log("Sate: " + state);
    }

    private static float CalcCoef(float rate, float targetRatio)
    {
        return (float)System.Math.Exp(-System.Math.Log((1.0f + targetRatio) / targetRatio) / rate);
    }

    /// <summary>
    /// Sustain Level (1 = 100%)
    /// </summary>
    public float SustainLevel
    {
        get
        {
            return sustainLevel;
        }
        set
        {
            sustainLevel = value;
            decayBase = (sustainLevel - targetRatioDecayRelease) * (1.0f - decayCoef);
        }
    }

    /// <summary>
    /// Sets the attack curve
    /// </summary>
    void SetTargetRatioAttack(float targetRatio)
    {
        if (targetRatio < 0.000000001f)
            targetRatio = 0.000000001f;  // -180 dB
        targetRatioAttack = targetRatio;
        attackBase = (1.0f + targetRatioAttack) * (1.0f - attackCoef);
    }

    /// <summary>
    /// Sets the decay release curve
    /// </summary>
    void SetTargetRatioDecayRelease(float targetRatio)
    {
        if (targetRatio < 0.000000001f)
            targetRatio = 0.000000001f;  // -180 dB
        targetRatioDecayRelease = targetRatio;
        decayBase = (sustainLevel - targetRatioDecayRelease) * (1.0f - decayCoef);
        releaseBase = -targetRatioDecayRelease * (1.0f - releaseCoef);
    }

    /// <summary>
    /// Read the next volume multiplier from the envelope generator
    /// </summary>
    /// <returns>A volume multiplier</returns>
    public float Process()
    {
        switch (state)
        {
            case EnvelopeState.Idle:
                break;
            case EnvelopeState.Attack:
                output = attackBase + output * attackCoef;
                if (output >= 1.0f)
                {
                    output = 1.0f;
                    state = EnvelopeState.Decay;
                    Debug.Log("Sate: " + state);
                }
                break;
            case EnvelopeState.Decay:
                output = decayBase + output * decayCoef;
                if (output <= sustainLevel)
                {
                    output = sustainLevel;
                    state = EnvelopeState.Sustain;
                    Debug.Log("Sate: " + state);
                }
                break;
            case EnvelopeState.Sustain:
                break;
            case EnvelopeState.Release:
                output = releaseBase + output * releaseCoef;
                if (output <= 0.0)
                {
                    output = 0.0f;
                    state = EnvelopeState.Idle;
                    Debug.Log("Sate: " + state);
                }
                break;
        }
        return output;
    }

    /// <summary>
    /// Trigger the gate
    /// </summary>
    /// <param name="gate">If true, enter attack phase, if false enter release phase (unless already idle)</param>
    public void Gate(bool gate)
    {
        if (gate)
            state = EnvelopeState.Attack;
        else if (state != EnvelopeState.Idle)
            state = EnvelopeState.Release;
    }

    public EnvelopeState State
    {
        get
        {
            return state;
        }
    }
    
    public void Reset()
    {
        state = EnvelopeState.Idle;
        output = 0.0f;
    }
}
