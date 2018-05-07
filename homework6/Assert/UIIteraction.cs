using UnityEngine;
using System.Collections;

public class UIIteraction : MonoBehaviour
{
	private Director director;
	private string message = 
		"Game Rule:\n" +
		"\nEscape from monster one time: plus 1" +
		"\nGet a blue diamond: plus 2" +
		"\nGet a violet diamond: plus 5" +
		"\nBe arrested by monster: Game Over" +
		"\n\nGood Luck!"
		;
	// Use this for initialization
	void Start ()
	{
		director = Director.getInstance ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		director = Director.getInstance ();
		if (director != null) {
			float translationX = Input.GetAxis("Horizontal");
			float translationZ = Input.GetAxis("Vertical");
			director.scence.MovePlayer(translationX, translationZ);
		}
	}

	void OnGUI(){
		GUIStyle scoreStyle = new GUIStyle ();
		scoreStyle.alignment = TextAnchor.MiddleCenter;
		GUIStyle messageStyle = new GUIStyle ();
		messageStyle.alignment = TextAnchor.MiddleLeft;
		messageStyle.fontSize = 14;
		if (director.gameOver) {
			scoreStyle.fontSize = 12;
			GUI.Label (new Rect(Screen.width / 2 - 100, 50, 200, 80), "Score: " + director.scence.getScore() 
				+ "\nBlue Diamond: " + director.scence.getDiamonds()[0] 
				+ "\nViolet Diamond: " + director.scence.getDiamonds()[1], scoreStyle);
			if (GUI.Button (new Rect (Screen.width / 2 - 50, Screen.height / 2 - 30, 100, 60), "Game Start") ){
				director.gameOver = false;
				director.gameStart ();
			}
		} else {
			scoreStyle.fontSize = 16;
			GUI.Label (new Rect(Screen.width - 200, 20, 200, 20), "Score: " + director.scence.getScore() 
				+ "\nBlue Diamond: " + director.scence.getDiamonds()[0] 
				+ "\nViolet Diamond: " + director.scence.getDiamonds()[1], scoreStyle);
			GUI.Label (new Rect(5, 10, 200, 100), message, messageStyle);
		}
	}
}

