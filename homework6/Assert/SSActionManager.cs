using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SSActionManager : MonoBehaviour, ISSActionCallback {

	private Director director;
	private Dictionary<int, SSAction> actions = new Dictionary<int, SSAction> ();
	private List<SSAction> waitingAdd = new List<SSAction> ();
	private List<int> waitingDelete = new List<int> ();
	// Use this for initialization
	void Start () {
		director = Director.getInstance ();
	}

	// Update is called once per frame
	protected void Update () {
		foreach (SSAction ac in waitingAdd) {
			actions [ac.GetInstanceID ()] = ac;
		}
		waitingAdd.Clear ();

		foreach (KeyValuePair<int, SSAction> kv in actions) {
			SSAction ac = kv.Value;
			if (ac.destroy) {
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

	public void RunAction(GameObject gameobject, SSAction action, ISSActionCallback manager){
		action.gameobject = gameobject;
		action.transform = gameobject.transform;
		action.callback = manager;
		waitingAdd.Add (action);
		action.Start ();
	}

	public void DestroyAll()
	{
		foreach (KeyValuePair<int, SSAction> kv in actions)
		{
			SSAction ac = kv.Value;
			ac.destroy = true;
			SSActionEvent (ac);
		}
	}

	public void StopAll(){
		foreach (KeyValuePair<int, SSAction> kv in actions)
		{
			SSAction ac = kv.Value;
			ac.stand ();
		}
	}

	public void SSActionEvent (SSAction source, 
		SSActionEventType events = SSActionEventType.Competeted,
		int intParam = 0,
		string strParam = null,
		Object objectParam = null){
		if (source is PatrolAction) {
			source.gameobject.GetComponent<PatrolData> ().isGate = false;
			source.gameobject.GetComponent<PatrolData> ().isWall = false;
			source.gameobject.GetComponent<PatrolData> ().isFollow = false;
			source.gameobject.GetComponent<Animator>().SetBool("isRun", false);
			source.gameobject.GetComponent<Animator>().SetBool("isWalk", false);
			source.gameobject.SetActive (false);
			director.scence.factory.freePatrol (source.gameobject);
		}
	}
}
