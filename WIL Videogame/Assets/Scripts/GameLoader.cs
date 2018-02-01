using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameLoader : MonoBehaviour {

	public GameObject circuitBuilder;
	public GameObject player;
	public GameObject timer;
	public GameObject obstacleManager;
	public GameObject main_camera;
	public Image finishImage;
	public Image advanceBar;
	public Button startButton;
	public Button restartButton;

	// Use this for initialization
	void Start () {

		GameData.data.startButton = startButton;
		finishImage.enabled = false;
		restartButton.interactable = false;
		restartButton.gameObject.SetActive (false);
		GameData.data.finishScreen = finishImage;
		GameData.data.restartButton = restartButton;

		// loads the circuit
		GameObject builder = Instantiate (circuitBuilder, Vector3.zero, Quaternion.identity) as GameObject;
		builder.GetComponent<DataManager> ().BuildCircuit ();

		GameObject circuit = builder.GetComponent<DataManager> ().circuitInstance;
		circuit.GetComponent<CircuitManager> ().advanceBar = advanceBar;
		circuit.GetComponent<CircuitManager> ().StartDrawing ();

		GameData.data.circuitMananger = circuit;

		Destroy (builder);
		Destroy (CircuitData.data);

		float x = circuit.GetComponent<CircuitManager> ().GetInitialX ();
		float y = circuit.GetComponent<CircuitManager> ().GetInitialY ();

		// loads the player
		GameObject pl = Instantiate(player, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
		obstacleManager.GetComponent<ObstacleGenerator> ().player = pl;

		GameData.data.player = pl;
		GameData.data.timer = timer;
		GameData.data.obstacleGenerator = obstacleManager;

		main_camera.GetComponent<CameraManager> ().SetPlayer (pl);
	}

	public void StartGame() {
		timer.GetComponent<TimerManager> ().StartCoroutine ("TrackTime");
		obstacleManager.GetComponent<ObstacleGenerator> ().StartCoroutine ("StartGeneration");
		Destroy (startButton);
		Destroy (gameObject);
	}
}
