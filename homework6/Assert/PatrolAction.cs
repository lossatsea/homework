using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PatrolAction : SSAction
{
	private List<Vector3> path = new List<Vector3>();
	private GameObject player;
	private int posNum = 1;
	private float speed;
	private Vector3 pos;
	private bool isStand = false;


	public static PatrolAction GetSSAction(GameObject player)
	{
		PatrolAction action = CreateInstance<PatrolAction>();
		action.player = player;
		return action;
	}
	// Use this for initialization
	public override void Start ()
	{
		speed = gameobject.GetComponent<PatrolData> ().speed;
		pos = gameobject.transform.position;
		getPath ();
	}
	
	// Update is called once per frame
	public override void Update ()
	{
		if (!isStand) {
			pos = gameobject.transform.position;
			gameobject.GetComponent<Animator>().SetBool("IsWalk", true);
			if (gameobject.GetComponent<PatrolData> ().isGate) {
				destroy = true;
				callback.SSActionEvent (this);
			} else {
				if (transform.localEulerAngles.x != 0 || transform.localEulerAngles.z != 0) {
					transform.localEulerAngles = new Vector3 (0, transform.localEulerAngles.y, 0);
				}            
				if (transform.position.y != 0) {
					transform.position = new Vector3 (transform.position.x, 0, transform.position.z);
				}
				if (!gameobject.GetComponent<PatrolData> ().isFollow) {
					gameobject.GetComponent<Animator> ().SetBool ("IsRun", false);
					if (gameobject.GetComponent<PatrolData> ().isWall) {
						getPath ();
						gameobject.GetComponent<PatrolData> ().isWall = false;
					}
					if (path.Count > 0) {
						Patrol ();
					}
				} else {
					gameobject.GetComponent<Animator> ().SetBool ("IsRun", true);
					gameobject.transform.LookAt (player.transform.position);
					gameobject.transform.position = Vector3.MoveTowards (gameobject.transform.position, player.transform.position, speed * 8 * Time.deltaTime);
				}
			}
		}
	}

	private void Patrol(){
		for (int i = 0; i < 4; i++) {
			if (gameobject.transform.position == path [i]) {
				if (i == 3) {
					posNum = 0;
				} else {
					posNum = i + 1;
				}
				break;
			}
		}
		gameobject.transform.LookAt (path[posNum]);
		gameobject.transform.position = Vector3.MoveTowards (pos ,path[posNum], speed * Time.deltaTime);
	}

	private void getPath(){
		path.Clear ();
		path.Add (pos);
		int length1 = Random.Range (-15, 15);
		while (Mathf.Abs (length1) <= 5) {
			length1 = Random.Range (-15, 15);
		}
		int length2 = Random.Range (-18, 18);
		while (Mathf.Abs (length2) <= 5) {
			length2 = Random.Range (-15, 15);
		}
		Vector3 location1 = new Vector3 (pos.x + length1, 0, pos.z + length2);
		path.Add (location1);
		Vector3 location2 = new Vector3 (location1.x - length2, 0, location1.z + length1);
		path.Add (location2);
		Vector3 location3 = new Vector3 (location2.x - length1, 0, location2.z - length2);
		path.Add (location3);
	}

	public override void stand(){
		isStand = true;
		gameobject.GetComponent<Animator> ().SetTrigger ("Stand");
	}
}

