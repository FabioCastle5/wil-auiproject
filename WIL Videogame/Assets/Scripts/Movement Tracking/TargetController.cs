using UnityEngine;
using System.Collections;

public class TargetController : MonoBehaviour {

	public float factor;

	Vector2 acceleration;
	Vector2 velocity;

	LowPassFilter vxFilter;
	LowPassFilter vyFilter;
	LowPassFilter xFilter;
	LowPassFilter yFilter;

	void Start () {
		vxFilter = new LowPassFilter (factor);
		vyFilter = new LowPassFilter (factor);
		xFilter = new LowPassFilter (factor);
		yFilter = new LowPassFilter (factor);
		acceleration = new Vector2 (0f, 0f);
		velocity = new Vector2(0f, 0f);
	}

	public void setAcceleration (float ax, float ay) {
		//acceleration = new Vector2 (ax, ay);
		acceleration.x = ax;
		acceleration.y = ay;
	}

	void Update () {
		// move the target based on the input acceleration got
		float vx = velocity.x + acceleration.x * Time.deltaTime;
		float vy = velocity.y + acceleration.y * Time.deltaTime;
		vx = vxFilter.Step (vx);
		vy = vyFilter.Step (vy);
		float x = transform.position.x + vx * Time.deltaTime;
		float y = transform.position.y + vy * Time.deltaTime;
		x = xFilter.Step (x);
		y = yFilter.Step (y);
		transform.position = new Vector3 (x, y, 0f);
		//velocity = new Vector2 (vx, vy);
		velocity.x = vx;
		velocity.y = vy;
	}
}
