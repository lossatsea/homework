### 可折叠公告栏（订阅与发布）

#### 实现效果

点击公告，如果是展示状态则折叠，如果是折叠状态则展示，下面的所有公告相应进行移动，当有公告在移动时点击无响应：

![effect](https://github.com/lossatsea/homework/blob/master/homework8/pictures/effect.gif)

#### 1. UI设置

首先新建一个Canavs命名为Bulletin board，新建子对象image作为背景图片和Scroll View作为公告栏，在Scroll View中新建image作为公告的背景，Viewport的content对象中新建4个buton作为测试公告，button中除了一个title文本外，新建一个内容文本，新建完成后的结构图如下：

![frame](https://github.com/lossatsea/homework/blob/master/homework8/pictures/frame.png)

Scroll View的大小和content的高度根据需要自行设置，我的是Scroll View为400 * 400, content高500.

下面设置button的位置，所有button发锚点都对准content的最上边的中点，根据需要设置内容文本的高度和button相应的位置，我的第一个公告的设置如下：

![button](https://github.com/lossatsea/homework/blob/master/homework8/pictures/button.png)

![text](https://github.com/lossatsea/homework/blob/master/homework8/pictures/text.png)

剩余的button同理，调整一下text和button的颜色后最终效果如下：

![final](https://github.com/lossatsea/homework/blob/master/homework8/pictures/final2.png)

#### 2. 新建代码

新建代码文件：

- Blletin.cs：响应代码，挂载到所有button上
- GameEvent.cs：事件源，用于订阅与发布模式
- Singleton.cs：单例模式

单例模式，这个不用多说，这里就是用来单例事件源的。代码:

```C#
public class Singleton<T> : MonoBehaviour where T: MonoBehaviour{

	protected static T instance;

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

#### 3. 公告的5个运动状态

在GameEvent.cs文件中加入集合：

```C#
public enum State:int {uping, downing, text_hidding, text_appearing, none}
```

- uping：因为按下某个显示的公告而处于向上滑动的状态
- downing：因为按下某个隐藏的公告而处于向下滑动的状态
- text_hidding：当前公告为显示,按下当前公告使文本处于渐隐状态
- text_appearing：当前公告为隐藏，按下当前公告使文本处于渐显状态
- none：没有任何运动

在Blletin类中有私有变量isHidden表示是否处于显示状态，变量state表示当前公告的运动状态：

```C#
private bool isHidden = false;
private State state = State.none;
```

#### 4. 订阅与发布

事件源：

```C#
public class GameEvent : MonoBehaviour {
    
    //向上滑动事件，num为发布者的编号
	public delegate void UpEvent(char num);
	public static event UpEvent upEvent;

    //向下滑动事件，num为发布者的编号
	public delegate void DownEvent (char num);
	public static event DownEvent downEvent;

	public void up(char num){
		if (upEvent != null) {
			upEvent (num);
		}
	}

	public void down(char num){
		if (downEvent != null) {
			downEvent (num);
		}
	}
}
```

发布与订阅都在Blletin类中进行，具体来说，发布者是被点击的Blletin，而订阅者是所有Blletin，实际响应的是在发布者下面的Blletin。

首先初始化：

```C#
private Button btn;     //button对象
private Text content;   //text对象
private char number;    //当前公告的编号
private float y;        //当前公告的y轴位置

void Start () {
	btn = this.GetComponent<Button> ();
	
	//点击事件
	btn.onClick.AddListener (ifClicked);
	content = this.transform.GetChild (1).GetComponent<Text> ();
	
	//公告的位置，只需要有y轴量即可，x与z不需要变化
	y = btn.transform.localPosition.y;
	
	//公告编号为名字的最后一个字符
	number = btn.name [8];
}

//订阅上升和下降事件
void OnEnable(){
	GameEvent.upEvent += Hide;
	GameEvent.downEvent += Appear;
}

//取消订阅
void OnDisable(){
	GameEvent.upEvent -= Hide;
	GameEvent.downEvent -= Appear;
}
```

订阅事件的处理：

```C#
private Vector3 target; //将要移动的目标位置

//隐藏（上升）事件的处理
void Hide(char changeNum){
    
    //只有当编号大于发布者时才会响应，也就是在点击的公告的下面的公告
	if (number > changeNum) {
		target = new Vector3 (btn.transform.localPosition.x, y + 100, btn.transform.localPosition.z);
		state = State.uping;
	}
}

//显示（下降）事件的处理
void Appear(char changeNum)

    //只有当编号大于发布者时才会响应，也就是在点击的公告的下面的公告
	if (number > changeNum) {
		target = new Vector3 (btn.transform.localPosition.x, y - 100, btn.transform.localPosition.z);
		state = State.downing;
	}
}
```

点击事件的响应：

```C#
void ifClicked(){

    //被点击的公告需要隐藏或显示文本，然后发布事件
	if (isHidden) {
		state = State.text_appearing;
		Singleton<GameEvent>.Instance.down (number);
	} else {
		state = State.text_hidding;
		Singleton<GameEvent>.Instance.up (number);
	}
}
```

#### 5. 公告的运动

首先是文本的变化，我用了渐隐和渐显：

```C#
private float duration = 2.5f;  //变化时间
private float time = 0;         //计时器

void TextChange(){
    //隐藏文本
	if (state == State.text_hidding) {
		time += 0.1f;
		if (time > duration) {
			isHidden = true;
			time = 0;
			state = State.none;
		} else {
		    //改变文本的颜色的透明度
			Color newColor = content.color;
			float proportion = time / duration;
			newColor.a = Mathf.Lerp (1, 0, proportion);
			content.color = newColor;
		}
	}
	//显示文本
	else if(state == State.text_appearing){
		time += 0.1f;
		if (time > duration) {
			isHidden = false;
			time = 0;
			state = State.none;
		} else {
		    //改变文本的颜色的透明度
			Color newColor = content.color;
			float proportion = time / duration;
			newColor.a = Mathf.Lerp (0, 1, proportion);
			content.color = newColor;
		}
	}
}
```

然后是公告位置的变化，就是简单的平移：

```C#
void PositionChange(){
    //上移
	if (state == State.uping) {
		btn.transform.localPosition = Vector3.MoveTowards (btn.transform.localPosition, target, 100f * Time.deltaTime);
		if (btn.transform.localPosition == target) {
			state = State.none;
			y = btn.transform.localPosition.y;
		}
	}
	//下移
	else if (state == State.downing) {
		btn.transform.localPosition = Vector3.MoveTowards (btn.transform.localPosition, target, 100f * Time.deltaTime);
		if (btn.transform.localPosition == target) {
			state = State.none;
			y = btn.transform.localPosition.y;
		}
	}
}
```

每个运动完成后都要将运动状态改为none，之后将两个运动放入Update函数就可以了：

```C#
void Update () {
	if (state != State.none) {
		TextChange ();
		PositionChange ();
	}
}
```

#### 6. 互斥锁

其实基本效果已经实现了，只不过有个不友好：当有公告在移动时，点击公告扔回响应，这就造成混乱，我们要保证有公告运动的时候，不会有点击响应，当运动结束时，回复点击响应。这就需要一个互斥锁，听起来很高大上，其实就是一个公共变量，由于事件源史丹利的，我们可以把它挡在事件源GameEvent类里：

```C#
public bool isChange{ get; set;}
```

在Bulletin里初始化时进行赋值false，然后每次点击事件都要判断一次，发生平移事件时将其置为true，平移事件结束后置为false（这里所有公告的平移事件都是同时开始和结束的，不用担心先后问题）。

在Start()加上最后一句:

```C#
Singleton<GameEvent>.Instance.isChange = false;
```

ifClicked()要先进行判断：

```C#
void ifClicked(){
	if (!Singleton<GameEvent>.Instance.isChange) {
		//todo
	}
}
```

Hide()和Appear()在确定运动后先置数:

```C#
void Hide(char changeNum){
	if (number > changeNum) {
		Singleton<GameEvent>.Instance.isChange = true;
		//···
	}
}

void Appear(char changeNum){
	if (number > changeNum) {
		Singleton<GameEvent>.Instance.isChange = true;
		//···
	}
}
```

PositionChange()在平移结束后更改isChange：

```C#
void PositionChange(){
	if (state == State.uping) {
		//move
		if (btn.transform.localPosition == target) {
			//todo
			Singleton<GameEvent>.Instance.isChange = false;
		}
	}
	else if (state == State.downing) {
		//move
		if (btn.transform.localPosition == target) {
			//todo
			Singleton<GameEvent>.Instance.isChange = false;
		}
	}
}
```

#### Bulletin完整代码

```C#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class Bulletin : MonoBehaviour {

	private Button btn;
	private Text content;

	private bool isHidden = false;
	private State state = State.none;
	private Vector3 target;
	private float y;
	private float duration = 2.5f;
	private float time = 0;
	private char number;
	// Use this for initialization
	void Start () {
		btn = this.GetComponent<Button> ();
		btn.onClick.AddListener (ifClicked);
		content = this.transform.GetChild (1).GetComponent<Text> ();
		y = btn.transform.localPosition.y;
		number = btn.name [8];
		Singleton<GameEvent>.Instance.isChange = false;
	}

	void OnEnable(){
		GameEvent.upEvent += Hide;
		GameEvent.downEvent += Appear;
	}

	void OnDisable(){
		GameEvent.upEvent -= Hide;
		GameEvent.downEvent -= Appear;
	}
	
	// Update is called once per frame
	void Update () {
		if (state != State.none) {
			TextChange ();
			PositionChange ();
		}
	}

	void Hide(char changeNum){
		if (number > changeNum) {
			Singleton<GameEvent>.Instance.isChange = true;
			target = new Vector3 (btn.transform.localPosition.x, y + 100, btn.transform.localPosition.z);
			state = State.uping;
		}
	}

	void Appear(char changeNum){
		if (number > changeNum) {
			Singleton<GameEvent>.Instance.isChange = true;
			target = new Vector3 (btn.transform.localPosition.x, y - 100, btn.transform.localPosition.z);
			state = State.downing;
		}
	}

	void ifClicked(){
		if (!Singleton<GameEvent>.Instance.isChange) {
			if (isHidden) {
				state = State.text_appearing;
				Singleton<GameEvent>.Instance.down (number);
			} else {
				state = State.text_hidding;
				Singleton<GameEvent>.Instance.up (number);
			}
		}
	}

	void TextChange(){
		if (state == State.text_hidding) {
			time += 0.1f;
			if (time > duration) {
				isHidden = true;
				time = 0;
				state = State.none;
			} else {
				Color newColor = content.color;
				float proportion = time / duration;
				newColor.a = Mathf.Lerp (1, 0, proportion);
				content.color = newColor;
			}
		}
		else if(state == State.text_appearing){
			time += 0.1f;
			if (time > duration) {
				isHidden = false;
				time = 0;
				state = State.none;
			} else {
				Color newColor = content.color;
				float proportion = time / duration;
				newColor.a = Mathf.Lerp (0, 1, proportion);
				content.color = newColor;
			}
		}
	}

	void PositionChange(){
		if (state == State.uping) {
			btn.transform.localPosition = Vector3.MoveTowards (btn.transform.localPosition, target, 100f * Time.deltaTime);
			if (btn.transform.localPosition == target) {
				state = State.none;
				y = btn.transform.localPosition.y;
				Singleton<GameEvent>.Instance.isChange = false;
			}
		}
		else if (state == State.downing) {
			btn.transform.localPosition = Vector3.MoveTowards (btn.transform.localPosition, target, 100f * Time.deltaTime);
			if (btn.transform.localPosition == target) {
				state = State.none;
				y = btn.transform.localPosition.y;
				Singleton<GameEvent>.Instance.isChange = false;
			}
		}
	}
}

```
