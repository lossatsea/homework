### 粒子光环

前半部分参考师兄博客(https://blog.csdn.net/simba_scorpio/article/details/51251126)

目标效果：
- 两层粒子光环，旋转方向相反
- 粒子大小随机
- 粒子运动在一定半径附近随机运动，运动速度有明显差异
- 点击后内层光环扩张，外层光环收缩，再点击恢复

#### 1. 新建对象

新建空对象myParticle，给myParticle新建两个空对象outer和inter，分别表示内层光环和外层光环。

![结构图](https://github.com/lossatsea/homework/blob/master/homework7/pictures/0.png);

给outer和inter添加Particle System的组件，不用更改什么属性。下面主要针对outer，即外层光环进行说明。

#### 2. 新建代码

新建particle类，用来实现粒子效果，新建ParticalInfo类用来储存单个粒子的信息。

```C#
public class particle : MonoBehaviour{
```

```C#
public class ParticalInfo{
	public float radius;    //粒子旋转的半径
	public float angle;     //粒子旋转的角度
	public float time;      //用于计算ingPong函数的时间
	public float size;      //粒子大小

	public ParticalInfo(float r, float a, float t, float s){
		radius = r;
		angle = a;
		time = t;
		size = s;
	}
}
```

particle类的成员变量有两种，公有成员可以在界面上进行调控，私有成员进行内部的逻辑运算。

```C#
public int number = 30000;          //粒子数量
public float MaxSize = 0.1f;        //粒子最大半径
public float MinSize = 0.03f;       //粒子最小半径
public float MaxRadius = 12f;       //扩展时的最大运动半径
public float MinRadius = 6f;        //扩展时的最小运动半径
public float collect_MaxRadius = 4f;//收缩时的最大运动半径
public float collect_MinRadius = 1f;//收缩时的最小运动半径
public bool clockWise = true;       //是否顺时针运动
public float speed = 2f;            //运动速度
public float pingPong = 0.02f;      //浮游偏移量
```

```C#
private ParticleSystem particleSys;             //粒子系统
private ParticleSystem.Particle[] particleArr;  //粒子系统中的粒子数组
private ParticalInfo[] particles;               //粒子的信息数组
```

#### 3. 初始化

Start函数进行初始化，主要就是给粒子系统进行赋值，就相当于在界面里调整各种属性

```C#
//私有变量的初始化
particleArr = new ParticleSystem.Particle[number];
particles = new ParticalInfo[number];
particleSys = GetComponent<ParticleSystem> ();

//粒子系统属性的初始化
particleSys.startSpeed = 0;
particleSys.loop = false;
particleSys.maxParticles = number;
particleSys.Emit (number);
particleSys.GetParticles (particleArr);

//随机粒子的位置，大小，半径
randomLocationAndSize ();
```

randomLocationAndSize函数用来随机粒子的运动和属性，outer的粒子初始时为扩展状态，顺时针运动。

```C#
void randomLocationAndSize(){
	for (int i = 0; i < number; i++) {
	    //随机运动半径
		float MidRadius = (MaxRadius + MinRadius) / 2;
		float outRate = Random.Range (1f, MidRadius / MinRadius);
		float inRate = Random.Range (MaxRadius / MidRadius, 1f);
		float _radius = Random.Range (MinRadius * outRate, MaxRadius * inRate);
        
        //随机大小和初始位置（即角度）
		float size = Random.Range (MinSize, MaxSize);
		float angleDgree = Random.Range (0, 360f);
		float angle = (angleDgree * Mathf.PI) / 180;

		float time = Random.Range (0, 360f);
		
		//将随机后的粒子信息存储
		particles [i] = new ParticalInfo (_radius, angleDgree, time, size);
		particleArr [i].position = new Vector3 (particles[i].radius * Mathf.Cos(angle), particles[i].radius * Mathf.Sin(angle), 0);
		particleArr [i].startSize = particles [i].size;
	}

    //将存储的粒子信息赋给粒子系统
	particleSys.SetParticles (particleArr, particleArr.Length);
}
```

这样就随机初始化好了。将particle代码文件挂载到outer上，运行为下面的效果：

![静态](https://github.com/lossatsea/homework/blob/master/homework7/pictures/1.png);

#### 4. 动起来

nam怎么动起来呢，运动一般是写在update函数中实现。只要在update中让每个粒子的角度发生改变就可以让它动起来：

```C#
void Update ()  
{  
    for (int i = 0; i < number; i++)  
    {  
        particles [i].angle -= 0.1f;    // 顺时针旋转   
  
        particles [i].angle = (360 + particles [i].angle) % 360f;// 保证angle在0~360度  
		float angle = (particles[i].angle * Mathf.PI) / 180;
  
        particleArr [i].position = new Vector3 (particles [i].radius * Mathf.Cos (angle), particles [i].radius * Mathf.Sin (angle), 0);  
    }  
  
    particleSys.SetParticles(particleArr, particleArr.Length);  
}  
```

但是由于每个粒子改变的角度都相同(0.1f)，整个看起来就像一张图片在旋转，因此要让粒子的转速有差异，引入一个分层变量：

```C#
private int tier = 10;//将粒子分为10层
```

将顺时针旋转的那句改为：

```C#
particles [i].angle -= (i % tier + 1) * (speed / particles[i].radius / tier)；// 顺时针旋转   
  
```

后边的变量使得运动半径越大的粒子运动叫速度越慢，此时的效果为：

![gif2](https://github.com/lossatsea/homework/blob/master/homework7/pictures/2.gif);

#### 5. 浮游

此时的粒子都是围绕着一定的半径进行运动，下面要让其在这个半径的附近进行“浮游”。主要是运用PingPong算法：

Update函数中在确定particleArr [i]的位置之前，对运动半径进行处理：

```C#
particles [i].time += Time.deltaTime;
particles [i].radius += Mathf.PingPong (particles [i].time / collect_MinRadius / collect_MaxRadius, pingPong) - pingPong / 2.0f;
```

浮游效果：

![gif3](https://github.com/lossatsea/homework/blob/master/homework7/pictures/3.gif);

#### 6. 透明度和颜色

在粒子系统的属性中有一个颜色条，可以控制透明度和颜色，在代码里也可以用Gradient类进行相同的操作。

加入新的公有成员：

```C#
public Gradient gradient;
```
Start函数中进行初始化，这里只进行和黑白两色的初始化；

```C#
//透明度设置
GradientAlphaKey[] alphaKey = new GradientAlphaKey[5];
alphaKey [0].time = 0;		alphaKey [0].alpha = 1f;
alphaKey [1].time = 0.4f;	alphaKey [1].alpha = 0.4f;
alphaKey [2].time = 0.6f;	alphaKey [2].alpha = 1f;
alphaKey [3].time = 0.9f;	alphaKey [3].alpha = 0.4f;
alphaKey [4].time = 1f;		alphaKey [4].alpha = 0.9f;

//颜色设置
GradientColorKey[] colorKey = new GradientColorKey[2];
colorKey [0].time = 0;		colorKey [0].color = Color.white;
colorKey [1].time = 1f;		colorKey [1].color = Color.white;

//加入设置
gradient.SetKeys (colorKey, alphaKey);
```

在Update中可以进行透明度的设置，这里我就让每个粒子都获得一个随机的透明度（0~1）:

```C#
float light = Random.Range (0, 1);
particleArr [i].startColor = gradient.Evaluate (light);
```

效果如下，可能有点不明显，但其实是一闪一闪的：

![gif4](https://github.com/lossatsea/homework/blob/master/homework7/pictures/4.gif);

#### 7. 扩展和收缩

下面实现点击后收缩和扩展的功能。首先，我们需要两个数组来存储收缩和扩展后的运动半径，好让它们能恢复到原来的轨道上：

```C#
private float[] radius;         //扩展后的每个粒子的运动半径
private float[] collect_radius; //收缩后每个粒子的运动半径
```

在Start函数中分配空间：

```C#
radius = new float[number];
collect_radius = new float[number];
```

但是每个粒子的轨道半径是浮游的，变化的，怎么存到数组里呢？我的方法就是找到那个不变的半径，它就在randomLocationAndSize里，我们最初求的随机运动半径，但是我们只求了扩展后的随机运动半径，现在我们再加上收缩后的随机运动半径：

```C#
void randomLocationAndSize(){
	for (int i = 0; i < number; i++) {
	    //扩展后的随机运动半径
		float MidRadius = (MaxRadius + MinRadius) / 2;
		float outRate = Random.Range (1f, MidRadius / MinRadius);
		float inRate = Random.Range (MaxRadius / MidRadius, 1f);
		float _radius = Random.Range (MinRadius * outRate, MaxRadius * inRate);
		radius[i] = _radius;
		
		//收缩后的随机运动半径
		float collect_MidRadius = (collect_MaxRadius + collect_MinRadius) / 2;
		float collect_outRate = Random.Range (1f, collect_MidRadius / collect_MinRadius);
		float collect_inRate = Random.Range (collect_MaxRadius / collect_MidRadius, 1f);
		float _collect_radius = Random.Range (collect_MinRadius * collect_outRate, collect_MaxRadius * collect_inRate);
		collect_radius[i] = _collect_radius;
        
        //下面保持不变
		float size = Random.Range (MinSize, MaxSize);
		float angleDgree = Random.Range (0, 360f);
		float angle = (angleDgree * Mathf.PI) / 180;

		float time = Random.Range (0, 360f);

		particles [i] = new ParticalInfo (_radius, angleDgree, time, size);
		particleArr [i].position = new Vector3 (particles[i].radius * Mathf.Cos(angle), particles[i].radius * Mathf.Sin(angle), 0);
		particleArr [i].startSize = particles [i].size;
	}
	particleSys.SetParticles (particleArr, particleArr.Length);
}
```

首先我们需要一个变量确定现在是扩展状态还是收缩状态：

```C#
//之所以申请为公共成员是为了iner可以直接进行修改
//0代表扩展状态，1代表收缩状态
public int isCollected = 0;
```

点击事件和半径的变化就在Update函数里进行:

```C#
//获得点击
if(Input.GetMouseButton(0)){
	isCollected = 1 - isCollected;
}

//如果现在要进行收缩
if (isCollected == 1) {

    //半径大的进行收缩直到小于目标半径
	if (particles [i].radius > collect_radius [i]) {  
		particles [i].radius -= 15f * (collect_radius [i] / collect_radius [i]) * Time.deltaTime;  
	} else {
		particles [i].radius = collect_radius [i];
		//由于半径过小就不进行浮游了
	}
} 

//如果现在要进行扩展
else {
    
    //半径小的进行扩展直到大于目标半径
	if (particles [i].radius < radius [i]) {
		particles [i].radius += 15f * (collect_radius [i] / collect_radius [i]) * Time.deltaTime;  
	} else {
		particles [i].time += Time.deltaTime;
		//浮游
		particles [i].radius += Mathf.PingPong (particles [i].time / MinRadius / MaxRadius, pingPong) - pingPong / 2.0f;
	}	
}
```

效果如下：

![gif5](https://github.com/lossatsea/homework/blob/master/homework7/pictures/5.gif);

我们发现会遇到“卡壳”的情况，这是因为点击事件的触发过于敏感，依次点击有可能触发很多次点击事件，因此我们要进行“消抖”：

引入一个次数变量：

```C#
private int time = 0;
```

只有当次数满10次才能触发下一次点击事件：

```C#
time++;
if(Input.GetMouseButton(0) ){
	isCollected = 1 - isCollected;
	time = 0;
}
```

消抖后效果如下：

![gif6](https://github.com/lossatsea/homework/blob/master/homework7/pictures/6.gif);

这样就顺畅很多了。这样outer就大功告成了！

#### 8. 添加逆时针和内环

我们外环已经做好了下面先做iner，iner和outer的区别在于，iner是逆时针，并且初始化时是收缩状态。这么一说就好办了，因为我们就有一个clockWise成员来判断旋转的方向。

根据上面的叙述，我们代码里只需要更改randomLocationAndSize函数就可以了，再初始化随机运动半径时进行判断：

```C#
//外环随机运动半径为扩展状态
if(isCollected == 0)
	particles [i] = new ParticalInfo (_radius, angleDgree, time, size);
	
//内环随机运动半径为收缩状态
else
	particles [i] = new ParticalInfo (_collect_radius, angleDgree, time, size);
```

下面就是在界面里手动调节参数（其实也可以在代码里初始化，但是我懒）：

![inner](https://github.com/lossatsea/homework/blob/master/homework7/pictures/2.png);

#### 最终效果

![gif7](https://github.com/lossatsea/homework/blob/master/homework7/pictures/7.gif);

你还可以改变一下内环和外环的颜色，有哪个颜色条变量，总之随心所欲喽。
