using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseUpdater : MonoBehaviour
{
    [SerializeField]
    private GenerateSound soundGenerator;

    public void OnClickWhiteNoiseButton()
    {

    }

    public void OnClickPinkNoiseButton()
    {

    }

    public void OnClickSquareButton()
    {
        UpdateWave(GenerateSound.WaveType.SQUARE);
    }

    public void UpdateWave(GenerateSound.WaveType wavetype)
    {
        soundGenerator.SetWaveType(wavetype);
    }
}