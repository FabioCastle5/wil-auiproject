using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Diagnostics;

/*
 * This script is run in the screen where the circuit building is host.
 * The screen shows a countdown to registrate a new circuit with the toy.
 * The purpose of the script is to connect to the Wemos D1 in the toy and
 * to get the movement data that are sent by it. These data will be saved
 * in an apposite circuit data file, which will hold the latest circuit to be
 * built in the game screen.
 */
using System.Collections;

public class WilDataManager : MonoBehaviour {

	public GameObject clientHandler;
	public GameObject countdownManager;
	public GameObject createButton;
	public GameObject startButton;
	public GameObject stopButton;
	public int maxPoints;

	private List<string> entries;

	private const int readingTime = 30;
	private const float threshold = 0.1f;
	private const float filterFactor = 0.65f;

	public Sprite countdown;
	public Sprite countdownEnd;
	public Sprite building;
	public Sprite ready;


	void Start () {
		UnityEngine.Debug.Log ("The Data Reveiver has started");
		entries = new List<string>();

		startButton.GetComponent<ButtonHandler> ().DisableClick ();
		stopButton.GetComponent<ButtonHandler> ().DisableClick ();
		GameData.data.GUIManager = gameObject;
	}

	public void startReadings() {
		UnityEngine.Debug.Log ("Start readings");
		createButton.GetComponent<ButtonHandler> ().DisableClick ();
		stopButton.SetActive (true);
		GetComponent<SpriteRenderer> ().sprite = countdown;
		clientHandler.GetComponent<MeasureManager> ().ConnectAndReadData ();
		countdownManager.GetComponent<CountdownManager> ().StartTimer();
	}

	public void StopReadings() {
		UnityEngine.Debug.Log ("Countdown finished");
		countdownManager.GetComponent<CountdownManager> ().StopTimer ();
		clientHandler.GetComponent<MeasureManager> ().SetFinished (true);
		stopButton.GetComponent<ButtonHandler> ().DisableClick ();
		GetComponent<SpriteRenderer> ().sprite = countdownEnd;
		entries = clientHandler.GetComponent<MeasureManager> ().GetEntries ();
		countdownManager.GetComponent<CountdownManager> ().StartSecondTimer ();
	}

	public void EndReadings() {
		UnityEngine.Debug.Log ("Countdown finished");
		clientHandler.GetComponent<MeasureManager> ().SetFinished (true);
		stopButton.GetComponent<ButtonHandler> ().DisableClick ();
		GetComponent<SpriteRenderer> ().sprite = countdownEnd;
		entries = clientHandler.GetComponent<MeasureManager> ().GetEntries ();
		countdownManager.GetComponent<CountdownManager> ().StartSecondTimer ();
	}



