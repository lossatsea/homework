using UnityEngine;
using System.Collections;

public class PatrolActionManager :SSActionManager
{
	PatrolAction patrol;
	public GameObject player;

	public void beginPatrol(GameObject patrolman){
		patrol = PatrolAction.GetSSAction (player);
		RunAction (patrolman, patrol, this);
	}

	public void destroy(){
		DestroyAll ();
	}

	public void stop(){
		StopAll ();
	}
}

