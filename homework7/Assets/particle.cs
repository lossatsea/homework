using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particle : MonoBehaviour {

	private ParticleSystem particleSys;
	private ParticleSystem.Particle[] particleArr;
	private ParticalInfo[] particles;
	private int tier = 10;
	private int time = 0;
	private float[] radius;
	private float[] collect_radius;
	private int changeNum;

	public int isCollected = 0;
	public int number = 30000;
	public float MaxSize = 0.1f;
	public float MinSize = 0.03f;
	public float MaxRadius = 12f;
	public float MinRadius = 6f;
	public float collect_MaxRadius = 4f;
	public float collect_MinRadius = 1f;
	public bool clockWise = true;
	public float speed = 2f;
	public float pingPong = 0.02f;
	public Gradient gradient;
	// Use this for initialization
	void Start () {
		particleArr = new ParticleSystem.Particle[number];
		particles = new ParticalInfo[number];
		particleSys = GetComponent<ParticleSystem> ();
		radius = new float[number];
		collect_radius = new float[number];
		changeNum = number;

		particleSys.startSpeed = 0;
		particleSys.loop = false;
		particleSys.maxParticles = number;
		particleSys.Emit (number);
		particleSys.GetParticles (particleArr);

		GradientAlphaKey[] alphaKey = new GradientAlphaKey[5];
		alphaKey [0].time = 0;		alphaKey [0].alpha = 1f;
		alphaKey [1].time = 0.4f;	alphaKey [1].alpha = 0.4f;
		alphaKey [2].time = 0.6f;	alphaKey [2].alpha = 1f;
		alphaKey [3].time = 0.9f;	alphaKey [3].alpha = 0.4f;
		alphaKey [4].time = 1f;		alphaKey [4].alpha = 0.9f;
		GradientColorKey[] colorKey = new GradientColorKey[2];
		colorKey [0].time = 0;		colorKey [0].color = Color.white;
		colorKey [1].time = 1f;		colorKey [1].color = Color.white;
		gradient.SetKeys (colorKey, alphaKey);

		randomLocationAndSize ();
	}
	
	// Update is called once per frame
	void Update () {
		time++;
		if(Input.GetMouseButton(0) && time > 10){
			isCollected = 1 - isCollected;
			time = 0;
		}
		for (int i = 0; i < number; i++) {
			if (clockWise) {
				particles [i].angle -= (i % tier + 1) * (speed / particles[i].radius / tier);
			} else {
				particles [i].angle += (i % tier + 1) * (speed / particles[i].radius / tier);
			}

			float light = Random.Range (0, 1);
			particles [i].angle = (360 + particles [i].angle) % 360f;
			float angle = (particles[i].angle * Mathf.PI) / 180;

			if (isCollected == 1) {
				if (particles [i].radius > collect_radius [i]) {  
					particles [i].radius -= 15f * (collect_radius [i] / collect_radius [i]) * Time.deltaTime;  
				} else {
					particles [i].radius = collect_radius [i];
				}
			} else {
				if (particles [i].radius < radius [i]) {
					particles [i].radius += 15f * (collect_radius [i] / collect_radius [i]) * Time.deltaTime;  
				} else {
					particles [i].time += Time.deltaTime;
					particles [i].radius += Mathf.PingPong (particles [i].time / MinRadius / MaxRadius, pingPong) - pingPong / 2.0f;
				}	
			}
			particleArr [i].position = new Vector3 (particles [i].radius * Mathf.Cos (angle), particles [i].radius * Mathf.Sin (angle), 0);
			particleArr [i].startColor = gradient.Evaluate (light);
		}
		particleSys.SetParticles (particleArr, particleArr.Length);
	}

	void randomLocationAndSize(){
		for (int i = 0; i < number; i++) {
			float MidRadius = (MaxRadius + MinRadius) / 2;
			float outRate = Random.Range (1f, MidRadius / MinRadius);
			float inRate = Random.Range (MaxRadius / MidRadius, 1f);
			float _radius = Random.Range (MinRadius * outRate, MaxRadius * inRate);
			radius[i] = _radius;
			float collect_MidRadius = (collect_MaxRadius + collect_MinRadius) / 2;

			float collect_outRate = Random.Range (1f, collect_MidRadius / collect_MinRadius);
			float collect_inRate = Random.Range (collect_MaxRadius / collect_MidRadius, 1f);
			float _collect_radius = Random.Range (collect_MinRadius * collect_outRate, collect_MaxRadius * collect_inRate);
			collect_radius[i] = _collect_radius;

			float size = Random.Range (MinSize, MaxSize);
			float angleDgree = Random.Range (0, 360f);
			float angle = (angleDgree * Mathf.PI) / 180;

			float time = Random.Range (0, 360f);

			if(isCollected == 0)
				particles [i] = new ParticalInfo (_radius, angleDgree, time, size);
			else
				particles [i] = new ParticalInfo (_collect_radius, angleDgree, time, size);
			particleArr [i].position = new Vector3 (particles[i].radius * Mathf.Cos(angle), particles[i].radius * Mathf.Sin(angle), 0);
			particleArr [i].startSize = particles [i].size;
		}

		particleSys.SetParticles (particleArr, particleArr.Length);
	}
		
}

public class ParticalInfo{
	public float radius;
	public float angle;
	public float time;
	public float size;

	public ParticalInfo(float r, float a, float t, float s){
		radius = r;
		angle = a;
		time = t;
		size = s;
	}
}

