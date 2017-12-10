using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameLoader : MonoBehaviour {

	public GameObject circuitBuilder;
	public GameObject player;
	public GameObject main_camera;
	public Image finishImage;
	public Image advanceBar;
	public Button startButton;

//	public static GameLoader instance = null;

	// Use this for initialization
	void Start () {
//
//		if (instance == null) {
//			instance = this;

		GameData.data.startButton = startButton;

		// loads the circuit
		GameObject builder = Instantiate (circuitBuilder, Vector3.zero, Quaternion.identity) as GameObject;
		builder.GetComponent<DataManager> ().BuildCircuit ();

		GameObject circuit = builder.GetComponent<DataManager> ().circuitInstance;
		circuit.GetComponent<CircuitManager> ().advanceBar = advanceBar;
		circuit.GetComponent<CircuitManager> ().StartDrawing ();

		GameData.data.circuitMananger = circuit;

		// loads the player and set its initial direction
		GameObject pl = Instantiate(player, Vector3.zero, Quaternion.identity) as GameObject;
		pl.GetComponent<PlayerController> ().finishScreen = finishImage;

		GameData.data.player = pl;

		main_camera.GetComponent<CameraManager> ().SetPlayer (pl);
//		}
//		else if (instance != this)
//			Destroy(gameObject);
	}

	public void StartGame() {
		if (! GameData.data.started)
			GameData.data.started = true;
		Destroy (startButton);
	}
}
