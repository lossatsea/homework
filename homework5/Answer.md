### 改进飞碟（Hit UFO）游戏：

#### 1. 游戏内容要求：

- 按adapter模式 设计图修改飞碟游戏
- 使它同时支持物理运动与运动学（变换）运动

#### 2. 代码分析

##### （1）作业要求外的改进：联系hit函数和fly动作的结束

在上次的作业中，hit函数响应用户的点击事件，在hit函数里使飞碟失活，移动飞碟位置，改变飞碟的属性并回收飞碟。但这其实与fly动作结束后的回收工作重复，不如将两者联系起来。

也就是说，fly动作的结束条件不仅是空间坐标超出屏幕范围。而且要加上hit事件的发生。

为此，要首先在DiskData中添加联系两者的桥梁isHited：

位置：**DiskData.cs**
``` C#
//标志是否发生了点击事件，初始默认为false
private bool isHited = false;

//得到isHited
public bool getHit(){
	return isHited;
}

//设置isHited
public void setHit(bool isHit){
	isHited = isHit;
}
```

然后修改hit函数，将以前对飞碟的各种处理简化为修改isHited属性：

位置：**ScenController.cs**
``` c#
//响应点击事件
public void hit(Vector3 pos)
{  
	Ray ray = Camera.main.ScreenPointToRay (pos);  
	RaycastHit[] hits;  
	hits = Physics.RaycastAll (ray);  
	for (int i = 0; i < hits.Length; i++) {  
		RaycastHit hit = hits [i];  
		
		//点击到飞碟
		if (hit.collider.gameObject.GetComponent<DiskData> () != null) {  
		
		    //计分
			scoreBoard.Record (hit.collider.gameObject);
			
			//将isHited属性设置为true
			hit.collider.gameObject.GetComponent<DiskData> ().setHit (true);
		}  
	}  
}
```

最后修改fly的终止条件，加上isHited属性就行了：

位置：**CCfly.cs**
``` C#
public override void Update ()
{
    //当飞碟激活时进行fly
	if (gameobject.activeSelf) {
		time += Time.deltaTime;
		transform.Translate (Vector3.down * 2f * time * Time.deltaTime);  
		transform.Translate (direction * speed * Time.deltaTime);  
		
		//当飞出屏幕或者被点击到的话，fly动作终止
		if (transform.position.x > 50 || transform.position.y > 20 || transform.position.y < -15 
		    || gameobject.GetComponent<DiskData> ().getHit () == true) {
			this.destory = true;
			this.callback.SSActionEvent (this);
		}
	}
}
```

所有回收处理工作都将在ActionManager的SSActionEvent函数里完成。当然不能忘记，回收处理时要把飞碟的isHited属性改为false后再回收。

经过简单的处理可以解决代码的冗余，也可以使飞碟的速度更加稳定。

##### （2）作业要求外的改进：依次生成飞碟

在之前的作业中，每轮游戏的开始都会一下生成10个飞碟在屏幕外待机，之后每隔一段时间就弹出一个飞碟。但在本次作业要求中，有下面的注意点：

- 有点击事件发生的飞碟一定要开启碰撞器
- 同时开启刚体和碰撞器的物体出现在同一个点附近时会发生碰撞并四散开来

因此进入物理模式后直接把10个飞碟生成在屏幕外附近很有可能发生碰撞，产生偏移和初速度，考虑下面几种解决方法：

- 物理模式下，刚产生的飞碟不是刚体，飞出去后才是刚体（当然重力也是飞出去后才起效）
- 物理模式下，刚产生的飞碟没有碰撞器，重力不起效，飞出去后才有碰撞器，重力开始起效
- 不同时产生10个飞碟，而是一个一个产生，前一个飞碟飞出去后1秒左右后一个飞碟才会出现，并且飞碟一出现就会飞出

前两种有个难题：什么时候算飞出去？有的玩家在飞碟刚刚出现一点的时候就会点击并期望得到反应，这个判断需要加入刚体或者碰撞器的时机很难把握（当然可以通过调整产生位置来解决，但那又需要重新设计轨迹，并且这么多性质来回开启关闭非常麻烦），于是就直接选择第三种方法，目的很简单，实现也很简单。

