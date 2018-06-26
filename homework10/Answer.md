### 联网双人赛车游戏

#### 游戏简单介绍

- 玩家操作自己的车完成环湖一圈，先到达出发点的用户胜利

#### 开发流程

- **场景预制**

在Assert Store中搜索“Lake Race Track”进行下载

![picture](https://github.com/lossatsea/homework/blob/master/homework10/pictures/store.png)

用已有的预制和模型制作自己的场景，下面是我的场景：

![picture](https://github.com/lossatsea/homework/blob/master/homework10/pictures/scene.png)

![pic](https://github.com/lossatsea/homework/blob/master/homework10/pictures/scene3.png)

![pic](https://github.com/lossatsea/homework/blob/master/homework10/pictures/scene2.png)

- **NetworkManager**

设置网络控制器（Network Manager）和 用户控制界面（Network Manager HUD，供玩家找到并加入游戏）

![pic](https://github.com/lossatsea/homework/blob/master/homework10/pictures/manager.png)

- **胜利条件**

当然，绕湖一周回到起点就是胜利条件，因此我们需要一个goal触发器放在终点来检测玩家的经过，对应玩家的isWin属性。

但是如果玩家掉头经过goal怎么办？我们还需要一个空游戏对象：gate，只有先经过gate再经过goal才会判定为胜利，对应玩家的winFlag属性。

因此gate和goal都在公路上，且goal在终点处，gate在goal的前面（先经过）调整位置和角度：

![pic](https://github.com/lossatsea/homework/blob/master/homework10/pictures/gate_goal.png)

> GateTrigger: gate的触发事件

```C#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GateTrigger : NetworkBehaviour {

	void OnTriggerEnter(Collider collider)
	{
		if (collider.tag == "Player") {
			collider.GetComponent<PlayerMove> ().winFlag = true;
		}
	}

    //...
}
```

设置为网络对象：

![pic](https://github.com/lossatsea/homework/blob/master/homework10/pictures/gate.png)

> GoalTrigger：goal的触发事件

```C#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GoalTrigger : MonoBehaviour {

	void OnTriggerEnter(Collider collider)
	{
	//只有当winFlag为true时，isWin才会是true
		if (collider.tag == "Player" && collider.GetComponent<PlayerMove> ().winFlag) {
			collider.GetComponent<PlayerMove> ().isWin = true;
		}
	}
    
    //...
}

```

设置为网络对象：

![pic](https://github.com/lossatsea/homework/blob/master/homework10/pictures/goal.png)

加入到网络管理器中：

![pic](https://github.com/lossatsea/homework/blob/master/homework10/pictures/gate_goal2.png)

- **Director和GUI**

游戏有三个状态：游戏中，host胜，client胜

那么用什么来记录当前游戏的状态呢，我在这里用一个空对象Director来表示唯一的用来记录游戏状态的对象。

Director上挂载着两个脚本：Director和UserGUI

> Director：记录当前游戏状态

```C#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
//游戏状态
public enum State:int{HostWin, ClientWin, Playing}

public class Director : NetworkBehaviour {
    
    //服务器权限，只有服务器才能进行修改
	[SyncVar]
	public State state = State.Playing;
    
    //记录当前连接的数量
	public int linkNum = 0;
    
    //游戏结束时调用，修改状态，1表示host胜，2表示client胜
	public void gameOver(int flag){
		if (!isServer)
			return;
		if (flag == 1)
			state = State.HostWin;
		else if (flag == 2)
			state = State.ClientWin;
	}
	
	//...
}

```

> UserGUI：根据Director中的游戏状态输出结果

```C#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserGUI : MonoBehaviour {

	Director director;     //Director
	GUIStyle HostStyle, ClientStyle;//两种字体
	
	void Start () {
		director = GetComponent<Director> ();

		HostStyle = new GUIStyle ();
		HostStyle.alignment = TextAnchor.MiddleCenter;
		HostStyle.normal.textColor = Color.red;
		HostStyle.fontSize = 30;

		ClientStyle = new GUIStyle ();
		ClientStyle.alignment = TextAnchor.MiddleCenter;
		ClientStyle.normal.textColor = Color.blue;
		ClientStyle.fontSize = 30;
	}
	
	void Update () {
		
	}
    
    //根据游戏状态输出结果
	void OnGUI(){
		if (director.state == State.HostWin) {
			GUI.Label (new Rect (Screen.width / 2 - 100, Screen.height / 2 - 20, 200, 40), "Host Win", HostStyle);
		} else if (director.state == State.ClientWin){
			GUI.Label (new Rect(Screen.width / 2 - 100, Screen.height / 2 - 20, 200, 40), "Client Win", ClientStyle);
		}
	}
}

```

游戏对象都通过读取Diector中的state知道当前的游戏状态，至于gameOver的参数flag怎么得到，往下看就知道了。

![pic](https://github.com/lossatsea/homework/blob/master/homework10/pictures/Director.png)

- **两个起始位置**

这就比较简单了，就是玩家1和玩家2的起始地点。

1. 新建了两个空对象pos1和pos2，分别移动到自己想要的玩家的起始地点
2. 给pos1和pos2都加上“NetworkStartPosition”这个组件
3. 将NetworkManager的SpawnInfo中的PlayerSpawnMet变成Round Robin模式

之后游戏开始时，玩家对象就会一次在两个起始地点出现。

到目前为止这个游戏的Hierarchy界面应该有以下的游戏对象：

![pic](https://github.com/lossatsea/homework/blob/master/homework10/pictures/h.png)

- **玩家角色**

车的预制用unity在自带的Vehicles的基础上进行修改

先换一下外观，看起来更帅气一点，注意更改的是SkyCarBody的Materia：

![pic](https://github.com/lossatsea/homework/blob/master/homework10/pictures/carBody.png)

![pic](https://github.com/lossatsea/homework/blob/master/homework10/pictures/carBody2.png)

之后在车的最外面的对象上加上碰撞器，好让gate和goal检测到的碰撞体的tag是player：

![pic](https://github.com/lossatsea/homework/blob/master/homework10/pictures/collider2.png)

更改后：

![pic](https://github.com/lossatsea/homework/blob/master/homework10/pictures/collider.png)

为了让摄像机一直跟着玩家对象，可以在car里加一个空游戏对象，然后让摄像机的位置和它保持一致：

![pic](https://github.com/lossatsea/homework/blob/master/homework10/pictures/look.png)
![pic](https://github.com/lossatsea/homework/blob/master/homework10/pictures/look2.png)

现在的车的操作不是联网的，因此我们要稍微修改一下，打开CarUserControl脚本

1. 将类变成网络类
2. 响应键盘前判断是否为本地游戏对象

```C#
using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.Networking;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof (CarController))]
    public class CarUserControl : NetworkBehaviour
    {
        private CarController m_Car; // the car controller we want to use


        private void Awake()
        {
            // get the car controller
            m_Car = GetComponent<CarController>();
        }


        private void FixedUpdate()
        {
			if (!isLocalPlayer)
				return;
            // pass the input to the car!
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            float v = CrossPlatformInputManager.GetAxis("Vertical");
#if !MOBILE_INPUT
            float handbrake = CrossPlatformInputManager.GetAxis("Jump");
            m_Car.Move(h, v, v, handbrake);
#else
            m_Car.Move(h, v, v, 0f);
#endif
        }
    }
}

```

再加上NetworkIdentity和NetworkTransform

![pic](https://github.com/lossatsea/homework/blob/master/homework10/pictures/player1.png)

之后就编写自己的脚本了，主要要实现的有：

- 玩家对象要在游戏结束时停下来，切开键盘响应（或响应几乎没有效果）
- 玩家对象在胜利的情况下（isWin为true），要申请修改服务器数据state
- 玩家对象从道路上掉下去时要回到初始地点重新出发
- 摄像机跟随

> PlayerMove

```C#
	public GameObject look; //摄像机放置的对象
	public GameObject gatePrefab;   //gate预制
	public GameObject goalPrefab;   //goal预制
	public GameObject PlayerPrefab; //SkyCar预制，用来重置游戏对象rotation
	public Vector3 pos1, pos2;  //pos1和pos2的位置，用来重置游戏对象位置
	Director director;  // Director
	public bool winFlag = false;
	public bool isWin = false;
	int flag;   //标记是host还是client

```

![pic](https://github.com/lossatsea/homework/blob/master/homework10/pictures/player.png)

在Start的时候初始化唯一Director和flag，这里要知道host一定比client游戏开始的早，因此一定是host先start，host时LinkNum一定为0，加一后，client时LinkNum一定为1，就这样区分了host和client

```c#
void Start () {
	director = GameObject.Find("Director").GetComponent<Director> ();
	director.linkNum++;
	flag = director.linkNum;    //host为1，client为2
	
	if (GameObject.Find("gate(Clone)") == null)
		CmdStart ();
}
```

Command指令来生成gate和goal

```C#
[Command]
void CmdStart(){
	var gate = Instantiate<GameObject> (gatePrefab, gatePrefab.transform.position, gatePrefab.transform.rotation);
	var goal = Instantiate<GameObject> (goalPrefab, goalPrefab.transform.position, goalPrefab.transform.rotation);
	NetworkServer.Spawn (gate);
	NetworkServer.Spawn (goal);
}
```

Update时先判断是否为本地对象，再实现摄像机跟随：

```C#
void Update () {
	if (!isLocalPlayer)
		return;
	Camera.main.transform.position = new Vector3 (look.transform.position.x, look.transform.position.y, look.transform.position.z);
	Camera.main.transform.forward = transform.forward + new Vector3(0, -0.2f, 0);
}
```

效果：

![gif](https://github.com/lossatsea/homework/blob/master/homework10/pictures/canera.gif)

判断输赢时要放在判断本地对象前面，因为这个判断结果要在服务器和客户端都有反应（但修改游戏状态的只有服务器）：

```c#
void Update () {
	if (isWin) {
		director.gameOver (flag);
	}
	if (!isLocalPlayer)
		return;
	Camera.main.transform.position = new Vector3 (look.transform.position.x, look.transform.position.y, look.transform.position.z);
	Camera.main.transform.forward = transform.forward + new Vector3(0, -0.2f, 0);
}
```

如果玩家掉到了赛道外面，我希望让它重回初始位置，这需要Rpc指令：

```C#
[ClientRpc]
void RpcReset(){
	if (!isLocalPlayer)
		return;
	transform.localRotation = PlayerPrefab.transform.rotation;
	GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezeAll;
	GetComponent<Rigidbody> ().velocity = Vector3.zero;
	
	//host回到pos1，client回到pos2
	if (!isServer)
		transform.position = pos2;
	else
		transform.position = pos1;
	GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.None;
}
```
把判断加到Update里：

```C#
void Update () {
	if (isWin) {
		director.gameOver (flag);
	}
	if (transform.position.y < 22.5) {
		if (!isServer)
			return;
		RpcReset ();
	}
	if (!isLocalPlayer)
		return;
	Camera.main.transform.position = new Vector3 (look.transform.position.x, look.transform.position.y, look.transform.position.z);
	Camera.main.transform.forward = transform.forward + new Vector3(0, -0.2f, 0);
}
```

效果：

![gif](https://github.com/lossatsea/homework/blob/master/homework10/pictures/restart.gif)

当游戏结束时，游戏对象的速度变成0，刚体休眠：

```C#
void Update () {
	if (isWin) {
		director.gameOver (flag);
	}
	if (transform.position.y < 22.5) {
		if (!isServer)
			return;
		RpcReset ();
	}
	if (!isLocalPlayer)
		return;
	Camera.main.transform.position = new Vector3 (look.transform.position.x, look.transform.position.y, look.transform.position.z);
	Camera.main.transform.forward = transform.forward + new Vector3(0, -0.2f, 0);
	if (director.state != State.Playing) {
		GetComponent<Rigidbody> ().velocity = Vector3.zero;
		GetComponent<Rigidbody> ().Sleep ();
	}
}
```

#### [视频链接]()
