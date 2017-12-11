using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameData : MonoBehaviour {

	public static GameData data;

	public int sceneToBeLoaded;
	public int maxNumberOfTiles;

	public GameObject GUIManager;

	public GameObject circuitMananger;
	public GameObject player;
	public GameObject timer;

	public Button startButton;
	public Image finishScreen;

	// Singleton and data accessible from anywhere
	void Awake () {
		if (data == null) {
			DontDestroyOnLoad (gameObject);
			data = this;
			maxNumberOfTiles = 15;
		} else if (data != this) {
			Destroy (gameObject);
		}
	}

	public void SetPlayScene () {
		sceneToBeLoaded = 2;
		Application.LoadLevel (1);
	}

	public void SetCreateScene () {
		sceneToBeLoaded = 3;
		Application.LoadLevel (1);
	}

	public void SetSCircuit () {
		maxNumberOfTiles = 6;
	}

	public void SetMCircuit () {
		maxNumberOfTiles = 15;
	}

	public void SetLCircuit () {
		maxNumberOfTiles = 25;
	}

	public void PauseGame () {
	}

	public void RestartGame () {
		Application.LoadLevel (0);
	}
}
