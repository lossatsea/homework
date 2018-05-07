## 智能巡逻兵

### 游戏基本内容

1. 玩家

    - 玩家操控人物在地图中移动
    - 靠近巡逻兵一定距离会被追赶，摆脱后得1分
    - 被巡逻兵抓到游戏结束
    - 吃到蓝色水晶得2分
    - 吃到紫色水晶得5分
    - 每轮游戏开始玩家会在复活区域开始游戏

2. 巡逻兵

    - 每个区域有2个巡逻兵
    - 巡逻兵无法跨越区域，当碰到每个区域的出入口时被消灭，在所属区域内重新生成新的的巡逻兵，进行巡逻
    - 以一个随机的四边形路线巡逻
    - 碰到墙壁后重新生成巡逻路线
    - 当玩家靠近到一定距离会追赶玩家，被摆脱后继续巡逻
    - 抓到玩家游戏结束
    
3. 水晶

    - 有两种水晶，蓝色水晶2分，紫色水晶5分
    - 普通区域有1个紫水晶，3个蓝水晶
    - 特殊区域有3个紫水晶
    - 被玩家吃后，当局游戏不再重新生成
    - 每局游戏开局自动生成，出现位置确定
    
4. 地图

    - 如图
    - 最上端区域为安全区域，不会出现巡逻兵，玩家会在安全区域开始游戏
    - 中间区域为特殊区域，只有一个出入口。
    - 其余区域为普通区域

### 游戏编程方法

- 单例模式
- 工厂模式
- 动作管理
- 订阅与发布模式

### 游戏素材

1. 玩家
2. 巡逻兵
3. 天空盒
4. 墙贴图
5. 自制预制

当然素材下载下来之后还要进行各种调整来符合去求

### 设置上的小技巧

1. 出入口的设置

    出入口需要能通过且能感知到，因此可以用空游戏对象，加上触发器，并设置其标签为Gate。这样可以让角色通过，并且可以通过角色的触发器感知到出入口的存在

2. 巡逻兵对出入口的感知

    巡逻兵的根节点上有一个触发器，用来感知玩家的。但这个触发器范围太广，会导致离出入口还很远就感知到出入口，然后被消灭掉。
    
    因此不要把感知出入口的函数放在根节点，可以放在子节点，调整该节点的触发器范围小一点，这样就可以让巡逻兵在足够靠近出入口时感知到，然后被消灭。
    
3. 摄像机的跟随

    当将摄像机跟随的目标设为玩家角色时，发现怎么调整都不能得到令人满意的第三人称视角。
    
    我就在玩家角色中增加了一个子对象cube，去掉渲染，放在头顶合适的位置。让摄像跟随的目标变成这个cube，就可以实现满意的跟随效果。

### 游戏主要代码分析

- CamareFollow.cs
    
> 实现摄像机的跟随，平滑移动

```C#
public Transform target;
public float distanceH = 10f;
public float distanceV = 5f;
public float smoothSpeed = 10f; //平滑参数

void LateUpdate()
{
	Vector3 nextpos = target.forward * -1 * distanceH + target.up * distanceV + target.position;
	this.transform.position =Vector3.Lerp(this.transform.position, nextpos, smoothSpeed * Time.deltaTime); 
	this.transform.LookAt(target);
}
```

- PatrolData.cs

> 巡逻兵的数据

```C#
public GameObject player;   //玩家
public float speed;         //速度
public bool isFollow = false;   //是否跟随玩家
public bool isGate = false;     //是否撞到区域的出入口
public bool isWall = false;     //是否撞到墙
public int area;            //所在的区域
```

- PatrolFactory.cs

> 巡逻兵工厂，用于生成与回收巡逻兵

getPatrol，用于根据区域在该区域中随机生成巡逻兵

