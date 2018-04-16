### 编写一个简单的鼠标打飞碟（Hit UFO）游戏
#### 1. 游戏基本内容
- 游戏有 4 个 round，每个 round 都包括10 次 trial
- 每个 trial 的飞碟有随机性，总体难度随 round 上升

  round|颜色|速度|半径|方向|发射位置
  --|--|--|--|--|--
  1|绿色|10|2.5|任意|任意
  2|蓝色|12|2.3|任意|任意
  3|黄色|15|2.2|任意|任意
  4|红色|17|2|任意|任意
- 鼠标点中得分，得分规则：
 
  round|每个飞碟的基本分
  --|--
  1|1
  2|2
  3|3
  4|4

  在基本分的基础上，发射角度越远离45，加分越多
#### 2. 代码分析
- 基本要求
    - 使用带缓存的工厂模式管理不同飞碟的生产与回收，该工厂必须是场景单实例的！具体实现见参考资源 Singleton 模板类
    - 近可能使用前面 MVC 结构实现人机交互与游戏模型分离
- **Singleton.cs（单例模板）**

    ``` C#
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class Singleton<T> : MonoBehaviour where T: MonoBehaviour{
        //单例
    	protected static T instance;
        //返回单例
    	public static T Instance{
    		get{
    			if (instance == null) {
    				instance = (T)FindObjectOfType (typeof(T));
    				if (instance == null) {
    					Debug.LogError ("An instance of " + typeof(T) +
    					" is needed in the scence, but there is none.");
    				}
    			}
    			return instance;
    		}
    	}
    }
    ```

- A**llIterface.cs（公共接口）**

    ``` c#
    using UnityEngine;
    using System.Collections;
    //动作基本类型枚举
    public enum SSActionEventType:int {Started, Competeted}
    //游戏状态基本类型枚举
    public enum GameState:int {Over, Playing, Init}
    //事件处理接口
    public interface ISSActionCallback{
    	void SSActionEvent (SSAction source, 
    		SSActionEventType events = SSActionEventType.Competeted,
    		int intParam = 0,
    		string strParam = null,
    		Object objectParam = null);
    }
    ```
    
- **DiskData.cs（飞碟基本属性）**

    ``` C#
    using UnityEngine;
    using System.Collections;
    
    public class DiskData : MonoBehaviour
    {
    
    	public Vector3 size;
    	public Color color;
    	public float speed;
    	private Vector3 direction;
    
    	public void setDir(Vector3 dir){
    		direction = dir;
    	}
    
    	public Vector3 getDir(){
    		return direction;
    	}
    }
    ```

