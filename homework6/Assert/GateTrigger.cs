using UnityEngine;
using System.Collections;

public class GateTrigger : MonoBehaviour
{

	void OnTriggerEnter(Collider collider)
	{
		if(collider.gameObject.tag == "Gate"){
			this.gameObject.transform.parent.GetComponent<PatrolData> ().isGate = true;
			Singleton<GameEventManager>.Instance.GateCollide(this.gameObject.transform.parent.gameObject);
		}
	}
	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
}

