using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour {

	public int defaultNumberOfTiles;

	void Start () {
		GameData.data.maxNumberOfTiles = defaultNumberOfTiles;
	}

	public void SetPlayScene () {
		GameData.data.sceneToBeLoaded = 2;
		Application.LoadLevel (1);
	}

	public void SetCreateScene () {
		GameData.data.sceneToBeLoaded = 3;
		Application.LoadLevel (1);
	}

	public void SetSCircuit () {
		GameData.data.maxNumberOfTiles = 10;
	}

	public void SetMCircuit () {
		GameData.data.maxNumberOfTiles = 25;
	}

	public void SetLCircuit () {
		GameData.data.maxNumberOfTiles = 50;
	}
}
