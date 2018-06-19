using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tankData : MonoBehaviour {

	public int hp;
	public GameObject bulletPrefab;
	private Team team;

	public void setTeam(Team t){
		team = t;
	}

	public Team getTeam(){
		return team;
	}
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
