using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAction : MonoBehaviour {

	Director director;
	public GameObject target;
	State state;
	private bool isIEnumerator = false;
	private bool canShoot = false;
	// Use this for initialization
	void Start () {
		director = Director.getInstance ();
	}

	// Update is called once per frame
	void Update () {
		state = Director.getInstance ().scence.getState();
		if (state == State.Playing) {
			judgeShoot ();
			if (!isIEnumerator) {
				StartCoroutine(shoot());
				isIEnumerator = true;
			}
			if (target != null) {
				this.transform.LookAt (target.transform.position);
				this.GetComponent<NavMeshAgent> ().SetDestination (target.transform.position);
			} else if(director.scence.getTeamB().Count > 0 && director.scence.getTeamR().Count > 0) {
				getTarget ();
			}
		} else {
			this.GetComponent<NavMeshAgent> ().velocity = Vector3.zero;
			this.GetComponent<NavMeshAgent> ().ResetPath ();
			this.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezeAll;
		}
	}

	void FixedUpdate(){
		this.GetComponent<Rigidbody> ().velocity = new Vector3(0, 0, 0);
	}

	IEnumerator shoot() {
		while(state == State.Playing) {
			for (float i = 1; i > 0; i -= Time.deltaTime) {
				yield return 0; 
			}
			if (state == State.Playing && Vector3.Distance(this.transform.position, target.transform.position) < 100 && canShoot) {
				if (Vector3.Distance (this.transform.position, target.transform.position) < 20) {
					this.GetComponent<NavMeshAgent> ().speed = 0;
				} else {
					this.GetComponent<NavMeshAgent> ().speed = 1.0f;
				}
				GameObject bullet = Instantiate<GameObject> (this.GetComponent<tankData> ().bulletPrefab, this.transform.position + this.transform.forward * 3f + new Vector3(0, 1, 0), Quaternion.identity);
				bullet.GetComponent<bulletAction> ().team = this.GetComponent<tankData>().getTeam();
				bullet.transform.forward = reloadPos( this.transform.forward);
				bullet.GetComponent<Rigidbody> ().AddForce (bullet.transform.forward * 20, ForceMode.Impulse);
			}
		}
	}

	void getTarget(){
		List<GameObject> targets;
		if (this.GetComponent<tankData> ().getTeam () == Team.Blue) {
			targets = director.scence.getTeamR ();
		} else {
			targets = director.scence.getTeamB ();
		}
		if (targets.Count == 0)
			return;
		int max = targets.Count;
		int random = 0;
		do {
			random = (int)Mathf.Floor(Random.Range(0, max));
		} while(targets [random] == null);
		target = targets [random];
	}

	Vector3 reloadPos(Vector3 pos){
		float randomX = Random.Range (-1, 1);
		float randomZ = Random.Range (-1, 1);
		return new Vector3 (pos.x + randomX / 6, pos.y, pos.z + randomZ / 6);
	}

	void judgeShoot(){
		if (target != null) {
			Ray ray = new Ray (this.transform.position, target.transform.position - this.transform.position);
			RaycastHit hit;  
			bool result = Physics.Raycast (ray, out hit, 1000);
			if (result) {
				Debug.DrawLine (this.transform.position, hit.point, Color.red);
				Debug.Log (hit.collider.gameObject.tag);
				if (hit.collider.gameObject.tag == "enemy" || hit.collider.gameObject.tag == "player") {
					canShoot = true;
					return;
				}
			}
			canShoot = false;
		}
	}
}