```C#
public GameObject getPatrol(int area){
	GameObject newPatrol = null;
	float baseX = 0;
	float baseZ = 0;
	
	//根据区域确定基本坐标
	switch (area) {
	case 0:
		baseX = -100;
		baseZ = -50;
		break;
	case 1:
		baseX = 80;
		baseZ = -180;
		break;
	case 2:
		baseX = 250;
		baseZ = -180;
		break;
	case 3:
		baseX = 250;
		baseZ = -50;
		break;
	case 4:
		baseX = 50;
		baseZ = -50;
		break;
	}
	
	//当巡逻兵有库存时
	if (free.Count > 0) {
		newPatrol = free [0].gameObject;
		
		//在基本坐标的基础上生成有随机的偏移坐标
		float dev = Random.Range(-20, 20);
		newPatrol.transform.position = new Vector3(baseX + dev, 0, baseZ + dev);
		free.Remove (free[0]);
	} else {
	
	    //在基本坐标的基础上生成有随机的偏移坐标
		float dev = Random.Range(-20, 20);
		newPatrol = Instantiate<GameObject> (patrolPrefab, new Vector3(baseX + dev, 0, baseZ + dev), Quaternion.identity);
	}
	
	newPatrol.SetActive (true);
	newPatrol.GetComponent<PatrolData> ().area = area;
	
	//加入使用中队列
	used.Add (newPatrol.GetComponent<PatrolData>());
	return newPatrol;
}
```

freePatrol方法，回收巡逻兵，与Hit UFO中基本相同

```C#
public void freePatrol(GameObject oldPatrol){
	for (int i = 0; i < used.Count; i++) {
		if (used [i].gameObject == oldPatrol) {
			PatrolData move = used[i];
			used.Remove (move);
			free.Add (move);
			return;
		}
	}
	Debug.Log ("Exception: No such disk int used list");
}
```

- Director.cs

> 单例的导演类

```C#
public ScenceController scence;     //场景控制器
private static Director instance;   //单例
public bool gameOver = false;       //游戏状态

public static Director getInstance(){
	if (instance == null) {
		instance = new Director ();
	}
	return instance;
}

//开始游戏
public void gameStart(){
	scence.gameStart ();
}
```

- GameEventManager.cs

> 事件发布类，连接触发条件和响应方法

```C#
//游戏结束事件
public delegate void GameoverEvent();
public static event GameoverEvent GameoverChange;

//玩家逃脱事件
public delegate void AddscoreEvent();
public static event GameoverEvent Addscore;

//巡逻兵撞到出入口事件
public delegate void GateeEvent(GameObject monster);
public static event GateeEvent Gatecollide;

//吃到水晶事件
public delegate void DiamondEvent(GameObject diamond);
public static event GateeEvent Diamondcollide;


public void PlayerGameover()
{
	if (GameoverChange != null)
	{
		GameoverChange();
	}
}

public void AddScore(){
	if (Addscore != null) {
		Addscore ();
	}
}

public void GateCollide(GameObject monster){
	if (Gatecollide != null) {
		Gatecollide (monster);
	}
}

public void DiamondCollide(GameObject diamond){
	if (Diamondcollide != null) {
		Diamondcollide (diamond);
	}
}
```

- ScenceController.cs

> 场景控制器，在这里的功能有加载预制，控制玩家的移动，订阅者，控制游戏开始和结束，功能较多这里分开分析

1. 初始化

    Start方法

    ```C#
    void Start ()
    {
        //单例们的获取
    	director = Director.getInstance ();
    	factory = Singleton<PatrolFactory>.Instance;
    	board = Singleton<ScoreBoard>.Instance;
    	director.scence = this;
    	
    	//巡逻动作的管理层
    	manager = gameObject.AddComponent<PatrolActionManager>() as PatrolActionManager;
    	
    	//设置玩家
    	gameObject.GetComponent<PatrolActionManager> ().player = player;
    	
    	//生成巡逻兵，并赋予巡逻动作
    	for (int i = 0; i < 10; i++) {
    		GameObject monster = factory.getPatrol (i / 2);
    		monsters.Add (monster);
    		manager.beginPatrol (monster);
    	}
    	
    	//设置水晶的位置
    	getLocations (5);
    	
    	//加载其他预制
    	reloadSource ();
    }
    ```

   getLocations方法，只运行一次，初始化19个水晶的位置

    ```C#
    //height表示水晶的Y轴，浮在空中
    private void getLocations(float height){
        
        //7个紫水晶的位置
    	diamondLocations.Add (new Vector3(-175, height, -80));
    	diamondLocations.Add (new Vector3(115, height, -130));
    	diamondLocations.Add (new Vector3(25, height, -70));
    	diamondLocations.Add (new Vector3(80, height, -45));
    	diamondLocations.Add (new Vector3(125, height, -80));
    	diamondLocations.Add (new Vector3(200, height, -140));
    	diamondLocations.Add (new Vector3(215, height, -70));
    	
    	//12个蓝水晶的位置
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
    ```
    
