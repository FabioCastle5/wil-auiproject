using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameDataHandler : MonoBehaviour {

	public float actualAccX;
	public float actualAccY;
	public bool connected;
	public string logFileName;

	public static GameDataHandler dataHandler = null;

	void Start () {
		if (dataHandler == null) {
			dataHandler = this;
			actualAccX = 0f;
			actualAccY = 0f;
			connected = false;
		} else if (dataHandler != this) {
			Destroy (gameObject);
		}
	}
}
