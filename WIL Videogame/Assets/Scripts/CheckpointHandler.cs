using UnityEngine;
using System.Collections;

public class CheckpointHandler : MonoBehaviour {

	void OnTriggerEnter2D (Collider2D other) {
		if (other.CompareTag ("Player")) {
			Debug.Log ("Checkpoint reached");
			CircuitManager circuit = GameData.data.circuitMananger.GetComponent<CircuitManager> ();
			circuit.StartCoroutine ("NextCheckpoint");
			Destroy (gameObject);
		}
	}
}
