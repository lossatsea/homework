using UnityEngine;
using System.Collections;

public class ScoreBoard : MonoBehaviour
{
	private int score = 0;
	private int blueDiamondNum = 0;
	private int violetDiamondNum = 0;

	public void clear(){
		score = 0;
		blueDiamondNum = 0;
		violetDiamondNum = 0;
	}
		
	public void escaped(){
		score++;
	}

	public void getBlueDiamond(){
		score += 2;
		blueDiamondNum++;
	}

	public void getVioletDiamond(){
		score += 5;
		violetDiamondNum++;
	}

	public int Score(){
		return score;
	}

	public int Blue(){
		return blueDiamondNum;
	}

	public int Violet(){
		return violetDiamondNum;
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

