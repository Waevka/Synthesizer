using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnvelopeGeneratorUpdater : MonoBehaviour
{
    [SerializeField]
    EnvelopGenerator m_EnvelopeGenerator;


    public Slider m_Attack;
    public Slider m_Delay;
    public Slider m_Sustain;
    public Slider m_Release;
    public Text m_TextAttack;
    public Text m_TextDelay;
    public Text m_TextSustain;
    public Text m_TextRelease;


    void Start ()
    {
        AValueUpdate(m_Attack.value);
        DValueUpdate(m_Delay.value);
        SValueUpdate(m_Sustain.value);
        RValueUpdate(m_Release.value);
    }

    public void StartAttack()
    {
        m_EnvelopeGenerator.StartAttack();
    }

    public void IsEnabled(bool enable)
    {
        Debug.Log("Updating envelope generator state, is enabled: " + enable);
        m_EnvelopeGenerator.SetIsFilterActive(enable);
        m_EnvelopeGenerator.generator.interactable = enable;
    }

    public void AValueUpdate(float v)
    {
        m_EnvelopeGenerator.AttackRate = v;
        m_TextAttack.text = GetTimeFromSampleRate(v).ToString("F2");
    }

    public void DValueUpdate(float v)
    {
        m_EnvelopeGenerator.DecayRate = v;
        m_TextDelay.text = GetTimeFromSampleRate(v).ToString("F2");
    }

    public void SValueUpdate(float v)
    {
        m_EnvelopeGenerator.SustainLevel = v;
        m_TextSustain.text = v.ToString("F2");
    }

    public void RValueUpdate(float v)
    {
        m_EnvelopeGenerator.ReleaseRate = v;
        m_TextRelease.text = GetTimeFromSampleRate(v).ToString("F2");
    }

    private float GetTimeFromSampleRate(float v)
    {
        return v / 44100.0f;
    }
}
