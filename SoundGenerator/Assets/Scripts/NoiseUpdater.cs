﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseUpdater : MonoBehaviour
{
    [SerializeField]
    private GenerateSound soundGenerator;

    public void OnClickWhiteNoiseButton()
    {
        UpdateWave(GenerateSound.WaveType.WHITE_NOISE);

    }

    public void OnClickPinkNoiseButton()
    {
        UpdateWave(GenerateSound.WaveType.PINK_NOISE);
    }

    public void UpdateWave(GenerateSound.WaveType wavetype)
    {
        soundGenerator.SetWaveType(wavetype);
    }
}