	public void ElaborateEntries() {

		// create a new file to save the new cicuit
		string newFile = "circuit-" + System.DateTime.Now.ToString("dd-MM-yyyy") + "-" + System.DateTime.Now.ToString("hh-mm-ss");
		CircuitData.data.circuitName = newFile;

		GetComponent<SpriteRenderer> ().sprite = building;

		List<float> xList = new List<float> ();
		List<float> yList = new List<float> ();
		List<int> moveX = new List<int> ();
		List<int> moveY = new List<int> ();
		List<int> posX = new List<int> ();
		List<int> posY = new List<int> ();

		LowPassFilter xFilter = new LowPassFilter (filterFactor);
		LowPassFilter yFilter = new LowPassFilter (filterFactor);

		int i = 0;
		int j = 0;

		string entry;
		// ax(f);ay(f) - format of the single entry in entries
		for (i = 0; i < entries.Count; i++) {
			entry = entries [i];
			if (entry != null) {
				string[] split = entry.Split (';');
				// split[0] contains ax; split[1] contains ay
				UnityEngine.Debug.Log("x read: " + split[0]);
				UnityEngine.Debug.Log ("y read: " + split [1]);
				var ax = float.Parse (split [0]);
				var ay = float.Parse (split [1]);
				xList.Add (ax);
				yList.Add (ay);
			}
		}

		// filter the data
		for (i = 0; i < xList.Count; i++) {
			xList [i] = xFilter.Step (xList [i]);
			yList [i] = yFilter.Step (yList [i]);
		}

		int movex, movey;
		// convert data into motion detection values
		for (i = 0; i < xList.Count; i++) {
			// movex
			if (xList [i] > threshold) {
				movex = 1;
			} else if (xList [i] < -threshold) {
				movex = -1;
			} else {
				movex = 0;
			}
			//movey
			if (yList [i] > threshold) {
				movey = 1;
			} else if (yList [i] < -threshold) {
				movey = -1;
			} else {
				movey = 0;
			}
			moveX.Add (movex);
			moveY.Add (movey);
		}

		// 1st operation: removes the (0,0) moves, which are not useful to evaluate the circuit
		List<int> removeIndex = new List<int>();
		for (i = 0; i < moveX.Count; i++) {
			if (moveX [i] == 0 && moveY [i] == 0)
				removeIndex.Add (i);
		}

		removeIndex.Reverse ();
		for (i = 0; i < removeIndex.Count; i++) {
			moveX.RemoveAt (removeIndex [i]);
			moveY.RemoveAt (removeIndex [i]);
		}
			
		// 2nd operation: removes the unsupported change in direction, that are the ones
		// that makes the player going back on its current path(180° curves)
		// note: two movement vectors are opposite if their sum is vector (0,0)
		removeIndex = new List<int>();
		i = 1;
		j = 0;

		while (i < moveX.Count) {
			if ((moveX [i] + moveX [j] == 0) && (moveY [i] + moveY [j] == 0)) {
				// i make an opposite movement wrt j -> remove i
				removeIndex.Add (i);
			} else {
				// movements i and j aren't opposite, i is the new comparator
				j = i;
			}
			i++;
		}

		removeIndex.Reverse ();
		for (i = 0; i < removeIndex.Count; i++) {
			moveX.RemoveAt (removeIndex [i]);
			moveY.RemoveAt (removeIndex [i]);
		}

		// 3rd operation: remove points in order to stay in the maximum number of tiles
		maxPoints = GameData.data.maxNumberOfTiles;
		int lastx = 0;
		int lasty = 0;
		if (moveX.Count > 0) {
			lastx = moveX [0];
			lasty = moveY [0];
		}
		bool bruteNeeded = false;

		while (moveX.Count > maxPoints + 2) {
			removeIndex = new List<int>();
			// half the sub-paths in the same direction
			for (i = 1; i < moveX.Count; i++) {
				if (!bruteNeeded) {
					if (moveX [i] == lastx && moveY [i] == lasty) {
						removeIndex.Add (i);
						i++;
					}
				} else {
					// brutal half is needed
					if (i % 2 == 0)
						removeIndex.Add (i);
				}
			}

			if (removeIndex.Count > 0) {
				removeIndex.Reverse ();
				for (i = 0; i < removeIndex.Count; i++) {
					moveX.RemoveAt (removeIndex [i]);
					moveY.RemoveAt (removeIndex [i]);
				}
			} else {
				bruteNeeded = true;
			}
		}

		// evaluation of the circuit: evaluate the position of the toy over each step
		// the circuit always starts in the position (0,0)
		posX.Add (0);
		posY.Add (0);
		lastx = 0;
		lasty = 0;
		int px = 0;
		int py = 0;

		for (i = 0; i < moveX.Count; i++) {
			px = lastx + moveX [i];
			py = lasty + moveY [i];
			if (!PointAlreadyPresent (posX, posY, px, py)) {
				posX.Add (px);
				posY.Add (py);
				lastx = px;
				lasty = py;
			}
		}

		// prints the circuit data in the new file
		CircuitData.data.xList = posX;
		CircuitData.data.yList = posY;
		UnityEngine.Debug.Log ("Number of entries for the circuit: " + posX.Count);

		// Saves the circuit in memory
		CircuitData.data.SaveCircuit();

		GetComponent<SpriteRenderer> ().sprite = ready;
		startButton.GetComponent<ButtonHandler> ().EnableClick ();
	}

	bool PointAlreadyPresent(List<int> xList, List<int> yList, int x, int y) {
		bool found = false;
		for (int i = 0; i < xList.Count && !found; i++) {
			if (xList [i] == x && yList [i] == y) {
				found = true;
			}
		}
		return found;
	}

	public void startGame() {
		UnityEngine.Debug.Log ("Now the game starts");
		startButton.GetComponent<ButtonHandler> ().DisableClick ();
		Application.LoadLevel (GameData.data.sceneToBeLoaded);
	}
}
