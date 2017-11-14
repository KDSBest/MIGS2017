using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotDestroy : MonoBehaviour
{
    private float live = 0.3f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
	{
	    live -= Time.deltaTime;

        if(live <= 0)
            GameObject.Destroy(this.gameObject);
	}
}
