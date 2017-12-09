using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public float speed;
	public int rotationAngle;

	public Image finishScreen;

	private SpriteRenderer renderer;
	private Rigidbody2D body;

	public Sprite ne;
	public Sprite nw;
	public Sprite se;
	public Sprite sw;

	private int routeDirection;
	private int offset;

	void Start () {
		Debug.Log (this.name + " has started!");
		
		// get the sprite renderer associated to the player
		renderer = this.GetComponent<SpriteRenderer> ();
		// get the RigidBody2d associated to the player
		body = this.GetComponent<Rigidbody2D> ();

		offset = 0;

		finishScreen.enabled = false;
	}

	public void SetDirection (int direction) {
		routeDirection = direction;
		offset = -offset;
	}

	// make the player steer in the complementary direction
	void ChangeDirection () {
		if (offset == 0) {
			// at start, and at every change in the routeDirection
			offset = - rotationAngle;
		} else {
			// null the previous force applied to the body
			body.velocity = Vector2.zero;
			// the direction in inverted
			offset = - offset;
		}
		// make the player move in the new direction
		Vector2 newDir = (Vector2) (Quaternion.AngleAxis (routeDirection + offset, Vector3.forward) * Vector3.right);
		body.AddForce (newDir * speed);

		DrawSprite ();
	}

	void DrawSprite () {
		if (routeDirection == 90) {
			if (offset < 0)
				renderer.sprite = ne;
			else
				renderer.sprite = nw;
		} else if (routeDirection == 0) {
			if (offset < 0)
				renderer.sprite = se;
			else
				renderer.sprite = ne;
		} else if (routeDirection == 180) {
			if (offset < 0)
				renderer.sprite = nw;
			else
				renderer.sprite = sw;
		}
	}

	void FixedUpdate () {
		if (Input.GetMouseButtonDown (0))
			ChangeDirection ();
	}

	// this is called when the player cross a curve and pass for the curve's trigger
	void OnTriggerEnter2D (Collider2D other) {
		if (other.CompareTag ("Tile")) {
			string curveName = other.gameObject.name;
			if (curveName == "TileCurveNE" || curveName == "TileCurveNE(Clone)")
				routeDirection = 0;
			else if (curveName == "TileCurveNW" || curveName == "TileCurveNW(Clone)")
				routeDirection = 180;
			else
				routeDirection = 90;
			
			other.enabled = false;
		} else if (other.CompareTag ("Flag")) {
			// game is finished
			body.velocity = Vector2.zero;
			finishScreen.enabled = true;
			gameObject.SetActive (false);
		}
	}
}
