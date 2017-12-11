using UnityEngine;
using System.Collections;

public class FlagHandler : MonoBehaviour {

	void OnTriggerEnter2D (Collider2D other) {
		// make the game finish
		if (other.CompareTag ("Player")) {
			other.GetComponent<PlayerController> ().StopMoving ();
			GameData.data.timer.GetComponent<TimerManager> ().StopTimer ();
			CircuitManager circuit = GameData.data.circuitMananger.GetComponent<CircuitManager> ();
			circuit.StartCoroutine ("NextCheckpoint");
			GameData.data.finishScreen.enabled = true;
			other.gameObject.SetActive (false);
		}
	}
}
