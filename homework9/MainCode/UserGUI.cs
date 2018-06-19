using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserGUI : MonoBehaviour {

	Director director = Director.getInstance();
	GUIStyle style1, style2, style3;

	void OnGUI(){
		//Debug.Log (director.scence.getState ());

		GUI.Label (new Rect (Screen.width / 2 - 30, Screen.height - 60, 30, 50), director.scence.getTeamB().Count.ToString (), style2);
		GUI.Label (new Rect (Screen.width / 2, Screen.height - 60, 30, 50), director.scence.getTeamR().Count.ToString (), style1);
		GUI.Label (new Rect (Screen.width / 2 - 5, Screen.height - 60, 10, 50), ":", style3);
		if (director.scence.getState () == State.Ready) {
			if (GUI.Button (new Rect (Screen.width / 2 - 200, Screen.height / 2 - 20, 150, 40), "Auto")) {
				director.scence.startWithAuto ();
			} else if (GUI.Button (new Rect (Screen.width / 2 + 200, Screen.height / 2 - 20, 150, 40), "Play")) {
				director.scence.startWithPlay ();
			}
		} else if (director.scence.getState () == State.Playing) {
			for (int i = 0; i < director.scence.RedHPs ().Length; i++) {
				GUI.Label (new Rect (Screen.width - i * 20 - 20, Screen.height - 60, 20, 50), director.scence.RedHPs () [i].ToString (), style1);
			}
			for (int i = 0; i < director.scence.BlueHPs ().Length; i++) {
				GUI.Label (new Rect (i * 20 + 20, Screen.height - 60, 20, 50), director.scence.BlueHPs () [i].ToString (), style2);
			}
		} else if (director.scence.getState () == State.Win) {
			GUI.Label (new Rect (Screen.width / 2 - 100, Screen.height / 2 - 30, 200, 60), "Team Red Win", style1);
		} else{
			GUI.Label (new Rect (Screen.width / 2 - 100, Screen.height / 2 - 30, 200, 60), "Team Blue Win", style2);
		}
	}
	// Use this for initialization
	void Start () {
		style1 = new GUIStyle();
		style1.normal.textColor = Color.red;
		style1.fontStyle = FontStyle.Bold;
		style1.fontSize = 15;
		style1.alignment = TextAnchor.MiddleCenter;

		style2 = new GUIStyle();
		style2.normal.textColor = Color.blue;
		style2.fontStyle = FontStyle.Bold;
		style2.fontSize = 15;
		style2.alignment = TextAnchor.MiddleCenter;

		style3 = new GUIStyle();
		style3.normal.textColor = Color.black;
		style3.fontStyle = FontStyle.Bold;
		style3.fontSize = 15;
		style3.alignment = TextAnchor.MiddleCenter;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
