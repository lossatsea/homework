using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAction : MonoBehaviour{

	Director director;
	private int duration = 10;

	public void moveForward(){
		this.GetComponent<Rigidbody> ().velocity = this.transform.forward * 20;
	}

	public void moveBack(){
		this.GetComponent<Rigidbody> ().velocity = this.transform.forward * -20;
	}

	public void turn(float offset){
		float y = this.transform.localEulerAngles.y + offset * 2;
		float x = this.transform.localEulerAngles.x;
		this.transform.localEulerAngles = new Vector3 (x, y, 0);
	}

	public void shoot(){
		GameObject bullet = Instantiate<GameObject> (this.GetComponent<tankData> ().bulletPrefab, this.transform.position + this.transform.forward * 3f, Quaternion.identity);
		bullet.GetComponent<bulletAction> ().team = Team.Red;
		bullet.transform.forward = this.transform.forward;
		bullet.GetComponent<Rigidbody> ().AddForce (bullet.transform.forward * 35, ForceMode.Impulse);
	}

	// Use this for initialization
	void Start () {
		
	}
		

	// Update is called once per frame
	void Update () {
		director = Director.getInstance ();
		if (director != null && director.scence.getState() == State.Playing) {
			duration--;
			if (Input.GetKey (KeyCode.W)) {
				moveForward ();
			} else if (Input.GetKey (KeyCode.S)) {
				moveBack ();
			} else {
				this.GetComponent<Rigidbody> ().velocity = new Vector3 (0, 0, 0);
			}
			if (Input.GetKey (KeyCode.Space)) {
				if (duration < 0) {
					duration = 10;
					shoot ();
				}
			}
			float offset = Input.GetAxis ("Horizontal1");
			turn (offset);
		} else {
			this.GetComponent<Rigidbody> ().velocity = Vector3.zero;
			this.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezeAll;
		}
	}
}
