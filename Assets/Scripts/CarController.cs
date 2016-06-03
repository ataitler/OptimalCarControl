using UnityEngine;
using System.Collections;

public class CarController : MonoBehaviour {

	private GameController gameController = null;
	private Vector3 velocity;
	public Vector3 position;
	private bool reset = false;
	public float MaximumValocity;
	private float dt;


	// Use this for initialization
	void Start () {
		dt = Time.deltaTime;
		GameObject gameControllerObject = GameObject.FindWithTag ("GameController");
		if (gameControllerObject != null) {
			gameController = gameControllerObject.GetComponent<GameController> ();
		}
		else {
			Debug.Log ("Error initializing agent, cannot find game controller");
		}
		velocity = new Vector3 (0, 0, 0);
		this.rigidbody.position = position;
		this.rigidbody.velocity = velocity;
		reset = true;
	}

	void FixedUpdate() {
		if (gameController.State == TrainerState.Training) {
			this.rigidbody.velocity = new Vector3(velocity.x, this.rigidbody.velocity.y, this.rigidbody.velocity.z);
		}
		else {
			if (reset) {
				this.rigidbody.velocity = velocity;
				this.rigidbody.position = position;
				reset = false;
				Debug.Log("reseting agent's values to position:" + position.ToString());
			}
		}
	}

	public void SetAction(float Ux) {
		// TODO: enforce physical constrains
		if (!reset) {
			// first order velocity system with momentum
			float Vnext = (float)(1.0-dt*50)*velocity.x + Ux;
			//float Vnext = Vx;
			Vnext = Mathf.Clamp(Vnext, -MaximumValocity, MaximumValocity);
			velocity.Set (Vnext, 0, 0);
			Debug.Log ("action updated");
		}
	}
	
	public void Reset() {
		reset = true;
		velocity.Set (0, 0, 0);
		//this.rigidbody.position = position;
	}
}
