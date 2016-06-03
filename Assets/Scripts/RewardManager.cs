using UnityEngine;
using System.Collections;

public class RewardManager : MonoBehaviour {

	private long counter;
	private GameController gameController = null;

	// Use this for initialization
	void Start () {
		counter = 0;

		GameObject gameControllerObject = GameObject.FindWithTag ("GameController");
		if (gameControllerObject != null) {
			gameController = gameControllerObject.GetComponent<GameController> ();
		}
		else {
			Debug.Log ("Error initializing agent, cannot find game controller");
		}

	}
	
	// Update is called once per frame
	void FixUpdate () {
		if (gameController.State == TrainerState.Training)
		{
			counter++;
		}
	}

	public double GetReward() {
		double reward = CalculateReward ();
		Reset ();
		// reward formula...
		return reward;
	}

	public void Reset() {
		counter = 0;
	}

	private double CalculateReward()
	{
		double reward = 0;
		
		return reward;
	}

}
