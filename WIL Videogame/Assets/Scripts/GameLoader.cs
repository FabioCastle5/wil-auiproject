using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameLoader : MonoBehaviour {

	public GameObject circuitBuilder;
	public GameObject player;
	public GameObject timer;
	public GameObject main_camera;
	public Image finishImage;
	public Image advanceBar;
	public Button startButton;

	// Use this for initialization
	void Start () {

		GameData.data.startButton = startButton;
		finishImage.enabled = false;
		GameData.data.finishScreen = finishImage;

		// loads the circuit
		GameObject builder = Instantiate (circuitBuilder, Vector3.zero, Quaternion.identity) as GameObject;
		builder.GetComponent<DataManager> ().BuildCircuit ();

		GameObject circuit = builder.GetComponent<DataManager> ().circuitInstance;
		circuit.GetComponent<CircuitManager> ().advanceBar = advanceBar;
		circuit.GetComponent<CircuitManager> ().StartDrawing ();

		GameData.data.circuitMananger = circuit;

		Destroy (builder);
		Destroy (CircuitData.data);

		// loads the player
		GameObject pl = Instantiate(player, Vector3.zero, Quaternion.identity) as GameObject;

		GameData.data.player = pl;
		GameData.data.timer = timer;

		main_camera.GetComponent<CameraManager> ().SetPlayer (pl);
	}

	public void StartGame() {
		timer.GetComponent<TimerManager> ().StartCoroutine ("TrackTime");
		Destroy (startButton);
		Destroy (gameObject);
	}
}