2. 加载预制部分
    
    reloadSource方法，加载墙壁，Plane，水晶的预制，并给墙壁的所有子对象进行贴图，紧跟着是相应的函数

    ```C#
    private void reloadSource(){
        //加载紫水晶
    	reloadVioletDiamonds ();
    	
    	//加载蓝水晶
    	reloadBlueDiamonds ();
    	
    	//加载地图（墙壁）
    	GameObject wall = Instantiate<GameObject> (wallPrefab, new Vector3(0, 0, 0), Quaternion.identity);
    	
    	//遍历墙壁进行贴图
    	findAllSon (wall.transform);
    	
    	//加载Plane
    	Instantiate<GameObject> (planePrefab, new Vector3(0, 0, 0), Quaternion.identity);
    }
    ```

    ```C#
    //给墙壁贴图
    private void findAllSon(Transform wall){
    	for (int i = 0; i < wall.transform.childCount; i++) {
    		if (wall.GetChild (i).transform.childCount > 0) {
    		    //递归
    			findAllSon (wall.transform.GetChild (i));
    		} else if(wall.GetChild (i).tag == "Wall"){
    		    //贴图
    			wall.GetChild (i).GetComponent<MeshRenderer> ().material = brickMateria;
    		}
    	}
    }
    //加载紫水晶
    private void reloadVioletDiamonds(){
    	for (int i = 0; i < 7; i++) {
    		diamonds[i] = Instantiate<GameObject> (pointObjects[1], diamondLocations[i], Quaternion.identity);
    	}
    }
    //加载蓝水晶
    private void reloadBlueDiamonds(){
    	for (int i = 7; i < 19; i++) {
    		diamonds[i] = Instantiate<GameObject> (pointObjects[0], diamondLocations[i], Quaternion.identity);
    	}
    }
    ```

3. 订阅部分

    注册事件与取消注册事件
    
    ```C#
    //注册事件
    void OnEnable(){
		GameEventManager.GameoverChange += GameOver;
		GameEventManager.Addscore += addScore;
		GameEventManager.Gatecollide += addMonster;
		GameEventManager.Diamondcollide += getDiamond;
	}
    //取消注册事件
	void OnDisable()
	{
		GameEventManager.GameoverChange -= GameOver;
		GameEventManager.Addscore -= addScore;
		GameEventManager.Gatecollide -= addMonster;
		GameEventManager.Diamondcollide -= getDiamond;
	}
    ```
    
    事件响应
    
    ```C#
    //巡逻兵抓到玩家的响应
    private void GameOver(){
        //防止玩家被撞飞
		player.transform.position = new Vector3(player.transform.position.x, 0, player.transform.position.z);
		player.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezeAll;
		director.gameOver = true;
		
		//停止所有巡逻兵的动作
		manager.stop ();
	}
	
	//玩家逃脱巡逻兵的响应
	private void addScore(){
	    //计分板加分
		board.escaped ();
	}
	
	//巡逻兵撞到区域出入口的响应，重新生成一个巡逻兵
	private void addMonster(GameObject monster){
		monsters.Remove (monster);
		GameObject newMonster = factory.getPatrol (monster.GetComponent<PatrolData> ().area);
		monsters.Add (newMonster);
		manager.beginPatrol (newMonster);
	}
	
	//玩家吃到水晶的响应
	private void getDiamond(GameObject diamond){
		if (diamond.transform.tag == "Point0") {
		    //计分板蓝水晶加分
			board.getBlueDiamond ();
		} else if (diamond.transform.tag == "Point1") {
		    //计分板紫水晶加分
			board.getVioletDiamond ();
		}
	}
    ```
    
4. 玩家移动部分

    与UIIteraction.cs的互动，控制玩家角色的移动，旋转和动画的播放
    
    ```C#
    public void MovePlayer(float translationX, float translationZ)
	{
		if(!director.gameOver)
		{
		    //有按键
			if (translationX != 0 || translationZ != 0)
			{
			    //切换到Run动画
				player.GetComponent<Animator> ().SetBool ("isRun", true);
			}
			else
			{
			    //切换到Stand动画
				player.GetComponent<Animator> ().SetBool ("isRun", false);
			}
			
			//移动和旋转
			player.transform.Translate(0, 0, translationZ * player_speed * Time.deltaTime);
			player.transform.Rotate(0, translationX * rotate_speed * Time.deltaTime, 0);
			
			//防止角色因碰撞产生异动
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
    ```

