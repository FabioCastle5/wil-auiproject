using UnityEngine;
using System.Collections;

public class ObstacleManager : MonoBehaviour {

	public void SetDirection (int direction) {
		if (direction == 0 || direction == 180)
			transform.Rotate (new Vector3(0f,0f,90f));
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.CompareTag ("Player")) {
			other.gameObject.GetComponent<PlayerController> ().speed *= 0.2f;
		}
	}

	void OnTriggerExit2D(Collider2D other) {
		if (other.gameObject.CompareTag ("Player")) {
			other.gameObject.GetComponent<PlayerController> ().speed *= 5f;
		}
	}
}
