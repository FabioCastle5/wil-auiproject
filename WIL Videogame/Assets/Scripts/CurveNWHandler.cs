using UnityEngine;
using System.Collections;

public class CurveNWHandler : MonoBehaviour {

	void OnTriggerEnter2D (Collider2D other) {
		if (other.CompareTag ("Player")) {
			int direction = other.GetComponent<PlayerController> ().GetRouteDirection ();
			int newDir = 0;
			if (direction == 90)
				newDir = 180;
			else // was 0
				newDir = 270;

			other.GetComponent<PlayerController> ().SetRouteDirection (newDir);
			Destroy (gameObject);
		}
	}
}
