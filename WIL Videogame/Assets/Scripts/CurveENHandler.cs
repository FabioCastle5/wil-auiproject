using UnityEngine;
using System.Collections;

public class CurveENHandler : MonoBehaviour {

	void OnTriggerEnter2D (Collider2D other) {
		if (other.CompareTag ("Player")) {
			int direction = other.GetComponent<PlayerController> ().GetRouteDirection ();
			int newDir = 0;
			if (direction == 0)
				newDir = 90;
			else // was 270
				newDir = 180;

			other.GetComponent<PlayerController> ().SetRouteDirection (newDir);
			Destroy (gameObject);
		}
	}
}
