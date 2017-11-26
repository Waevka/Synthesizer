using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelicopterFilterUpdater : MonoBehaviour {
    [SerializeField]
    HelicopterFilter helicopterFilter;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void HelicopterIsEnabledUpdater(bool b)
    {
        Debug.Log("Updating Helicopter filter state, is enabled: " + b);
        helicopterFilter.SetIsFilterActive(b);
    }
}
