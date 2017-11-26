using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InstrumentUpdater : MonoBehaviour
{
    public Instrument instrument;
	// Use this for initialization
	void Start () {
        instrument = FindObjectOfType<Instrument>();


    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PlayInstrument()
    {
        instrument.musicCube.StartInstrument();
        instrument.btn.interactable = false;
    }
}
