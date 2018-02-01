using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TimerManager : MonoBehaviour {

	public Text timeText;
	private int time;
	private bool stop;

	void Start () {
		time = 0;
		stop = false;
		timeText.text = "00:00";
	}

	public IEnumerator TrackTime () {
		string minutes = "";
		string seconds = "";
		while (!stop) {
			time++;
			minutes = (time / 60).ToString ("00");
			seconds = (time % 60).ToString ("00");
			timeText.text = minutes + ":" + seconds;
			yield return new WaitForSeconds (1);
		}
	}

	public void StopTimer () {
		stop = true;
	}

	public void RestartGame() {
		Application.LoadLevel (0);
	}
}