5. 公有方法部分

    gameStart方法，重新开始游戏。主要是水晶和巡逻兵的初始化，分数的清零，玩家角色的重置
    
    ```C#
    public void gameStart(){
        //变量的清零
		for (int i = 0; i < 19; i++) {
			if (diamonds [i] != null) {
				Destroy (diamonds[i]);
			}
		}
		board.clear ();
		monsters.Clear ();
		
		//巡逻兵的重置
		manager.destroy ();
		for (int i = 0; i < 10; i++) {
			GameObject monster = factory.getPatrol (i / 2);
			monsters.Add (monster);
			manager.beginPatrol (monster);
		}
		
		//玩家角色的重置
		player.transform.position = new Vector3 (50, 0, 50);
		player.transform.localEulerAngles = new Vector3 (0, 180, 0);
		player.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.None;
		
		//水晶的初始化
		reloadBlueDiamonds ();
		reloadVioletDiamonds ();
	}
    ```
    
    getScore方法，得到分数
    
    ```C#
    public int getScore(){
		return board.Score();
	}
    ```
    
    getDiamonds方法，得到两种水晶的数量
    
    ```C#
    public int[] getDiamonds(){
		int[] diamonds = new int[2];
		diamonds [0] = board.Blue ();
		diamonds [1] = board.Violet ();
		return diamonds;
	}
    ```

- SSActionManager.cs

> 动作管理器，重点是callback回调函数，在动作结束后的处理

```C#
public void SSActionEvent (SSAction source, 
	SSActionEventType events = SSActionEventType.Competeted,
	int intParam = 0,
	string strParam = null,
	Object objectParam = null){
	
	//巡逻事件
	if (source is PatrolAction) {
	    
	    //初始化巡逻兵的数据
		source.gameobject.GetComponent<PatrolData> ().isGate = false;
		source.gameobject.GetComponent<PatrolData> ().isWall = false;
		source.gameobject.GetComponent<PatrolData> ().isFollow = false;
		source.gameobject.GetComponent<Animator>().SetBool("isRun", false);
		source.gameobject.GetComponent<Animator>().SetBool("isWalk", false);
		source.gameobject.SetActive (false);
		
		//工厂回收
		director.scence.factory.freePatrol (source.gameobject);
	}
}
```

另外还有在具体动作管理器中用到的，针对所有动作的方法

```C#
//消灭所有动作
public void DestroyAll()
{
	foreach (KeyValuePair<int, SSAction> kv in actions)
	{
		SSAction ac = kv.Value;
		ac.destroy = true;
		SSActionEvent (ac);
	}
}

//所有巡逻兵停止动作，站立
public void StopAll(){
	foreach (KeyValuePair<int, SSAction> kv in actions)
	{
		SSAction ac = kv.Value;
		ac.stand ();
	}
}

```

- PatrolActionManager.cs

> 具体动作管理器，相当于之前的CCActionManager，主要是管理具体的动作，这里具体动作只有PatrolAction，巡逻动作，这一个。

```C#
public class PatrolActionManager :SSActionManager
{
	PatrolAction patrol;
	public GameObject player;

    //赋予一个巡逻兵巡逻事件
	public void beginPatrol(GameObject patrolman){
		patrol = PatrolAction.GetSSAction (player);
		RunAction (patrolman, patrol, this);
	}

    //消灭所用动作，在游戏重新开始时调用
	public void destroy(){
		DestroyAll ();
	}

    //停止所有动作
	public void stop(){
		StopAll ();
	}
}
```

- PatrolAction.cs

> 具体的巡逻动作，主要有生成巡逻路线，巡逻，追赶玩家，撞到墙，站立等的响应

