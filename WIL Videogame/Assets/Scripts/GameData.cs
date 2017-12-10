using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameData : MonoBehaviour {

	public static GameData data;

	public GameObject GUIManager;

	public GameObject circuitMananger;
	public GameObject player;

	public bool started;
	public Button startButton;

	// Singleton and data accessible from anywhere
	void Awake () {
		if (data == null) {
			DontDestroyOnLoad (gameObject);
			data = this;
		} else if (data != this) {
			Destroy (gameObject);
		}
	}

	public void PauseGame () {
	}
}
