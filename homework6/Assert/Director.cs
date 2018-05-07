using UnityEngine;
using System.Collections;

public class Director : System.Object 
{
	public ScenceController scence;
	private static Director instance;
	public bool gameOver = false;

	public static Director getInstance(){
		if (instance == null) {
			instance = new Director ();
		}
		return instance;
	}

	public void gameStart(){
		scence.gameStart ();
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