- **DiskFactory.cs（飞碟工厂）**

    变量成员
    ``` C#
    //飞碟预制
    public GameObject diskPrefab;
    //存放正在使用的飞碟
	private List<DiskData> used = new List<DiskData>();
	//存放未使用的飞碟
	private List<DiskData> free = new List<DiskData>();
    ```
    getDisk函数（输出飞碟）
    ``` c#
    //studio：判断输出符合round的飞碟
    public GameObject getDisk(int studio){
        //输出的飞碟
		GameObject newDisk = null;
		
	    //如果free中有飞碟，拿出来，没有就实例化预制
		if (free.Count > 0) {
			newDisk = free [0].gameObject;
		    //随机y坐标
			float y = Random.Range(-10, 0);
			newDisk.transform.position = new Vector3 (-30, y, 0);
			free.Remove (free[0]);
		} else {
		    //随机y坐标
			float y = Random.Range(-10, 0);
			newDisk = Instantiate<GameObject> (diskPrefab, new Vector3(-30, y, 0), Quaternion.identity);
		}
		
	    //选择符合round的studio
		switch (studio) {
		case 1:
			return studio1 (newDisk);
		case 2:
			return studio2 (newDisk);
		case 3:
			return studio3 (newDisk);
		case 4:
			return studio4 (newDisk);
		default:
			return null;
		}
	}
    ```
    freeDisk函数（储存飞镖）
    ``` C3
    //是否是used中的飞镖
    for (int i = 0; i < used.Count; i++) {
		if (used [i].gameObject == oldDisk) {
			DiskData move = used[i];
			used.Remove (move);
			free.Add (move);
			return;
		}
	}
	//不是的话就新加入free
	free.Add (oldDisk.GetComponent<DiskData>());
    ```
    
    studio函数（工作室，返回符合round的飞镖）
    ``` C#
    //返回round1的飞碟
    private GameObject studio1(GameObject newDisk){
        //随机方向
		Vector3 direction = new Vector3 ();
		direction.z = 0;
		direction.x = 1;
		direction.y = Random.Range (0.2f, 0.7f);
	    //设置颜色
		newDisk.transform.GetComponent<DiskData> ().color = Color.green;
		newDisk.transform.GetComponent<Renderer> ().material.color = newDisk.transform.GetComponent<DiskData> ().color;
	    //设置速度
		newDisk.transform.GetComponent<DiskData> ().speed = 10.0f;
	    //设置大小
		newDisk.transform.GetComponent<DiskData> ().size = new Vector3 (2.5f, 2.5f, 2.5f);
		newDisk.transform.localScale = newDisk.transform.GetComponent<DiskData> ().size;
		newDisk.transform.GetComponent<DiskData> ().setDir (direction);
		used.Add (newDisk.transform.GetComponent<DiskData> ());
		return newDisk;
	}

    //返回round2的飞碟
	private GameObject studio2(GameObject newDisk){
		Vector3 direction = new Vector3 ();
		direction.z = 0;
		direction.x = 1;
		direction.y = Random.Range (0.2f, 0.7f);
		newDisk.transform.GetComponent<DiskData> ().color = Color.blue;
		newDisk.transform.GetComponent<Renderer> ().material.color = newDisk.transform.GetComponent<DiskData> ().color;
		newDisk.transform.GetComponent<DiskData> ().speed = 12.0f;
		newDisk.transform.GetComponent<DiskData> ().size = new Vector3 (2.3f, 2.3f, 2.3f);
		newDisk.transform.localScale = newDisk.transform.GetComponent<DiskData> ().size;
		newDisk.transform.GetComponent<DiskData> ().setDir (direction);
		used.Add (newDisk.transform.GetComponent<DiskData> ());
		return newDisk;
	}

    //返回round3的飞碟
	private GameObject studio3(GameObject newDisk){
		Vector3 direction = new Vector3 ();
		direction.z = 0;
		direction.x = 1;
		direction.y = Random.Range (0.2f, 0.7f);
		newDisk.transform.GetComponent<DiskData> ().color = Color.yellow;
		newDisk.transform.GetComponent<Renderer> ().material.color = newDisk.transform.GetComponent<DiskData> ().color;
		newDisk.transform.GetComponent<DiskData> ().speed = 15.0f;
		newDisk.transform.GetComponent<DiskData> ().size = new Vector3 (2.2f, 2.2f, 2.2f);
		newDisk.transform.localScale = newDisk.transform.GetComponent<DiskData> ().size;
		newDisk.transform.GetComponent<DiskData> ().setDir (direction);
		used.Add (newDisk.transform.GetComponent<DiskData> ());
		return newDisk;
	}

    //返回round4的飞碟
	private GameObject studio4(GameObject newDisk){
		Vector3 direction = new Vector3 ();
		direction.z = 0;
		direction.x = 1;
		direction.y = Random.Range (0.2f, 0.7f);
		newDisk.transform.GetComponent<DiskData> ().color = Color.red;
		newDisk.transform.GetComponent<Renderer> ().material.color = newDisk.transform.GetComponent<DiskData> ().color;
		newDisk.transform.GetComponent<DiskData> ().speed = 17.0f;
		newDisk.transform.GetComponent<DiskData> ().size = new Vector3 (2, 2, 2);
		newDisk.transform.localScale = newDisk.transform.GetComponent<DiskData> ().size;
		newDisk.transform.GetComponent<DiskData> ().setDir (direction);
		used.Add (newDisk.transform.GetComponent<DiskData> ());
		return newDisk;
	}
    ```

