using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public float speed;
	public int rotationAngle;

	private SpriteRenderer drawing;

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

		routeDirection = GameData.data.circuitMananger.GetComponent<CircuitManager> ().GetInitialDirection ();
		offset = - rotationAngle;
		cos = 0f;
		sin = 0f;
		moving = false;
	}

	void Update () {
		if (Input.GetMouseButtonDown (0)) {
			ChangeDirection ();
		}
		if (moving) {
			float newX = gameObject.transform.position.x + speed * Time.deltaTime * cos;
			float newY = gameObject.transform.position.y + speed * Time.deltaTime * sin;
			gameObject.transform.position = new Vector3 (newX, newY, 0f);
		}
	}

	// make the player steer in the complementary direction
	void ChangeDirection () {
		if (!moving) {
			moving = true;
			offset = -offset;
		} else {
			// the direction in inverted
			offset = - offset;
		}
//		// make the player move in the new direction
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

	public int GetRouteDirection () {
		return routeDirection;
	}

	public void SetRouteDirection (int direction) {
		routeDirection = direction;
		offset = -offset;
	}

	public void StopMoving () {
		moving = false;
	}
		
	void OnTriggerEnter2D (Collider2D other) {
		if (other.CompareTag ("Tile")) {
			// the player is hitting a wall
			moving = false;
			// go back for a little
			float newX = this.gameObject.transform.position.x - cos;
			float newY = this.gameObject.transform.position.y - sin;
			this.gameObject.transform.position = new Vector3 (newX, newY, 0f);
		}
	}
}
