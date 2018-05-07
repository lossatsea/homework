using UnityEngine;
using System.Collections;

public class CamareFollow : MonoBehaviour
{
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

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}
		
}