```C#
public class PatrolAction : SSAction
{
	private List<Vector3> path = new List<Vector3>();   //巡逻路线，由4个点组成
	private GameObject player;  //玩家
	private int posNum = 1;     //下一个路线点的下标
	private float speed;        //速度
	private Vector3 pos;        //当前位置
	private bool isStand = false;   //是否站立


	public static PatrolAction GetSSAction(GameObject player)
	{
		PatrolAction action = CreateInstance<PatrolAction>();
		action.player = player;
		return action;
	}
	
	// 初始化速度，位置和路径
	public override void Start ()
	{
		speed = gameobject.GetComponent<PatrolData> ().speed;
		pos = gameobject.transform.position;
		getPath ();
	}
```

Update方法包含了所有的判断

```C#
	// Update is called once per frame
	public override void Update ()
	{
	    //是否站立
		if (!isStand) {
		
		    //默认是行走动画
			pos = gameobject.transform.position;
			gameobject.GetComponent<Animator>().SetBool("IsWalk", true);
			
			//当撞到区域的出入口，动作结束
			if (gameobject.GetComponent<PatrolData> ().isGate) {
				destroy = true;
				callback.SSActionEvent (this);
			} else {
			
			    //防止因桩已发生旋转移动
				if (transform.localEulerAngles.x != 0 || transform.localEulerAngles.z != 0) {
					transform.localEulerAngles = new Vector3 (0, transform.localEulerAngles.y, 0);
				}            
				if (transform.position.y != 0) {
					transform.position = new Vector3 (transform.position.x, 0, transform.position.z);
				}
				
				//是否跟随
				if (!gameobject.GetComponent<PatrolData> ().isFollow) {
				
					gameobject.GetComponent<Animator> ().SetBool ("IsRun", false);
					
					//是否撞到墙
					if (gameobject.GetComponent<PatrolData> ().isWall) {
					
					    //重新生成路径
						getPath ();
						gameobject.GetComponent<PatrolData> ().isWall = false;
					}
					
					//巡逻
					if (path.Count > 0) {
						Patrol ();
					}
				} else {
				    
				    //切换到跑步动画，提升速度，面向玩家，追赶
					gameobject.GetComponent<Animator> ().SetBool ("IsRun", true);
					gameobject.transform.LookAt (player.transform.position);
					gameobject.transform.position = Vector3.MoveTowards (gameobject.transform.position, player.transform.position, speed * 8 * Time.deltaTime);
				}
			}
		}
	}
```

Patrol方法是巡逻的具体动作，getPath方法以当前位置为基准重新生成随机路径

```C#

	private void Patrol(){
	
	    //循环巡逻这个路径
		for (int i = 0; i < 4; i++) {
			if (gameobject.transform.position == path [i]) {
				if (i == 3) {
					posNum = 0;
				} else {
					posNum = i + 1;
				}
				break;
			}
		}
		
		//面朝下一个路径点移动
		gameobject.transform.LookAt (path[posNum]);
		gameobject.transform.position = Vector3.MoveTowards (pos ,path[posNum], speed * Time.deltaTime);
	}

	private void getPath(){
		path.Clear ();
		
		//第一个点是当前位置
		path.Add (pos);
		
		//保证随机数不会太小
		int length1 = Random.Range (-15, 15);
		while (Mathf.Abs (length1) <= 5) {
			length1 = Random.Range (-15, 15);
		}
		int length2 = Random.Range (-18, 18);
		while (Mathf.Abs (length2) <= 5) {
			length2 = Random.Range (-15, 15);
		}
		Vector3 location1 = new Vector3 (pos.x + length1, 0, pos.z + length2);
		path.Add (location1);
		Vector3 location2 = new Vector3 (location1.x - length2, 0, location1.z + length1);
		path.Add (location2);
		Vector3 location3 = new Vector3 (location2.x - length1, 0, location2.z - length2);
		path.Add (location3);
	}
```

Stand方法切换到站立动画

```C#

	public override void stand(){
		isStand = true;
		gameobject.GetComponent<Animator> ().SetTrigger ("Stand");
	}
}
```

- CollisionEvent.cs

> 挂载到巡逻兵的根节点上的碰撞类，用于检测撞墙和感应玩家