- **Director.cs（场景总控制类，实现模块分离）**

    ``` C#
    using UnityEngine;
    using System.Collections;
    //单例模式
    public class Director : System.Object 
    {
        //ScenceController对象
    	public ScenController scence;
        //Directo单例
    	private static Director instance;
        //返回单例
    	public static Director getInstance(){
    		if (instance == null) {
    			instance = new Director ();
    		}
    		return instance;
    	}
    }
    ```
    
- **ScenController.cs（场景控制类）**

    变量成员
    ``` C#
    //总控制类
    Director director;
    
    //公有成员
    //飞碟工厂对象
	public DiskFactory factory;
	//游戏状态
	public GameState state = GameState.Init;
	//动作管理对象
	public CCActionManager manager = null;
    
    //私有成员
    //回合
	private int round = 1;
	//记分板对象
	private ScoreController scoreBoard;
	当前飞碟数
	private int diskNum = 10;
	//发射间隔时间
	private float interval = 0;
	//Stack储存将要发射飞碟
	private Stack<GameObject> disks = new Stack<GameObject>();
    ```
    Start函数
    ``` C#
    void Start ()
	{
	    //单例化工厂
		factory = Singleton<DiskFactory>.Instance;
	    //单例化记分板
		scoreBoard = Singleton<ScoreController>.Instance;
	    //初始化总控制对象
		director = Director.getInstance ();
		director.scence = this;
	}
    ```
    Update函数
    ``` c#
    void Update ()
	{
	    //确保动作管理对象存在
		if (manager != null) {
		    //进入下一回合
			if (disks.Count == 0 && state == GameState.Playing && diskNum == 0) {
				nextRound ();
			}
			//当场景中还有飞碟时
			if (diskNum > 0 && disks.Count > 0) {
			    //游戏进行时，时间间隔大于1就发射飞碟
				if (interval >= 1 && state == GameState.Playing) {
					GameObject target = disks.Pop();
					manager.throwDisk (target);
					interval = 0;
				}
			    //游戏进行时，时间间隔小于1就继续计时
				else if (interval < 1 && state == GameState.Playing) {
					interval += 1 * Time.deltaTime;
				}
			}
		}
	}
    ```
    其他函数
    ``` C#
    //Disk数量减一
    public void subDisknum(){
		diskNum--;
	}
    
    //返回当前分数
	public float getScore(){
		return scoreBoard.score;
	}

    //返回当前回合数
	public int getRound(){
		return round;
	}

    //进入下一回合
	private void nextRound(){
	    //4回合时结束游戏
		if (round == 4) {
			state = GameState.Over;
			return;
		}
		diskNum = 10;
		round++;
		for (int i = 0; i < 10; i++) {
			disks.Push (factory.getDisk(round));
		}
	}

    //初始化游戏
	public void gameStart(){
		diskNum = 10;
		round = 1;
		interval = 0;
	    //重置分数
		scoreBoard.Reset ();
		state = GameState.Playing;
		for (int i = 0; i < 10; i++) {
			disks.Push (factory.getDisk(round));
		}
	}

    //点击发生的后台反应
	public void hit(Vector3 pos)
	{  
		Ray ray = Camera.main.ScreenPointToRay (pos);  

		RaycastHit[] hits;  
		hits = Physics.RaycastAll (ray);  
		for (int i = 0; i < hits.Length; i++) {  
			RaycastHit hit = hits [i];  
                //当点击到飞镖时
			if (hit.collider.gameObject.GetComponent<DiskData> () != null) {  
			    /计分
				scoreBoard.Record (hit.collider.gameObject);
			    //飞镖数-1
				diskNum--;
			    //使点击到的飞镖失活
				hit.collider.gameObject.SetActive (false);
			    //储存点击到的飞镖
				factory.freeDisk (hit.collider.gameObject);
			}  
		}  
	}
    ```

