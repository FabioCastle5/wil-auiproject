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
	public GameObject obstacleGenerator;

	public Button startButton;
	public Image finishScreen;
	public Button restartButton;

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

	public void PauseGame () {
	}

	public void RestartGame () {
		Application.LoadLevel (0);
	}
}
