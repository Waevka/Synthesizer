using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorUpdater : MonoBehaviour {
    [SerializeField]
    GenerateSound musicCube;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnGeneratorToggleChange(bool generatorIsOn)
    {
        musicCube.setGeneratorEnabled(generatorIsOn);
    }
}