- **ScoreController.cs（记分板类）**

    ``` C#
    using UnityEngine;
    using System.Collections;
    
    public class ScoreController : MonoBehaviour
    {
        //分数
    	public float score = 0;
    
        //计分
    	public void Record(GameObject disk){
    		Color color = disk.transform.GetComponent<DiskData> ().color;
    		float yDir = disk.transform.GetComponent<DiskData> ().getDir ().y;
    		float tmp;
    	    //角度越偏离45tmp越大
    		if (yDir > 0.5) {
    			tmp = (yDir - 0.5f) * 1.5f;
    		} else if (yDir < 0.3) {
    			tmp = (0.3f - yDir) * 1.5f;
    		} else {
    			tmp = Mathf.Abs (0.4f - yDir);
    		}
    		
    	    //基本分
    		if (color == Color.green) {
    			score += 1;
    		} else if (color == Color.blue) {
    			score += 2;
    		}else if (color == Color.yellow) {
    			score += 2;
    		}else if (color == Color.red) {
    			score += 2;
    		}
    	    //算分
    		score += tmp * 2;
    	}
        
        //重置
    	public void Reset(){
    		score = 0;
    	}
    	
    	void Start ()
    	{
    	
    	}
    	
    	void Update ()
    	{
    	
    	}
    }
    ```
    
- **SSActionManager.cs（动作管理基类）**

    ``` C#
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    //创建 MonoBehaiviour 管理一个动作 集合，动作做完自动回收动作
    public class SSActionManager : MonoBehaviour {
    
    	private Dictionary<int, SSAction> actions = new Dictionary<int, SSAction> ();
    	private List<SSAction> waitingAdd = new List<SSAction> ();
    	private List<int> waitingDelete = new List<int> ();
    	
    	void Start () {
    
    	}
    
    	protected void Update () {
    	    //从waitingAdd中拿出动作
    		foreach (SSAction ac in waitingAdd) {
    			actions [ac.GetInstanceID ()] = ac;
    		}
    		waitingAdd.Clear ();
    		
    	    //持续执行动作知道其destroy为true，并放入waitingDelete，删除
    		foreach (KeyValuePair<int, SSAction> kv in actions) {
    			SSAction ac = kv.Value;
    			if (ac.destory) {
    				waitingDelete.Add (ac.GetInstanceID ());
    			} else if (ac.enable) {
    				ac.Update ();
    			}
    		}
    
    		foreach (int key in waitingDelete) {
    			SSAction ac = actions [key];
    			actions.Remove (key);
    			DestroyObject (ac);
    		}
    		waitingDelete.Clear ();
    	}
    
        //运行一个新动作的方法。该方 法把游戏对象与动作绑定，并绑定该动 作事件的消息接收者
    	public void RunAction(GameObject gameobject, SSAction action, ISSActionCallback manager){
    		action.gameobject = gameobject;
    		action.transform = gameobject.transform;
    		action.callback = manager;
    		waitingAdd.Add (action);
    	    //执行改动作的 Start 方法
    		action.Start ();
    	}
    }
    ```
    
- **CCActionManager（飞行动作管理类）**

    变量成员
    ``` c#
    //Director单例对象
    Director director; 
    //飞行动作
	public CCfly fly;
    ```
    Start函数为空
    
    Update函数
    ``` C#
    protected new void Update ()
	{
	    //得到Director
		director = Director.getInstance ();
	    //给场景控制器中的动作管理对象赋值自己
		if (director.scence != null) {
			director.scence.manager = this;
		}
		base.Update ();
	}
    ```
    throwDisk函数
    ``` C#
    public void throwDisk(GameObject disk){
        给disk绑上fly
		fly = CCfly.GetSSAction ();
		RunAction (disk, fly, this);
	}
    ```
    SSActionEvent函数
    ``` c#
    public void SSActionEvent(SSAction source,  
		SSActionEventType events = SSActionEventType.Competeted,  
		int intParam = 0,  
		string strParam = null,  
		UnityEngine.Object objectParam = null)
	{  
		if (source is CCfly) {
		    //disk数量-1
			director.scence.subDisknum ();
		    //将完成飞行动作（没有被击中）的飞镖储存
			director.scence.factory.freeDisk (source.gameobject);
		}
	}
    ```

