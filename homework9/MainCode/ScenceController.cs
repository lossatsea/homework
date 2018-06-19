using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenceController : MonoBehaviour {

	public int enemiesNum = 5;
	public GameObject enemyPrefab;
	public GameObject playerPrefab;
	public Mode mode = Mode.Play;
	public Material bluePrefabs;
	public Material redPrefabs;
	public GameObject bulletExplosion;
	public GameObject tankExplosion;

	private float enemyPos_x = -80;
	private float enemyPos_y = 40;
	private Vector3 playerPos = new Vector3(80, 1, -40);
	private List<GameObject> teamR;
	private List<GameObject> teamB;
	private GameObject player;
	private State state = State.Ready;
	private float explosionTime = 2;

	void Awake(){
		Director.getInstance ().scence = this;
		teamR = new List<GameObject> ();
		teamB = new List<GameObject> ();
	}

	void OnEnable(){
		EventManager.destroy += destroyTank;
		EventManager.hit += hitTank;
	}

	void OnDisable(){
		EventManager.destroy -= destroyTank;
		EventManager.hit -= hitTank;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (state == State.Playing) {
			if (teamR.Count == 0) {
				state = State.Lose;
			} else if (teamB.Count == 0) {
				state = State.Win;
			}
			if (mode == Mode.Play && state == State.Playing) {
				Camera.main.transform.position = new Vector3 (player.transform.position.x, Camera.main.transform.position.y, player.transform.position.z);
			} else if (mode == Mode.Auto && state == State.Playing) {
				Camera.main.transform.position = new Vector3 (0, 80, 0);
			}
		}
	}

	public State getState(){
		return state;
	}

	public int[] RedHPs(){
		int[] phs = new int[teamR.Count];
		for (int i = 0; i < teamR.Count; i++) {
			phs [i] = Mathf.Max(teamR [i].GetComponent<tankData> ().hp, 0);
		}
		return phs;
	}

	public int[] BlueHPs(){
		int[] phs = new int[teamB.Count];
		for (int i = 0; i < teamB.Count; i++) {
			phs [i] = Mathf.Max(teamB [i].GetComponent<tankData> ().hp, 0);
		}
		return phs;
	}

	public List<GameObject> getTeamR(){
		return teamR;
	}

	public List<GameObject> getTeamB(){
		return teamB;
	}

	public void startWithAuto(){
		mode = Mode.Auto;
		for (int i = 0; i < enemiesNum; i++) {
			GameObject enemy = Instantiate<GameObject> (enemyPrefab, new Vector3(-enemyPos_x, 1, -enemyPos_y + 8 * i), Quaternion.identity);
			enemy.GetComponent<tankData> ().setTeam (Team.Red);
			enemy.transform.GetChild (0).GetComponent<MeshRenderer> ().material = redPrefabs;
			teamR.Add (enemy);
		}
		for (int i = 0; i < enemiesNum; i++) {
			GameObject enemy = Instantiate<GameObject> (enemyPrefab, new Vector3(enemyPos_x, 1, enemyPos_y - 8 * i), Quaternion.identity);
			enemy.GetComponent<tankData> ().setTeam (Team.Blue);
			enemy.transform.GetChild (0).GetComponent<MeshRenderer> ().material = bluePrefabs;
			teamB.Add (enemy);
		}
		
		state = State.Playing;
	}

	public void startWithPlay(){
		mode = Mode.Play;
		player = Instantiate<GameObject> (playerPrefab, playerPos, Quaternion.identity);
		player.GetComponent<tankData> ().setTeam (Team.Red);
		player.transform.GetChild (0).GetComponent<MeshRenderer> ().material = redPrefabs;
		teamR.Add (player);
		for (int i = 0; i < enemiesNum; i++) {
			GameObject enemy = Instantiate<GameObject> (enemyPrefab, new Vector3(enemyPos_x, 1, enemyPos_y - 8 * i), Quaternion.identity);
			enemy.GetComponent<tankData> ().setTeam (Team.Blue);
			enemy.transform.GetChild (0).GetComponent<MeshRenderer> ().material = bluePrefabs;
			teamB.Add (enemy);
		}
		state = State.Playing;

	}

	private void destroyTank(GameObject tank){
		GameObject ex = Instantiate<GameObject> (tankExplosion, tank.transform.position, Quaternion.identity);
		ex.GetComponent<ParticleSystem> ().Play ();
		Destroy (ex, explosionTime);
		if (tank.GetComponent <tankData> ().getTeam () == Team.Blue) {
			teamB.Remove (tank);
			Destroy (tank);
		} else {
			teamR.Remove (tank);
			Destroy (tank);
		}
	}

	private void hitTank(GameObject bullet){
		GameObject ex = Instantiate<GameObject> (bulletExplosion, bullet.transform.position, Quaternion.identity);
		ex.GetComponent<ParticleSystem> ().Play ();
		Destroy (ex, explosionTime);
	}
}
