using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveUpdater : MonoBehaviour {

    [SerializeField]
    private GenerateSound soundGenerator;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnClickSineButton()
    {
        UpdateWave(GenerateSound.WaveType.SINE);
    }

    public void OnClickTriangleButton()
    {
        UpdateWave(GenerateSound.WaveType.TRIANGLE);
    }

    public void OnClickSawButton()
    {
        UpdateWave(GenerateSound.WaveType.SAW);
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