- **SSAction.cs（动作基类）**

    ``` C#
    using UnityEngine;
    using System.Collections;

    public class SSAction : ScriptableObject {
    
    	public bool enable = true;
    	public bool destory = false;
    	
    	public GameObject gameobject{ get; set;}
    	public Transform transform{ get; set;}
    	public ISSActionCallback callback{ get; set;}
    
    	protected SSAction(){}
    	// Use this for initialization
    	public virtual void Start () {
    		//throw new System.NotImplementedException ();
    	}
    
    	// Update is called once per frame
    	public virtual void Update () {
    		//throw new System.NotImplementedException ();
    	}
    }

    ```
    
- **CCfly.cs（飞行动作）**

    ``` C#
    using UnityEngine;
    using System.Collections;
    
    public class CCfly : SSAction
    {
        //飞行时间
    	private float time;
        //飞行方向
    	private Vector3 direction;
        //飞行速度
    	private float speed;
    
        //返回动作
    	public static CCfly GetSSAction(){
    		CCfly action = ScriptableObject.CreateInstance<CCfly> ();
    		return action;
    	}
    	
    	public override void Start ()
    	{
    		time = 0;
    	    //方向与速度为当前飞镖的DiskDate中的数据
    		direction = gameobject.GetComponent<DiskData>().getDir();
    		speed = gameobject.gameObject.GetComponent<DiskData> ().speed;
    		gameobject.SetActive (true);
    	}
    
    	
    	public override void Update ()
    	{
    	    //当非标还处于激活状态时进行飞行
    		if (gameobject.activeSelf) {
    		    //以下为物理公式算法
    			time += Time.deltaTime;
    			transform.Translate (Vector3.down * 2f * time * Time.deltaTime);  
    			transform.Translate (direction * speed * Time.deltaTime);
    		    //当飞出屏幕动作结束
    			if (transform.position.x > 50 || transform.position.y > 20 || transform.position.y < -15) {
    				this.destory = true;
    				gameobject.SetActive (false);
    				this.callback.SSActionEvent (this);
    			}
    		}
    	}
    }
    ```

- **UIinterface.cs（GUI交互界面类）**

    ``` C#
    using UnityEngine;
    using System.Collections;
    
    public class UIinterface : MonoBehaviour
    {
    	Director director = Director.getInstance();
    	
    	void Start ()
    	{
    	}
    	
    	
    	void Update ()
    	{
    	    //点击动作
    		if (Input.GetButtonDown("Fire1"))  
    		{  
    			Vector3 pos = Input.mousePosition;  
    			director.scence.hit(pos);  
    		}  
    	}
    
    	void OnGUI(){
    	    //开始游戏前的界面
    		if (director.scence.state == GameState.Init) {
    			if (GUI.Button (new Rect ((Screen.width - 50) / 2, (Screen.height - 20) / 2, 100, 40), "Game Start")) {
    				director.scence.gameStart ();
    			}
    		} 
    	    //结束游戏时的界面
    		else if (director.scence.state == GameState.Over) {
    			GUI.Label (new Rect (Screen.width / 2 - 20, Screen.height / 2 - 100, 100, 20), "Your Score: " + director.scence.getScore().ToString("0.0"));
    			if (GUI.Button (new Rect ((Screen.width - 50) / 2, (Screen.height - 20) / 2, 100, 40), "Game Start")) {
    				director.scence.gameStart ();
    			}
    		} 
    	    //游戏中的界面
    		else if (director.scence.state == GameState.Playing) {
    			GUI.Label (new Rect (Screen.width / 2 + 240, Screen.height / 2 - 120, 150, 60), "Round:    " + director.scence.getRound() + "\nScore:     " + director.scence.getScore().ToString("0.0"));
    		}
    	}
    }
    ```
- 代码解析完成
- [视频链接](http://v.youku.com/v_show/id_XMzU0Mjg0MTM3Ng==.html?spm=a2hzp.8244740.0.0)
