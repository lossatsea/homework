## 坦克对战游戏 AI 设计

### 1. 游戏要求

从商店下载游戏：“Kawaii” Tank 或 其他坦克模型，构建 AI 对战坦克。具体要求

- 使用“感知-思考-行为”模型，建模 AI 坦克
- 场景中要放置一些障碍阻挡对手视线
- 坦克需要放置一个矩阵包围盒触发器，以保证 AI 坦克能使用射线探测对手方位
- AI 坦克必须在有目标条件下使用导航，并能绕过障碍。（失去目标时策略自己思考）
- 实现人机对战

**让坦克变得智能一点**：

- 有目标时进行导航
- 没有目标时自动锁定下一个目标
- 离目标越近速度越慢，小于20时停止
- 当与目标之间没有障碍物时才会进行射击

**让坦克变得笨一点：**

- 躲避障碍物的动作很慢
- 射击有误差
- 只能以固定频率发射子弹（1s）

### 2. 游戏素材

在游戏商店下载“Tanks! Tutorial”

![素材](https://github.com/lossatsea/homework/blob/master/homework9/pictures/%E7%B4%A0%E6%9D%90.png)

自制地图后bake：

![map](https://github.com/lossatsea/homework/blob/master/homework9/pictures/%E5%9C%B0%E5%9B%BE%20(1).png)

![map](https://github.com/lossatsea/homework/blob/master/homework9/pictures/%E5%9C%B0%E5%9B%BE%20(2).png)

预制的地图，坦克和子弹：

![map](https://github.com/lossatsea/homework/blob/master/homework9/pictures/%E9%A2%84%E5%88%B6%20(1).png)
![tank](https://github.com/lossatsea/homework/blob/master/homework9/pictures/%E9%A2%84%E5%88%B6%20(2).png)
![tank](https://github.com/lossatsea/homework/blob/master/homework9/pictures/%E9%A2%84%E5%88%B6%20(3).png)
![bullet](https://github.com/lossatsea/homework/blob/master/homework9/pictures/%E9%A2%84%E5%88%B6%20(4).png)

在enemy，player上有碰撞器，地图上的几个比较重要的建筑也加上了碰撞器，而子弹有触发器：

![tank](https://github.com/lossatsea/homework/blob/master/homework9/pictures/%E7%A2%B0%E6%92%9E%E5%99%A8%20(1).png)
![tank_pengzhaung](https://github.com/lossatsea/homework/blob/master/homework9/pictures/%E7%A2%B0%E6%92%9E%E5%99%A8%20(2).png)

![bullet](https://github.com/lossatsea/homework/blob/master/homework9/pictures/%E8%A7%A6%E5%8F%91%E5%99%A8%20(2).png)
![bullet_chufa](https://github.com/lossatsea/homework/blob/master/homework9/pictures/%E8%A7%A6%E5%8F%91%E5%99%A8%20(1).png)

enemy，player，bullet有不同的行为，他们都是刚体：

![action](https://github.com/lossatsea/homework/blob/master/homework9/pictures/cation%20(1).png)

![action](https://github.com/lossatsea/homework/blob/master/homework9/pictures/cation%20(2).png)

![action](https://github.com/lossatsea/homework/blob/master/homework9/pictures/cation%20(3).png)

enemy和player都有作为坦克的数据类，它们都是NavMeshAgent：

![data](https://github.com/lossatsea/homework/blob/master/homework9/pictures/tank_data%20(1).png)

![data](https://github.com/lossatsea/homework/blob/master/homework9/pictures/tank_data%20(2).png)

### 3. 代码分析

- Director

> 导演类，单例模式

```C#
public class Director : System.Object {
	
	private static Director _instance;
	public ScenceController scence { get; set;}
	public static Director getInstance() {
		if (_instance == null) {
			_instance = new Director ();
		} 
		return _instance;
	}
}
```

- tankData

> 坦克的数据类

```C#
public class tankData : MonoBehaviour {

	public int hp;  //血条
	public GameObject bulletPrefab;//子弹预制
	private Team team;  //队伍
    
    //设置队伍
	public void setTeam(Team t){
		team = t;
	}

    //返回队伍
	public Team getTeam(){
		return team;
	}
}
```

- PublicQuote

> 公共引用的枚举，接口等

```C#
//队伍：红队，蓝队
public enum Team:int{Red, Blue}

//游戏模式：自动，人机
public enum Mode:int{Auto, Play}

//游戏状态：准备，游戏中，红队胜利，蓝队胜利
public enum State:int{Ready, Playing, Win, Lose}

```

- EventManager

> 事件发布器

```C#
public class EventManager: MonoBehaviour{

    //坦克的破坏事件（HP降为0）
	public delegate void DestroyEvent (GameObject tank);
	public static event DestroyEvent destroy;
    
    //坦克的击中事件
	public delegate void HitEvent (GameObject bullet);
	public static event DestroyEvent hit;

	public void destroyTank(GameObject tank){
		if (destroy != null) {
			destroy (tank);
		}
	}

	public void hitTank(GameObject bullet){
		if (hit != null) {
			hit (bullet);
		}
	}
}
```

- ScenceController

> 场景管理器

(1) 成员变量

```C#
public int enemiesNum = 5;          //敌人的数量
public GameObject enemyPrefab;      //enemy预制
public GameObject playerPrefab;     //player预制
public Mode mode = Mode.Play;       //游戏模式
public Material bluePrefabs;        //蓝色标志材料
public Material redPrefabs;         //红色标志材料
public GameObject bulletExplosion;  //子弹爆炸预制
public GameObject tankExplosion;    //坦克爆炸预制

//敌人初始位置
private float enemyPos_x = -80;
private float enemyPos_y = 40;
//玩家初始位置
private Vector3 playerPos = new Vector3(80, 1, -40);

private List<GameObject> teamR;     //红队列表
private List<GameObject> teamB;     //蓝队列表
private GameObject player;          //玩家
private State state = State.Ready;  //游戏状态
private float explosionTime = 2;    //爆炸时长
```

（2）初始化

```C#
void Awake(){
	Director.getInstance ().scence = this;
	teamR = new List<GameObject> ();
	teamB = new List<GameObject> ();
}

//添加事件响应
void OnEnable(){
	EventManager.destroy += destroyTank;
	EventManager.hit += hitTank;
}

void OnDisable(){
	EventManager.destroy -= destroyTank;
	EventManager.hit -= hitTank;
}
```

（3）Update

> 主要功能是摄像机跟随，判断游戏是否结束

```C#
void Update () {
	if (state == State.Playing) {
	    //游戏结束条件
		if (teamR.Count == 0) {
			state = State.Lose;
		} else if (teamB.Count == 0) {
			state = State.Win;
		}
		//摄像机位置
		if (mode == Mode.Play && state == State.Playing) {
			Camera.main.transform.position = new Vector3 (player.transform.position.x, Camera.main.transform.position.y, player.transform.position.z);
		} else if (mode == Mode.Auto && state == State.Playing) {
			Camera.main.transform.position = new Vector3 (0, 80, 0);
		}
	}
}
```

（4）返回函数

> 用来返回各种信息，主要用来显示GUI

```C#
//返回当前游戏状态
public State getState(){
	return state;
}

//返回红队HP
public int[] RedHPs(){
	int[] phs = new int[teamR.Count];
	for (int i = 0; i < teamR.Count; i++) {
		phs [i] = Mathf.Max(teamR [i].GetComponent<tankData> ().hp, 0);
	}
	return phs;
}

//返回蓝队HP
public int[] BlueHPs(){
	int[] phs = new int[teamB.Count];
	for (int i = 0; i < teamB.Count; i++) {
		phs [i] = Mathf.Max(teamB [i].GetComponent<tankData> ().hp, 0);
	}
	return phs;
}

//返回红队成员
public List<GameObject> getTeamR(){
	return teamR;
}

//返回蓝队成员
public List<GameObject> getTeamB(){
	return teamB;
}
```

（5）游戏开始函数

> 两种模式的游戏开始，进行坦克的预制实例化

```C#
//自动模式
public void startWithAuto(){
	mode = Mode.Auto;
	
	//实例化红队和蓝队的坦克
	for (int i = 0; i < enemiesNum; i++) {
	    //实例化坦克
		GameObject enemy = Instantiate<GameObject> (enemyPrefab, new Vector3(-enemyPos_x, 1, -enemyPos_y + 8 * i), Quaternion.identity);
		//设置队伍为红队
		enemy.GetComponent<tankData> ().setTeam (Team.Red);
		//将标志球变为红色
		enemy.transform.GetChild (0).GetComponent<MeshRenderer> ().material = redPrefabs;
		//加入红队
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

//人机模式
public void startWithPlay(){
	mode = Mode.Play;
	//实例化玩家，玩家默认为红队
	player = Instantiate<GameObject> (playerPrefab, playerPos, Quaternion.identity);
	player.GetComponent<tankData> ().setTeam (Team.Red);
	player.transform.GetChild (0).GetComponent<MeshRenderer> ().material = redPrefabs;
	teamR.Add (player);
	
	//实例化蓝队
	for (int i = 0; i < enemiesNum; i++) {
		GameObject enemy = Instantiate<GameObject> (enemyPrefab, new Vector3(enemyPos_x, 1, enemyPos_y - 8 * i), Quaternion.identity);
		enemy.GetComponent<tankData> ().setTeam (Team.Blue);
		enemy.transform.GetChild (0).GetComponent<MeshRenderer> ().material = bluePrefabs;
		teamB.Add (enemy);
	}
	state = State.Playing;

}
```

（6）事件响应函数

> 之前订阅的事件的响应

```C#
//坦克的销毁
private void destroyTank(GameObject tank){
    
    //坦克爆炸效果的实例化
	GameObject ex = Instantiate<GameObject> (tankExplosion, tank.transform.position, Quaternion.identity);
	//播放
	ex.GetComponent<ParticleSystem> ().Play ();
	//一定时间后销毁
	Destroy (ex, explosionTime);
	
	//根据队伍的不同进行销毁
	if (tank.GetComponent <tankData> ().getTeam () == Team.Blue) {
		teamB.Remove (tank);
		Destroy (tank);
	} else {
		teamR.Remove (tank);
		Destroy (tank);
	}
}

//子弹的爆炸
private void hitTank(GameObject bullet){
    //子弹爆炸效果的实例化，同上
	GameObject ex = Instantiate<GameObject> (bulletExplosion, bullet.transform.position, Quaternion.identity);
	ex.GetComponent<ParticleSystem> ().Play ();
	Destroy (ex, explosionTime);
}
```

- PlayerAction

> 玩家坦克的动作类，主要是响应键盘的动作

```C#
public class PlayerAction : MonoBehaviour{

	Director director;      //导演对象
	private int duration = 10;  //射击时间的间隔
    
    //前进
	public void moveForward(){
		this.GetComponent<Rigidbody> ().velocity = this.transform.forward * 20;
	}
    
    //后退
	public void moveBack(){
		this.GetComponent<Rigidbody> ().velocity = this.transform.forward * -20;
	}

    //转身
	public void turn(float offset){
		float y = this.transform.localEulerAngles.y + offset * 2;
		float x = this.transform.localEulerAngles.x;
		this.transform.localEulerAngles = new Vector3 (x, y, 0);
	}
	
    //射击
	public void shoot(){
	    //子弹预制的实例化
		GameObject bullet = Instantiate<GameObject> (this.GetComponent<tankData> ().bulletPrefab, this.transform.position + this.transform.forward * 3f, Quaternion.identity);
		//子弹所属队伍
		bullet.GetComponent<bulletAction> ().team = Team.Red;
		//子弹方向
		bullet.transform.forward = this.transform.forward;
		//给子弹一个前进的力（子弹不受重力）
		bullet.GetComponent<Rigidbody> ().AddForce (bullet.transform.forward * 35, ForceMode.Impulse);
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		director = Director.getInstance ();
		if (director != null && director.scence.getState() == State.Playing) {
			duration--;
			
			//通过键盘按钮进行动作
			if (Input.GetKey (KeyCode.W)) {
				moveForward ();
			} else if (Input.GetKey (KeyCode.S)) {
				moveBack ();
			} else {
				this.GetComponent<Rigidbody> ().velocity = new Vector3 (0, 0, 0);
			}
			if (Input.GetKey (KeyCode.Space)) {
				if (duration < 0) {
					duration = 10;
					shoot ();
				}
			}
			float offset = Input.GetAxis ("Horizontal1");
			turn (offset);
		} else {
		    //游戏结束时保持不动
			this.GetComponent<Rigidbody> ().velocity = Vector3.zero;
			this.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezeAll;
		}
	}
}
```

- EnemyAction

> 敌人的动作类，主要实现了AI的功能

```C#
public class EnemyAction : MonoBehaviour {

	Director director;  //导演对象
	public GameObject target;   //目标坦克
	State state;    //游戏状态
	private bool isIEnumerator = false; //是否进行了协程
	private bool canShoot = false;  //是否能进行射击
	
	void Start () {
		director = Director.getInstance ();
	}

	void Update () {
		state = Director.getInstance ().scence.getState();
		if (state == State.Playing) {
		    //判断是否能进行射击
			judgeShoot ();
			
			//判断是否进行协程
			if (!isIEnumerator) {
				StartCoroutine(shoot());
				isIEnumerator = true;
			}
			
			//当有目标坦克时进行导航
			if (target != null) {
				this.transform.LookAt (target.transform.position);
				this.GetComponent<NavMeshAgent> ().SetDestination (target.transform.position);
			}
			
			//当丢失目标时锁定另一个目标
			else if(director.scence.getTeamB().Count > 0 && director.scence.getTeamR().Count > 0) {
				getTarget ();
			}
		} 
		
		//游戏结束时保持不动
		else {
			this.GetComponent<NavMeshAgent> ().velocity = Vector3.zero;
			this.GetComponent<NavMeshAgent> ().ResetPath ();
			this.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezeAll;
		}
	}

    //使坦克不被撞飞
	void FixedUpdate(){
		this.GetComponent<Rigidbody> ().velocity = new Vector3(0, 0, 0);
	}
    
    //协程，当有目标，距离够近且可以射击时，1s射击一次
	IEnumerator shoot() {
		while(state == State.Playing) {
			for (float i = 1; i > 0; i -= Time.deltaTime) {
				yield return 0; 
			}
			
			//当距离小于200且可以射击时
			if (state == State.Playing && Vector3.Distance(this.transform.position, target.transform.position) < 100 && canShoot) {
			    //当距离小于20，静止不动
				if (Vector3.Distance (this.transform.position, target.transform.position) < 20) {
					this.GetComponent<NavMeshAgent> ().speed = 0;
				} 
				//否则以1的速度移动（默认是8）
				else {
					this.GetComponent<NavMeshAgent> ().speed = 1.0f;
				}
				
				//射击，和玩家射击同理，但注意子弹的瞄准是有误差的
				GameObject bullet = Instantiate<GameObject> (this.GetComponent<tankData> ().bulletPrefab, this.transform.position + this.transform.forward * 3f + new Vector3(0, 1, 0), Quaternion.identity);
				bullet.GetComponent<bulletAction> ().team = this.GetComponent<tankData>().getTeam();
				bullet.transform.forward = reloadPos( this.transform.forward);
				bullet.GetComponent<Rigidbody> ().AddForce (bullet.transform.forward * 20, ForceMode.Impulse);
			}
		}
	}

    //锁定目标
	void getTarget(){
		List<GameObject> targets;
		
		//得到另一支队伍的成员列表
		if (this.GetComponent<tankData> ().getTeam () == Team.Blue) {
			targets = director.scence.getTeamR ();
		} else {
			targets = director.scence.getTeamB ();
		}
		
		//随机抽取
		if (targets.Count == 0)
			return;
		int max = targets.Count;
		int random = 0;
		do {
			random = (int)Mathf.Floor(Random.Range(0, max));
		} while(targets [random] == null);
		target = targets [random];
	}
    
    //射击位置的误差
	Vector3 reloadPos(Vector3 pos){
	    //随机-1 到 1之间的数字
		float randomX = Random.Range (-1, 1);
		float randomZ = Random.Range (-1, 1);
		//加上误差
		return new Vector3 (pos.x + randomX / 6, pos.y, pos.z + randomZ / 6);
	}

    //判断是否可以射击，向目标发出一条射线如果射线直接到达目标而没有任何阻拦，则说明可以射击，否则不可以
	void judgeShoot(){
		if (target != null) {
			Ray ray = new Ray (this.transform.position, target.transform.position - this.transform.position);
			RaycastHit hit;  
			bool result = Physics.Raycast (ray, out hit, 1000);
			if (result) {
				if (hit.collider.gameObject.tag == "enemy" || hit.collider.gameObject.tag == "player") {
					canShoot = true;
					return;
				}
			}
			canShoot = false;
		}
	}
}

```

导航和用来检测的射线：

![gif](https://github.com/lossatsea/homework/blob/master/homework9/pictures/find%20(1).gif)

![gif](https://github.com/lossatsea/homework/blob/master/homework9/pictures/find%20(2).gif)

- bulletAction

> 子弹的动作类，主要进行触发器的判断

```C#
public class bulletAction : MonoBehaviour {

	public Team team;   //子弹的队伍

	void OnTriggerEnter(Collider other){
	    //如果集中的是坦克
		if (other.transform.GetComponent<tankData> ()) {
		    //如果击中的坦克是敌方，则触发击中事件，销毁子弹，hp-1
			if (other.transform.GetComponent<tankData> ().getTeam () != team) {
				Singleton<EventManager>.Instance.hitTank (gameObject);
				other.transform.GetComponent<tankData> ().hp--;
				if (other.transform.GetComponent<tankData> ().hp <= 0) {
					Singleton<EventManager>.Instance.destroyTank (other.gameObject);
				}
				Destroy (this.gameObject);
			}
		} 
		//如果击中的是建筑物，销毁
		else if (other.gameObject.tag == "building"){
			Destroy (this.gameObject);
		}
	}

	//超出游戏界面进行销毁
	void Update () {
		if (this.transform.position.x > 100 || this.transform.position.x < -100 || this.transform.position.z > 50 || this.transform.position.z < -50) {
			Destroy (this.gameObject);
		}
	}
}
```

- UserGUI

> 用来进行模式选择，和显示当前战况

```C#
public class UserGUI : MonoBehaviour {

	Director director = Director.getInstance(); //导演对象
	GUIStyle style1, style2, style3;    //3种GUI样式

	void OnGUI(){
        //红蓝双方的剩余坦克数
		GUI.Label (new Rect (Screen.width / 2 - 30, Screen.height - 60, 30, 50), director.scence.getTeamB().Count.ToString (), style2);
		GUI.Label (new Rect (Screen.width / 2, Screen.height - 60, 30, 50), director.scence.getTeamR().Count.ToString (), style1);
		GUI.Label (new Rect (Screen.width / 2 - 5, Screen.height - 60, 10, 50), ":", style3);
		
		//准备状态时，对游戏模式进行选择
		if (director.scence.getState () == State.Ready) {
			if (GUI.Button (new Rect (Screen.width / 2 - 200, Screen.height / 2 - 20, 150, 40), "Auto")) {
				director.scence.startWithAuto ();
			} else if (GUI.Button (new Rect (Screen.width / 2 + 200, Screen.height / 2 - 20, 150, 40), "Play")) {
				director.scence.startWithPlay ();
			}
		} 
		//游戏中
		else if (director.scence.getState () == State.Playing) {
		    //显示红队的成员的HP
			for (int i = 0; i < director.scence.RedHPs ().Length; i++) {
				GUI.Label (new Rect (Screen.width - i * 20 - 20, Screen.height - 60, 20, 50), director.scence.RedHPs () [i].ToString (), style1);
			}
			//显示蓝队的成员的HP
			for (int i = 0; i < director.scence.BlueHPs ().Length; i++) {
				GUI.Label (new Rect (i * 20 + 20, Screen.height - 60, 20, 50), director.scence.BlueHPs () [i].ToString (), style2);
			}
		} 
		//游戏结束，显示哪队的和、胜利
		else if (director.scence.getState () == State.Win) {
			GUI.Label (new Rect (Screen.width / 2 - 100, Screen.height / 2 - 30, 200, 60), "Team Red Win", style1);
		} else{
			GUI.Label (new Rect (Screen.width / 2 - 100, Screen.height / 2 - 30, 200, 60), "Team Blue Win", style2);
		}
	}
	
	//3种样式的初始化
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
}

```

- Singleton<T>

> 单例模式 

```C#
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
	protected static T instance;
	public static T Instance {
		get {
			if (instance == null) {
				instance = (T)FindObjectOfType (typeof(T));
				if (instance == null) {
					Debug.LogError ("An instance of " + typeof(T) + " is needed in the scene, but there is none.");
				}
			}
			return instance;
		}
	}
}
```

### 4. 视频演示

[video](http://www.iqiyi.com/w_19ryzsugl1.html)
