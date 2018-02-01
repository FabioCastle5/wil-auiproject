using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

	public TargetController target;
	public InputController input;
	public Text connectedText;
	public Text accXText;
	public Text accYText;


	private float latestAx;
	private float latestAy;

	private string fileName;


	void Start () {
		accXText.text = "AccX = 0.0";
		accYText.text = "AccY = 0.0";
		connectedText.text = "Connected: false";

		latestAx = 0f;
		latestAy = 0f;

		fileName = Application.dataPath + "Scripts/Movement Tracking/Measures/";
		fileName += "run-" + System.DateTime.Now.ToString("dd-MM-yyyy") + "-" + System.DateTime.Now.ToString("hh-mm-ss");
		Debug.Log ("Saving to: " + fileName);
		GameDataHandler.dataHandler.logFileName = fileName;

		input.Activate ();
	}

	void Update () {
		connectedText.text = "Connected: " + GameDataHandler.dataHandler.connected;
		if (latestAx != GameDataHandler.dataHandler.actualAccX || latestAy != GameDataHandler.dataHandler.actualAccY) {
			latestAx = GameDataHandler.dataHandler.actualAccX;
			accXText.text = "AccX = " + latestAx.ToString ();
			latestAy = GameDataHandler.dataHandler.actualAccY;
			accYText.text = "AccY = " + latestAy.ToString ();
			target.setAcceleration (latestAx, latestAy);
		}
	}
}
