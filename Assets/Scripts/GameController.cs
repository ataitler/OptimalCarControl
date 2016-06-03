using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	public RewardManager reward;
	private TrainerState state;
	public CommunicationController communication;
	public CarController car;
	public bool trainingDelay;
	public float Timer;
	private float _Timer;
	private long iterNum;
	public GUIText IterationLabel;

	public TrainerState State
	{
		get { return state; }
	}

	// Use this for initialization
	void Start () {
		//state = TrainerState.Disconnected;
		state = TrainerState.Init;
		trainingDelay = true;
		UpdateState (TrainerState.Disconnected);
		Random.seed = 42;
		_Timer = Timer;
		iterNum = 0;
	}

	// FixedUpdate is called once every physical time step
	void FixedUpdate () {
		if (state == TrainerState.Training) {
			string carData = car.rigidbody.position.x.ToString("F3") + "," + car.rigidbody.velocity.x.ToString("F3");
			
			string msg = "<Message>:" + carData;
			communication.Send(msg);
			
			_Timer -= Time.fixedDeltaTime;
			if (_Timer <= 0) {
				//reward.Timeout();
				UpdateState(TrainerState.IdleTraining);
			}
		}
		// intialize new episode
		else if (state == TrainerState.IdleTraining) {
			if (trainingDelay) {
				// reset all positions and velocities
				car.Reset();
				Debug.Log("reseting objects");
				// send reward
				
				trainingDelay = false;
				
				_Timer = Timer;
				
				IterationLabel.text = "Iteration Number: " + iterNum.ToString();
			}
			else {
				UpdateState(TrainerState.Training);
				trainingDelay = true;
				iterNum++;
			}
		}
	}

	// Update is called once per frame
	void Update () {
		
		// move to disconnected state
		if (Input.GetKeyDown (KeyCode.Q)) {
			//disconnect communication
			communication.ShutDown(false);
			// Quit simulator
			UpdateState(TrainerState.Disconnected);
		}
		
		if (state == TrainerState.IdleTraining || state == TrainerState.Training) {
			// stop game and move to idle state
			if (Input.GetKeyDown (KeyCode.S)) {
				UpdateState (TrainerState.Idle);
			}
		}
		if (state == TrainerState.Idle) {
			// start training
			if (Input.GetKeyDown(KeyCode.R)) {
				// Start trainning
				UpdateState(TrainerState.IdleTraining);
			}
		}
	}

	public void UpdateState (TrainerState st) {
		// handle the state machine
		switch (st) {
		case TrainerState.Disconnected:
			if (state != TrainerState.Disconnected)
				Debug.Log("Disconnected state");
			break;
		case TrainerState.Idle:
			car.Reset();
			// wait to start a new training session
			Debug.Log("Idle state");
			break;
		case TrainerState.Init:
			// obselete
			break;
		case TrainerState.Training:
			Debug.Log("Training state");
			// start new episode
			//reward.Reset();
			break;
		case TrainerState.IdleTraining:
			Debug.Log("IdleTraining state");
			if (state == TrainerState.Training) {
				 //send reward
				communication.Send("<Reward>:" + reward.GetReward().ToString());
				reward.Reset();
			}
			break;
		default:
			break;
		}
		// update the state
		if (state != st)
			state = st;
	}


}
