using UnityEngine;
using System.Collections;

public class CollisionEvent : MonoBehaviour
{

	void OnCollisionEnter(Collision colliser)
	{
		if (colliser.gameObject.tag == "Player") {
			colliser.gameObject.GetComponent<Animator> ().SetBool("isRun", false);
			this.GetComponent<Animator> ().SetTrigger ("Attack_1");
			Singleton<GameEventManager>.Instance.PlayerGameover ();
		} else if (colliser.gameObject.tag == "Wall") {
			this.GetComponent<PatrolData> ().isWall = true;
		}
	}

	void OnTriggerEnter(Collider collider)
	{
		if (collider.gameObject.tag == "Player")
		{
			this.gameObject.transform.GetComponent<PatrolData>().isFollow = true;
		}
	}
	void OnTriggerExit(Collider collider)
	{
		if (collider.gameObject.tag == "Player")
		{
			this.gameObject.transform.GetComponent<PatrolData> ().isFollow = false;
			Singleton<GameEventManager>.Instance.AddScore ();
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

