using UnityEngine;
using System.Collections;

public class GameEventManager : MonoBehaviour
{
	public delegate void GameoverEvent();
	public static event GameoverEvent GameoverChange;

	public delegate void AddscoreEvent();
	public static event GameoverEvent Addscore;

	public delegate void GateeEvent(GameObject monster);
	public static event GateeEvent Gatecollide;

	public delegate void DiamondEvent(GameObject diamond);
	public static event GateeEvent Diamondcollide;


	public void PlayerGameover()
	{
		if (GameoverChange != null)
		{
			GameoverChange();
		}
	}

	public void AddScore(){
		if (Addscore != null) {
			Addscore ();
		}
	}

	public void GateCollide(GameObject monster){
		if (Gatecollide != null) {
			Gatecollide (monster);
		}
	}

	public void DiamondCollide(GameObject diamond){
		if (Diamondcollide != null) {
			Diamondcollide (diamond);
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

