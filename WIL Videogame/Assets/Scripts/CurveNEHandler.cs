using UnityEngine;
using System.Collections;

public class CurveNEHandler : MonoBehaviour {

	void OnTriggerEnter2D (Collider2D other) {
		if (other.CompareTag ("Player")) {
			int direction = other.GetComponent<PlayerController> ().GetRouteDirection ();
			int newDir = 0;
			if (direction == 90)
				newDir = 0;
			else //was 180
				newDir = 270;

			other.GetComponent<PlayerController> ().SetRouteDirection (newDir);
			Destroy (gameObject);
		}
	}
}
