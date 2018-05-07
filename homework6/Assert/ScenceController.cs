using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScenceController : MonoBehaviour
{
	public PatrolFactory factory;
	public GameObject wallPrefab;
	public Material brickMateria;
	public List<GameObject> pointObjects;
	public PatrolActionManager manager;
	public GameObject player;
	public GameObject planePrefab;

	private Director director;
	private float player_speed = 50;
	private float rotate_speed = 100;
	private List<GameObject> monsters = new List<GameObject>();
	private List<Vector3> diamondLocations = new List<Vector3> ();
	private GameObject[] diamonds = new GameObject[19];
	private ScoreBoard board;

	// Use this for initialization
	void Start ()
	{
		director = Director.getInstance ();
		factory = Singleton<PatrolFactory>.Instance;
		board = Singleton<ScoreBoard>.Instance;
		director.scence = this;
		manager = gameObject.AddComponent<PatrolActionManager>() as PatrolActionManager;
		gameObject.GetComponent<PatrolActionManager> ().player = player;
		for (int i = 0; i < 10; i++) {
			GameObject monster = factory.getPatrol (i / 2);
			monsters.Add (monster);
			manager.beginPatrol (monster);
		}
		getLocations (5);
		reloadSource ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	void OnEnable(){
		GameEventManager.GameoverChange += GameOver;
		GameEventManager.Addscore += addScore;
		GameEventManager.Gatecollide += addMonster;
		GameEventManager.Diamondcollide += getDiamond;
	}

	void OnDisable()
	{
		GameEventManager.GameoverChange -= GameOver;
		GameEventManager.Addscore -= addScore;
		GameEventManager.Gatecollide -= addMonster;
		GameEventManager.Diamondcollide -= getDiamond;
	}

	public void MovePlayer(float translationX, float translationZ)
	{
		if(!director.gameOver)
		{
			if (translationX != 0 || translationZ != 0)
			{
				player.GetComponent<Animator> ().SetBool ("isRun", true);
			}
			else
			{
				player.GetComponent<Animator> ().SetBool ("isRun", false);
			}
			player.transform.Translate(0, 0, translationZ * player_speed * Time.deltaTime);
			player.transform.Rotate(0, translationX * rotate_speed * Time.deltaTime, 0);
			if (player.transform.localEulerAngles.x != 0 || player.transform.localEulerAngles.z != 0)
			{
				player.transform.localEulerAngles = new Vector3(0, player.transform.localEulerAngles.y, 0);
			}
			if (player.transform.position.y != 0)
			{
				player.transform.position = new Vector3(player.transform.position.x, 0, player.transform.position.z);
			}     
		}
	}

	private void GameOver(){
		player.transform.position = new Vector3(player.transform.position.x, 0, player.transform.position.z);
		player.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezeAll;
		director.gameOver = true;
		manager.stop ();
	}

	public void gameStart(){
		player.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.None;
		for (int i = 0; i < 19; i++) {
			if (diamonds [i] != null) {
				Destroy (diamonds[i]);
			}
		}
		board.clear ();
		monsters.Clear ();
		manager.destroy ();
		for (int i = 0; i < 10; i++) {
			GameObject monster = factory.getPatrol (i / 2);
			monsters.Add (monster);
			manager.beginPatrol (monster);
		}
		player.transform.position = new Vector3 (50, 0, 50);
		player.transform.localEulerAngles = new Vector3 (0, 180, 0);
		reloadBlueDiamonds ();
		reloadVioletDiamonds ();
	}

	private void addScore(){
		board.escaped ();
	}

	public int getScore(){
		return board.Score();
	}

	public int[] getDiamonds(){
		int[] diamonds = new int[2];
		diamonds [0] = board.Blue ();
		diamonds [1] = board.Violet ();
		return diamonds;
	}

	private void addMonster(GameObject monster){
		monsters.Remove (monster);
		GameObject newMonster = factory.getPatrol (monster.GetComponent<PatrolData> ().area);
		monsters.Add (newMonster);
		manager.beginPatrol (newMonster);
	}

	private void getDiamond(GameObject diamond){
		if (diamond.transform.tag == "Point0") {
			board.getBlueDiamond ();
		} else if (diamond.transform.tag == "Point1") {
			board.getVioletDiamond ();
		}
	}

	private void reloadSource(){
		reloadVioletDiamonds ();
		reloadBlueDiamonds ();
		GameObject wall = Instantiate<GameObject> (wallPrefab, new Vector3(0, 0, 0), Quaternion.identity);
		findAllSon (wall.transform);
		Instantiate<GameObject> (planePrefab, new Vector3(0, 0, 0), Quaternion.identity);
	}

	private void findAllSon(Transform wall){
		for (int i = 0; i < wall.transform.childCount; i++) {
			if (wall.GetChild (i).transform.childCount > 0) {
				findAllSon (wall.transform.GetChild (i));
			} else if(wall.GetChild (i).tag == "Wall"){
				wall.GetChild (i).GetComponent<MeshRenderer> ().material = brickMateria;
			}
		}
	}

	private void reloadVioletDiamonds(){
		for (int i = 0; i < 7; i++) {
			diamonds[i] = Instantiate<GameObject> (pointObjects[1], diamondLocations[i], Quaternion.identity);
		}
	}

	private void reloadBlueDiamonds(){
		for (int i = 7; i < 19; i++) {
			diamonds[i] = Instantiate<GameObject> (pointObjects[0], diamondLocations[i], Quaternion.identity);
		}
	}

	private void getLocations(float height){
		diamondLocations.Add (new Vector3(-175, height, -80));
		diamondLocations.Add (new Vector3(115, height, -130));
		diamondLocations.Add (new Vector3(25, height, -70));
		diamondLocations.Add (new Vector3(80, height, -45));
		diamondLocations.Add (new Vector3(125, height, -80));
		diamondLocations.Add (new Vector3(200, height, -140));
		diamondLocations.Add (new Vector3(215, height, -70));
		diamondLocations.Add (new Vector3(-70, height, -60));
		diamondLocations.Add (new Vector3(-150, height, -50));
		diamondLocations.Add (new Vector3(-85, height, -105));
		diamondLocations.Add (new Vector3(20, height, -150));
		diamondLocations.Add (new Vector3(40, height, -210));
		diamondLocations.Add (new Vector3(105, height, -200));
		diamondLocations.Add (new Vector3(225, height, -200));
		diamondLocations.Add (new Vector3(310, height, -180));
		diamondLocations.Add (new Vector3(255, height, -130));
		diamondLocations.Add (new Vector3(270, height, -75));
		diamondLocations.Add (new Vector3(300, height, -10));
		diamondLocations.Add (new Vector3(175, height, -25));
	}
}

