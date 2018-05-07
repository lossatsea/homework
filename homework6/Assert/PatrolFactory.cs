using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PatrolFactory : MonoBehaviour
{
	public GameObject patrolPrefab;
	private List<PatrolData> used = new List<PatrolData>();
	private List<PatrolData> free = new List<PatrolData>();


	public GameObject getPatrol(int area){
		GameObject newPatrol = null;
		float baseX = 0;
		float baseZ = 0;
		switch (area) {
		case 0:
			baseX = -100;
			baseZ = -50;
			break;
		case 1:
			baseX = 80;
			baseZ = -180;
			break;
		case 2:
			baseX = 250;
			baseZ = -180;
			break;
		case 3:
			baseX = 250;
			baseZ = -50;
			break;
		case 4:
			baseX = 50;
			baseZ = -50;
			break;
		}
		if (free.Count > 0) {
			newPatrol = free [0].gameObject;
			float dev = Random.Range(-20, 20);
			newPatrol.transform.position = new Vector3(baseX + dev, 0, baseZ + dev);
			free.Remove (free[0]);
		} else {
			float dev = Random.Range(-20, 20);
			newPatrol = Instantiate<GameObject> (patrolPrefab, new Vector3(baseX + dev, 0, baseZ + dev), Quaternion.identity);
		}
		newPatrol.SetActive (true);
		newPatrol.GetComponent<PatrolData> ().area = area;
		used.Add (newPatrol.GetComponent<PatrolData>());
		return newPatrol;
	}

	public void freePatrol(GameObject oldPatrol){
		for (int i = 0; i < used.Count; i++) {
			if (used [i].gameObject == oldPatrol) {
				PatrolData move = used[i];
				used.Remove (move);
				free.Add (move);
				return;
			}
		}
		Debug.Log ("Exception: No such disk int used list");
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

