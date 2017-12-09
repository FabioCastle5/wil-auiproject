using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameLoader : MonoBehaviour {

	public GameObject circuitBuilder;
	public GameObject player;
	public GameObject camera;
	public Image finishImage;

//	public static GameLoader instance = null;

	// Use this for initialization
	void Start () {
//
//		if (instance == null) {
//			instance = this;

		// loads the circuit
		GameObject builder = Instantiate (circuitBuilder, Vector3.zero, Quaternion.identity) as GameObject;
		builder.GetComponent<DataManager> ().BuildCircuit ();

		GameObject circuit = builder.GetComponent<DataManager> ().circuitInstance;
		int direction = circuit.GetComponent<CircuitManager> ().GetInitialDirection ();

		// loads the player and set its initial direction
		GameObject pl = Instantiate(player, Vector3.zero, Quaternion.identity) as GameObject;
		pl.GetComponent<PlayerController> ().SetDirection (direction);
		pl.GetComponent<PlayerController> ().finishScreen = finishImage;

		camera.GetComponent<CameraManager> ().SetPlayer (pl);
//		}
//		else if (instance != this)
//			Destroy(gameObject);
	}
}
