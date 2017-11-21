using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvelopeGeneratorUpdater : MonoBehaviour
{
    [SerializeField]
    EnvelopGenerator m_EnvelopeGenerator;

    void Start ()
    {

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
        m_EnvelopeGenerator.AValue = v;
    }

    public void DValueUpdate(float v)
    {
        m_EnvelopeGenerator.DValue = v;
    }

    public void SValueUpdate(float v)
    {
        m_EnvelopeGenerator.SValue = v;
    }

    public void RValueUpdate(float v)
    {
        m_EnvelopeGenerator.RValue = v;
    }
}
