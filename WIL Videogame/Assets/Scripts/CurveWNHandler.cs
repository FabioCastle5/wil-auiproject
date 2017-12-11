using UnityEngine;
using System.Collections;

public class CurveWNHandler : MonoBehaviour {

	void OnTriggerEnter2D (Collider2D other) {
		if (other.CompareTag ("Player")) {
			int direction = other.GetComponent<PlayerController> ().GetRouteDirection ();
			int newDir = 0;
			if (direction == 180)
				newDir = 90;
			else // was 270
				newDir = 0;

			other.GetComponent<PlayerController> ().SetRouteDirection (newDir);
			Destroy (gameObject);
		}
	}
}