只需要修改ScenController类：

既然是一个一个产生，就舍弃存放飞镖的stack。并增加diskProducted表示本回合已经产生的飞碟数。

``` C#
private int diskProducted = 0;
```

Update函数中stack进行的判断都可以用diskProducted代替：
``` c#
void Update ()
{
	if (manager != null) {
	
	    //当场上产生的飞碟都消失的话进入下一轮
		if (state == GameState.Playing && diskNum == 0) {
			nextRound ();
		}
		
		//当场上还有飞碟
		if (diskNum > 0) {
		
		    //当产生的飞碟不到10个且间隔时间1s以上，产生新飞碟
			if (interval >= 1 && state == GameState.Playing && diskProducted < 10) {
				GameObject target = factory.getDisk (round, mode);
				diskProducted++;
				manager.throwDisk (target);
				interval = 0;
			} else if (interval < 1 && state == GameState.Playing) {
				interval += 1 * Time.deltaTime;
			}
		}
	}
}
```

初始化时，比如进入下一轮或者重新开始一局，都要把diskProducted初始化为0.这样通过简单的改变就实现了逐个产生飞碟，由于是产生即飞出，也不用修改刚体的性质。

##### （3）作业要求的改进：按adapter模式增加物理学模式

只进行新增或修改的代码部分的分析

**1. 新增**

- 新增IActionManager接口和ActionMode的枚举

    - IActionManager接口：被两种动作管理器进行实现，是ScenceController和动作管理器的交互窗口
    
    - ActionMode枚举：有Kinematics, Physis, Null三种状态，表示当前回合的动作模式
    
    位置：**ALLIterface.cs**
    
    ``` C#
    //两个动作管理类都实现了throwDisk方法
    public interface IActionManager {  
    	void throwDisk(GameObject disk);  
    }
    
    public enum ActionMode:int{Kinematics, Physis, Null}
    ```

- 新增 **PhysisActionManager.cs** 动作管理器

    删除SSActionManager类，将其中的函数部分移植到PhysisActionManager中去。
    
    继承与实现：
    ``` C#
    public class PhysisActionManager :MonoBehaviour, IActionManager, ISSActionCallback 
    ```
    
    成员变量：
    ``` C#
    //导演类
    Director director;
    
    //fly动作类
	public CCfly fly;
	
	//原SSActionManager的成员函数
	private Dictionary<int, SSAction> actions = new Dictionary<int, SSAction> ();
	private List<SSAction> waitingAdd = new List<SSAction> ();
	private List<int> waitingDelete = new List<int> ();
    ```
    Start函数：
    ``` c#
    void Start () {
        //得到单例的导演类
		director = Director.getInstance ();
		
		//将自己作为ScenceController的管理类
		if (director.scence != null) {
			director.scence.manager = this;
		}
	}
    ```
    Update函数，注意这里重点是ac执行的是FixedUpdate，而不是Update，之后的代码会将运动学运动放在Update函数里，物理运动放在FixedUpdate函数里：
    ``` C#
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
				ac.FixedUpdate ();      //这里调用FixedUpdate函数
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
    ```
    ThrowDisk函数：
    ``` C#
    public void throwDisk(GameObject disk){
        //添加fly动作
		fly = CCfly.GetSSAction ();
		RunAction (disk, fly, this);
	}

    ```
    SSActionEvent函数，回收飞碟的函数：
    ``` c#
    public void SSActionEvent(SSAction source,  
		SSActionEventType events = SSActionEventType.Competeted,  
		int intParam = 0,  
		string strParam = null,  
		UnityEngine.Object objectParam = null)
	{  
		if (source is CCfly) {
		    //将isHit属性改为false
			source.gameobject.GetComponent<DiskData> ().setHit (false);
			//使飞碟失活
			source.gameobject.SetActive (false);
			//速度变为0
			source.gameobject.gameObject.GetComponent<DiskData> ().speed = 0;
			//通知ScenController有一个飞碟消失
			director.scence.subDisknum ();
			//回收
			director.scence.factory.freeDisk (source.gameobject);
		}
	}
    ```

