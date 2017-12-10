using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TimerManager : MonoBehaviour {

	public Text timeText;
	private int time;
	private float timePassed;

	void Start () {
		time = 0;
		timePassed = 0;
		timeText.text = "00:00";
	}

	void Update () {
		if (GameData.data.started) {
			timePassed += Time.deltaTime;
			if (timePassed >= 1f) {
				time++;
				string minutes = (time / 60).ToString("00");
				string seconds = (time % 60).ToString("00");
				timeText.text = minutes + ":" + seconds;
				timePassed = 0f;
			}
		}
	}
}
