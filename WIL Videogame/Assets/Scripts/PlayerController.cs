using UnityEngine;
using System;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public Sprite[] sprites;
	public float speed;
	public int rotationAngle;

	private SpriteRenderer renderer;
	private Rigidbody2D body;

	private int routeDirection;
	private int offset;

	void Start () {
		
		// get the sprite renderer associated to the player
		renderer = this.GetComponent<SpriteRenderer> ();
		// get the RigidBody2d associated to the player
		body = this.GetComponent<Rigidbody2D> ();

		routeDirection = 90;
		offset = 0;

		ChangeDirection ();
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
		if (offset < 0)
			renderer.sprite = sprites [0];
		else
			renderer.sprite = sprites [1];
	}

	//void OnCollisionEnter (Collision other) {
	//	// touching a wall make the player to slow down
	//	if (other.gameObject.CompareTag ("Tile")) {
	//		speed /= 2;
	//		collided = true;
	//	}
	//}

	void FixedUpdate () {
		if (Input.GetMouseButtonDown (0))
			ChangeDirection ();
	}

}
