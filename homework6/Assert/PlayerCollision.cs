using UnityEngine;
using System.Collections;

public class PlayerCollision : MonoBehaviour
{

	void OnCollisionEnter(Collision colliser)
	{
		if (colliser.transform.tag == "Point0" || colliser.transform.tag == "Point1") {
			Singleton<GameEventManager>.Instance.DiamondCollide (colliser.gameObject);
			Destroy (colliser.gameObject);
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

