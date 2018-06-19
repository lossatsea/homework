using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager: MonoBehaviour{

	public delegate void DestroyEvent (GameObject tank);
	public static event DestroyEvent destroy;

	public delegate void HitEvent (GameObject bullet);
	public static event DestroyEvent hit;

	public void destroyTank(GameObject tank){
		if (destroy != null) {
			destroy (tank);
		}
	}

	public void hitTank(GameObject bullet){
		if (hit != null) {
			hit (bullet);
		}
	}

}