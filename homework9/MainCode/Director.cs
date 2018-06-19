using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Director : System.Object {
	
	private static Director _instance;
	public ScenceController scence { get; set;}
	public static Director getInstance() {
		if (_instance == null) {
			_instance = new Director ();
		} 
		return _instance;
	}
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