**2. 修改**

- 修改CCActionManager类

    修改继承与实现：
    ``` C#
    public class CCActionManager : MonoBehaviour, IActionManager, ISSActionCallback
    ```
    和PhysisActionManager只有一点不同，Update函数中，动作ac调用的是Update函数而不是FixedUpdate函数 ：
    ``` C#
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    
    public class CCActionManager : MonoBehaviour, IActionManager, ISSActionCallback
    {
    	//导演类
        Director director;
        
        //fly动作类
        public CCfly fly;
        
        //原SSActionManager的成员函数
        private Dictionary<int, SSAction> actions = new Dictionary<int, SSAction> ();
        private List<SSAction> waitingAdd = new List<SSAction> ();
        private List<int> waitingDelete = new List<int> ();
    	
    	void Start () {
    		//得到单例的导演类
        	director = Director.getInstance ();
        	
        	//将自己作为ScenceController的管理类
        	if (director.scence != null) {
        		director.scence.manager = this;
        	}
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
        			ac.Update ();      //这里调用Update函数
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
    
    	public void throwDisk(GameObject disk){
    	    //添加fly动作
    		fly = CCfly.GetSSAction ();
    		RunAction (disk, fly, this);
    	}
    
        //回收飞碟的函数
    	public void SSActionEvent(SSAction source,  
        	SSActionEventType events = SSActionEventType.Competeted,  
        	int intParam = 0,  
        	string strParam = null,  
        	UnityEngine.Object objectParam = null)
        {  
        	if (source is CCfly) {
        	    //将isHit属性改为false
        		source.gameobject.GetComponent<DiskData> ().setHit (false);
        		//使飞碟失活
        		source.gameobject.SetActive (false);
        		//速度变为0
        		source.gameobject.gameObject.GetComponent<DiskData> ().speed = 0;
        		//通知ScenController有一个飞碟消失
        		director.scence.subDisknum ();
        		//回收
        		director.scence.factory.freeDisk (source.gameobject);
        	}
        }
    }
    ```

- 修改ScenController类

    除了为了实现飞碟的出个出现外，适应新结构也要修改该类
    
    将CCActionManager对象改为IActionanager接口，并增加ActionMode来表示当前游戏的动作模式：
    ``` C#
    public IActionManager manager = null;
	public ActionMode mode = ActionMode.Null;
    ```
    修改Start函数，设置重力加速度，取决于自己想要的效果。
    ``` C#
    void Start ()
	{
		factory = Singleton<DiskFactory>.Instance;
		scoreBoard = Singleton<ScoreController>.Instance;
		director = Director.getInstance ();
		director.scence = this;
		Physics.gravity = new Vector3 (0, -5f, 0);
	}
    ```
    修改nextRound函数，当本局游戏结束时，设置的动作模式要消除掉：
    ``` C#
    private void nextRound(){
        //当4回合结束，本局结束
		if (round == 4) {
		
		    //去除动作管理组件
			if (gameObject.GetComponent<CCActionManager> () != null) {
				Destroy (gameObject.GetComponent<CCActionManager> ());
			} else if(gameObject.GetComponent<PhysisActionManager> () != null) {
				Destroy (gameObject.GetComponent<PhysisActionManager> ());
			}
			
			//动作模式改为NULL
			mode = ActionMode.Null;
			state = GameState.Over;
			return;
		}
		diskNum = 10;
		diskProducted = 0;
		round++;
	}
    ```
    增加CCgameStart函数，表示选择以运动学模式进入游戏
    ``` c#
    public void CCgameStart(){
        //加上运动学动作管理器
		gameObject.AddComponent<CCActionManager> ();
		mode = ActionMode.Kinematics;
		gameStart ();
	}
    ```
    增加PhysisgameStart函数，表示选择以物理模式进入游戏
    ``` C#
    //加上物理动作管理器
    public void PhysisgameStart(){
		gameObject.AddComponent<PhysisActionManager> ();
		mode = ActionMode.Physis;
		gameStart ();
	}
    ```

