using UnityEngine;
using System.Collections;

public class GameData : MonoBehaviour {

	public static GameData data;

	public GameObject GUIManager;

	// Singleton and data accessible from anywhere
	void Awake () {
		if (data == null) {
			DontDestroyOnLoad (gameObject);
			data = this;
		} else if (data != this) {
			Destroy (gameObject);
		}
	}
}
