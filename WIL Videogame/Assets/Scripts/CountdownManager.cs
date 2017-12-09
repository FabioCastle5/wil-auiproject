using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Diagnostics;
using UnityEngine.UI;

public class CountdownManager : MonoBehaviour {

	public int measuringSeconds;
	public Text countdownText;

	private Thread timerThread;
	private Thread secondThread;

	private bool second;

	private bool started;
	private volatile bool finished;
	private volatile int secondsPassed;
	private int lastSeconds;

	void Start () {
		countdownText.text = "";
		timerThread = new Thread (CountDown);
		timerThread.IsBackground = true;
		secondThread = new Thread (WaitASecond);
		secondThread.IsBackground = true;
		started = false;
		finished = false;
		second = false;
		lastSeconds = -1;
		secondsPassed = 0;
	}

	void Update () {
		// check whether the countdown has finished, and if it the case, makes the program to carry on
		if (finished) {
			started = false;
			finished = false;
			if (!second) {
				if (timerThread.IsAlive)
					timerThread.Abort ();
				countdownText.text = "";
				GameData.data.GUIManager.GetComponent<WilDataManager> ().EndReadings ();
			} else {
				if (secondThread.IsAlive)
					secondThread.Abort ();
				UnityEngine.Debug.Log ("Waited a second");
				GameData.data.GUIManager.GetComponent<WilDataManager> ().ElaborateEntries ();
			}
		} else if (started) {
			if (secondsPassed != lastSeconds) {
				UnityEngine.Debug.Log (secondsPassed + "s passed");
				lastSeconds++;
				int remainingSecs = measuringSeconds - secondsPassed;
				string minutes = Mathf.Floor(remainingSecs / 60).ToString("00");
				string seconds = (remainingSecs % 60).ToString("00");
				countdownText.text = minutes + ":" + seconds;
			}
		}
	}

	// called by the main thread to start the timer for taking the measures
	public void StartTimer() {
		UnityEngine.Debug.Log ("Setting up the timer thread");
		started = true;
		timerThread.Start ();
	}

	// called to wait some seconds to display the screens
	public void StartSecondTimer() {
		UnityEngine.Debug.Log ("Waiting a second");
		second = true;
		secondThread.Start ();
	}

	void CountDown () {
		for (int i = 0; i < measuringSeconds; i++) {
			secondsPassed = i;
			// sleeps for one seconds, than go ahead until the seconds are passed
			Thread.Sleep (1000);
		}
		// in the end, measuringSeconds are passed, so notify to the measureManager
		finished = true;
	}

	void WaitASecond() {
		Thread.Sleep (1000);
		finished = true;
	}

	void OnDestroy() {
		if (timerThread.IsAlive)
			timerThread.Abort ();
		if (secondThread.IsAlive)
			secondThread.Abort ();
	}
}
