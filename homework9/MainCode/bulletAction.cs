using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletAction : MonoBehaviour {

	public Team team;

	void OnTriggerEnter(Collider other){
		if (other.transform.GetComponent<tankData> ()) {
			if (other.transform.GetComponent<tankData> ().getTeam () != team) {
				Singleton<EventManager>.Instance.hitTank (gameObject);
				other.transform.GetComponent<tankData> ().hp--;
				if (other.transform.GetComponent<tankData> ().hp <= 0) {
					Singleton<EventManager>.Instance.destroyTank (other.gameObject);
				}
				Destroy (this.gameObject);
			}
		} else if (other.gameObject.tag == "building"){
			//Debug.Log ("hit the scence");
			Destroy (this.gameObject);
		}
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (this.transform.position.x > 100 || this.transform.position.x < -100 || this.transform.position.z > 50 || this.transform.position.z < -50) {
			Destroy (this.gameObject);
		}
	}
}