```C#
void OnCollisionEnter(Collision colliser)
{
    //抓到玩家，切换动画到攻击，触发事件
	if (colliser.gameObject.tag == "Player") {
		colliser.gameObject.GetComponent<Animator> ().SetBool("isRun", false);
		this.GetComponent<Animator> ().SetTrigger ("Attack_1");
		Singleton<GameEventManager>.Instance.PlayerGameover ();
	} 
	
	//撞到墙，将isWall修改为true
	else if (colliser.gameObject.tag == "Wall") {
		this.GetComponent<PatrolData> ().isWall = true;
	}
}

void OnTriggerEnter(Collider collider)
{
    //感知到玩家，进行跟随
	if (collider.gameObject.tag == "Player")
	{
		this.gameObject.transform.GetComponent<PatrolData>().isFollow = true;
	}
}
void OnTriggerExit(Collider collider)
{
    //玩家逃脱，触发事件
	if (collider.gameObject.tag == "Player")
	{
		this.gameObject.transform.GetComponent<PatrolData> ().isFollow = false;
		Singleton<GameEventManager>.Instance.AddScore ();
	}
}
```

- GateTrigger.cs

> 挂载到巡逻兵第一个子节点上的触发器类，用于检测是否撞到出入口

```C#
void OnTriggerEnter(Collider collider)
{
    //撞到出入口，更改isGate，触发事件
	if(collider.gameObject.tag == "Gate"){
		this.gameObject.transform.parent.GetComponent<PatrolData> ().isGate = true;
		Singleton<GameEventManager>.Instance.GateCollide(this.gameObject.transform.parent.gameObject);
	}
}
```

- PlayerCollision.cs

> 挂载到玩家角色根节点上的碰撞类，用于检测是否吃到水晶

```C#
void OnCollisionEnter(Collision colliser)
{
    //吃到水晶，触发事件并消灭该水晶
	if (colliser.transform.tag == "Point0" || colliser.transform.tag == "Point1") {
		Singleton<GameEventManager>.Instance.DiamondCollide (colliser.gameObject);
		Destroy (colliser.gameObject);
	}
}
```

- UIIteraction.cs

> 与玩家的交互界面，主要有角色的移动和GUI组件

```C#
public class UIIteraction : MonoBehaviour
{
	private Director director;
	
	//游戏规则
	private string message = 
		"Game Rule:\n" +
		"\nEscape from monster one time: plus 1" +
		"\nGet a blue diamond: plus 2" +
		"\nGet a violet diamond: plus 5" +
		"\nBe arrested by monster: Game Over" +
		"\n\nGood Luck!"
		;

	void Start ()
	{
		director = Director.getInstance ();
	}
	
	void Update ()
	{
	    //控制角色的移动
		director = Director.getInstance ();
		if (director != null) {
			float translationX = Input.GetAxis("Horizontal");
			float translationZ = Input.GetAxis("Vertical");
			director.scence.MovePlayer(translationX, translationZ);
		}
	}

	void OnGUI(){
	    //字体设置
		GUIStyle scoreStyle = new GUIStyle ();
		scoreStyle.alignment = TextAnchor.MiddleCenter;
		GUIStyle messageStyle = new GUIStyle ();
		messageStyle.alignment = TextAnchor.MiddleLeft;
		messageStyle.fontSize = 14;
		
		//游戏结束界面，出现分数详情
		if (director.gameOver) {
			scoreStyle.fontSize = 12;
			GUI.Label (new Rect(Screen.width / 2 - 100, 50, 200, 80), "Score: " + director.scence.getScore() 
				+ "\nBlue Diamond: " + director.scence.getDiamonds()[0] 
				+ "\nViolet Diamond: " + director.scence.getDiamonds()[1], scoreStyle);
			if (GUI.Button (new Rect (Screen.width / 2 - 50, Screen.height / 2 - 30, 100, 60), "Game Start") ){
			
			    //游戏重新开始
				director.gameOver = false;
				director.gameStart ();
			}
		} 
		//游戏进行界面，左上角为游戏规则，右上角为分数详情
		else {
			scoreStyle.fontSize = 16;
			GUI.Label (new Rect(Screen.width - 200, 20, 200, 20), "Score: " + director.scence.getScore() 
				+ "\nBlue Diamond: " + director.scence.getDiamonds()[0] 
				+ "\nViolet Diamond: " + director.scence.getDiamonds()[1], scoreStyle);
			GUI.Label (new Rect(5, 10, 200, 100), message, messageStyle);
		}
	}
}
```

- 完整代码在[这里]()

### [视频链接]()