- 修改DiskFactory类

    修改getDisk函数，增加一个参数mode，表示要以什么运动模式进行游戏，当选择物理模式时，输出的飞碟要加上刚体：
    ``` C#
    public GameObject getDisk(int studio, ActionMode mode){
		GameObject newDisk = null;
		if (free.Count > 0) {
			newDisk = free [0].gameObject;
			float y = Random.Range(-10, 0);
			newDisk.transform.position = new Vector3 (-30, y, 0);
			free.Remove (free[0]);
		} else {
			float y = Random.Range(-10, 0);
			newDisk = Instantiate<GameObject> (diskPrefab, new Vector3(-30, y, 0), Quaternion.identity);
		}
		
		//判断动作模式
		if (mode == ActionMode.Physis) {
			newDisk.AddComponent<Rigidbody> ();
		}
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

- 修改SSAction类

    增加FixedUpdate函数，为空，让实际动作类来实现物理模式的飞行
    ``` C#
    public virtual void FixedUpdate(){
	}
    ```

- 修改CCfly类

    修改Start函数，游戏对象是刚体时（表示是物理模式），为其加上初速度：
    ``` C#
    public override void Start ()
	{
		time = 0;
		direction = gameobject.GetComponent<DiskData>().getDir();
		speed = gameobject.gameObject.GetComponent<DiskData> ().speed;
		gameobject.SetActive (true);
		
		//初速度是DiskData的速度与方向的乘积
		if (gameobject.GetComponent<Rigidbody> () != null) {
			gameobject.GetComponent<Rigidbody> ().velocity = speed * direction;
		}
	}
    ```
    增加FixedUpdate函数，用来实现物理模式的飞行，物理动作管理器中引用的就是这个函数。
    
    但由于游戏对象是刚体且受重力，并且Start中已经赋予了初速度，因此实际什么都不用做，只要设置动作的终止条件就好了：
    ``` C#
    public override void FixedUpdate(){
		if (gameobject.activeSelf)  
		{
			if (transform.position.x > 50 || transform.position.y > 20 || transform.position.y < -15 || gameobject.GetComponent<DiskData> ().getHit () == true) {
				this.destory = true;
				this.callback.SSActionEvent (this);
			}
		}  
	}
    ```

- 修改UIIterface类

    修改OnGUI函数，增加让用户选择动作模式的按钮，并且游戏进行时在右上角显示当前模式：
    ``` C#
    void OnGUI(){
    
        //游戏未开始时选择模式
		if (director.scence.state == GameState.Init) {
		
		    //运动学模式
			if (GUI.Button (new Rect (Screen.width / 2 - 150, (Screen.height - 20) / 2, 140, 40), "kinematics mode")) {
				director.scence.CCgameStart ();
			}
			
			//物理模式
			else if (GUI.Button (new Rect (Screen.width / 2 + 10, (Screen.height - 20) / 2, 140, 40), "physical mode")) {
				director.scence.PhysisgameStart ();
			}
		} 
		
		//游戏结束时显示分数，选择下一局模式
		else if (director.scence.state == GameState.Over) {
		
		    //显示分数
			GUI.Label (new Rect (Screen.width / 2 - 20, Screen.height / 2 - 100, 100, 20), "Your Score: " + director.scence.getScore().ToString("0.0"));
			
			//运动学模式
			if (GUI.Button (new Rect (Screen.width / 2 - 150, (Screen.height - 20) / 2, 140, 40), "kinematics mode")) {
				director.scence.CCgameStart ();
			}
			
			//物理模式
			else if (GUI.Button (new Rect (Screen.width / 2 + 10, (Screen.height - 20) / 2, 140, 40), "physical mode")) {
				director.scence.PhysisgameStart ();
			}
		} 
		
		//游戏进行时显示当前信息
		else if (director.scence.state == GameState.Playing) {
		
		    //回合数，分数，模式
			GUI.Label (new Rect (Screen.width / 2 + 220, Screen.height / 2 - 120, 200, 60), "Round:    " + director.scence.getRound() + "\nScore:     " + director.scence.getScore().ToString("0.0") + "\nMode: " + director.scence.mode.ToString());
		}
	}
    ```
    
- 删除SSActionManager类
- 有改动的代码全部解析完毕

##### （5）[视频演示]()
