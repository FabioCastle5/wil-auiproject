using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public float speed;
	public int rotationAngle;

	public Image finishScreen;

	private SpriteRenderer drawing;
	//private Rigidbody2D body;

	public Sprite ne;
	public Sprite nw;
	public Sprite se;
	public Sprite sw;

	private int routeDirection;
	private int offset;
	private float cos;
	private float sin;

	private bool moving;

	void Start () {
		Debug.Log (this.name + " has started!");
		
		// get the sprite renderer associated to the player
		drawing = this.GetComponent<SpriteRenderer> ();
		// get the RigidBody2d associated to the player
		//body = this.GetComponent<Rigidbody2D> ();

		routeDirection = GameData.data.circuitMananger.GetComponent<CircuitManager> ().GetInitialDirection ();
		offset = 0;
		cos = 0f;
		sin = 0f;
		moving = false;

		finishScreen.enabled = false;
	}

	void Update () {
		if (moving) {
			float newX = gameObject.transform.position.x + speed * Time.deltaTime * cos;
			float newY = gameObject.transform.position.y + speed * Time.deltaTime * sin;
			gameObject.transform.position = new Vector3 (newX, newY, 0f);
		}
	}

	// make the player steer in the complementary direction
	void ChangeDirection () {
		if (!moving)
			moving = true;
		if (offset == 0) {
			// at start, and at every change in the routeDirection
			offset = - rotationAngle;
		} else {
//			// null the previous force applied to the body
//			body.velocity = Vector2.zero;
			// the direction in inverted
			offset = - offset;
		}
//		// make the player move in the new direction
//		Vector2 newDir = (Vector2) (Quaternion.AngleAxis (routeDirection + offset, Vector3.forward) * Vector3.right);
//		body.AddForce (newDir * speed);

		float angleRad = (routeDirection + offset) * Mathf.Deg2Rad;
		cos = Mathf.Cos(angleRad);
		sin = Mathf.Sin (angleRad);

		DrawSprite ();
	}

	void DrawSprite () {
		if (routeDirection == 90) {
			if (offset < 0)
				drawing.sprite = ne;
			else
				drawing.sprite = nw;
		} else if (routeDirection == 270) {
			if (offset < 0)
				drawing.sprite = sw;
			else
				drawing.sprite = se;
		} else if (routeDirection == 0) {
			if (offset < 0)
				drawing.sprite = se;
			else
				drawing.sprite = ne;
		} else if (routeDirection == 180) {
			if (offset < 0)
				drawing.sprite = nw;
			else
				drawing.sprite = sw;
		}
	}

	void FixedUpdate () {
		if (Input.GetMouseButtonDown (0)) {
			ChangeDirection ();
		}
	}
		
	void OnTriggerEnter2D (Collider2D other) {
		if (other.CompareTag ("Tile")) {
			// the player is hitting a wall
			moving = false;
		} else if (other.CompareTag ("Curve")) {
			// this is called when the player cross a curve and pass for the curve's trigger
			string curveName = other.transform.parent.name;
			Debug.Log ("Entered " + curveName);
			if (curveName == "TileCurveNE" || curveName == "TileCurveNE(Clone)") {
				if (routeDirection == 90)
					routeDirection = 0;
				else //was 180
					routeDirection = 270;
			} else if (curveName == "TileCurveNW" || curveName == "TileCurveNW(Clone)") {
				if (routeDirection == 90)
					routeDirection = 180;
				else // was 0
					routeDirection = 270;
			} else if (curveName == "TileCurveEN" || curveName == "TileCurveEN(Clone)") {
				if (routeDirection == 0)
					routeDirection = 90;
				else // was 270
					routeDirection = 180;
			} else { // was WN
				if (routeDirection == 180)
					routeDirection = 90;
				else // was 270
					routeDirection = 0;
			}
			other.enabled = false;
			offset = 0;
		} else if (other.CompareTag ("Flag")) {
			// this is called when the player hits the final flag: game finished!
			moving = false;
			GameData.data.started = false;
			CircuitManager circuit = GameData.data.circuitMananger.GetComponent<CircuitManager> ();
			circuit.NextCheckpoint ();
			finishScreen.enabled = true;
			gameObject.SetActive (false);
		} else if (other.CompareTag ("Checkpoint")) {
			//other.gameObject.SetActive (false);
			Destroy (other.gameObject);
			Debug.Log ("Checkpoint reached");
			CircuitManager circuit = GameData.data.circuitMananger.GetComponent<CircuitManager> ();
			circuit.NextCheckpoint ();
		}
	}
}
