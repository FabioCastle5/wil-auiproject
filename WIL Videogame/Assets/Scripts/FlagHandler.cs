using UnityEngine;
using System.Collections;

public class FlagHandler : MonoBehaviour {

	void OnTriggerEnter2D (Collider2D other) {
		// make the game finish
		if (other.CompareTag ("Player")) {
			other.GetComponent<PlayerController> ().StopMoving ();
			GameData.data.timer.GetComponent<TimerManager> ().StopTimer ();
			GameData.data.obstacleGenerator.GetComponent<ObstacleGenerator> ().StopGeneration ();
			CircuitManager circuit = GameData.data.circuitMananger.GetComponent<CircuitManager> ();
			circuit.StartCoroutine ("NextCheckpoint");
			GameData.data.finishScreen.enabled = true;
			GameData.data.restartButton.gameObject.SetActive (true);
			GameData.data.restartButton.interactable = true;
			other.gameObject.SetActive (false);
		}
	}
}
