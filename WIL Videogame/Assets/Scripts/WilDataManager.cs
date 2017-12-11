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
	public int maxPoints;

	private List<string> entries;

	private const int readingTime = 30;

	public Sprite countdown;
	public Sprite countdownEnd;
	public Sprite building;
	public Sprite ready;


	void Start () {
		UnityEngine.Debug.Log ("The Data Reveiver has started");
		entries = new List<string>();

		startButton.GetComponent<ButtonHandler> ().DisableClick ();
		GameData.data.GUIManager = gameObject;
	}

	public void startReadings() {
		UnityEngine.Debug.Log ("Start readings");
		createButton.GetComponent<ButtonHandler> ().DisableClick ();
		GetComponent<SpriteRenderer> ().sprite = countdown;
		clientHandler.GetComponent<MeasureManager> ().ConnectAndReadData ();
		countdownManager.GetComponent<CountdownManager> ().StartTimer();
	}

	public void EndReadings() {
		UnityEngine.Debug.Log ("Countdown finished");
		clientHandler.GetComponent<MeasureManager> ().SetFinished (true);
		GetComponent<SpriteRenderer> ().sprite = countdownEnd;
		entries = clientHandler.GetComponent<MeasureManager> ().GetEntries ();
		countdownManager.GetComponent<CountdownManager> ().StartSecondTimer ();
	}



	public void ElaborateEntries() {

		// create a new file to save the new cicuit
		string newFile = "circuit-" + System.DateTime.Now.ToString("dd-MM-yyyy") + "-" + System.DateTime.Now.ToString("hh-mm-ss");
		CircuitData.data.circuitName = newFile;

		GetComponent<SpriteRenderer> ().sprite = building;

		List<int> moveX = new List<int> ();
		List<int> moveY = new List<int> ();
		List<int> posX = new List<int> ();
		List<int> posY = new List<int> ();

		int i = 0;
		int j = 0;

		string entry;
		// (-1|0|1);(0|1) - format of the single entry in entries
		for (i = 0; i < entries.Count; i++) {
			entry = entries [i];
			if (entry != null) {
				string[] split = entry.Split (';');
				// split[0] contains (-1|0|1); split[1] contains (0|1)
				UnityEngine.Debug.Log("x read: " + split[0]);
				UnityEngine.Debug.Log ("y read: " + split [1]);
				moveX.Add (int.Parse (split [0]));
				moveY.Add (int.Parse (split [1]));
			}
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
		removeIndex = new List<int>();
		i = 0;
		j = 0;

		while (i < moveX.Count - 1) {
			if (moveY [i] == 0 && moveX [i] != 0) {
				for (j = i + 1; j < moveX.Count && moveY [j] == 0; j++)
					if (moveX [j] == -moveX [i]) {
						//moveX [j] = 0; <- this introduces 0,0 move
						removeIndex.Add(j);
					}
				i = j;
			} else if (moveX [i] == 0 && moveY [i] != 0) {
				for (j = i + 1; j < moveX.Count && moveX [j] == 0; j++)
					if (moveY [j] == -moveY [i]) {
						//moveY [j] = 0; <- this introduces 0,0 move
						removeIndex.Add(j);
					}
				i = j;
			} else
				i += 1;
		}

		removeIndex.Reverse ();
		for (i = 0; i < removeIndex.Count; i++) {
			moveX.RemoveAt (removeIndex [i]);
			moveY.RemoveAt (removeIndex [i]);
		}

		maxPoints = GameData.data.maxNumberOfTiles;

		while (moveX.Count > maxPoints + 2) {
			removeIndex = new List<int>();

			for (i = 0; i < moveX.Count; i++)
				if (i % 2 == 0)
					removeIndex.Add (i);

			removeIndex.Reverse ();
			for (i = 0; i < removeIndex.Count; i++) {
				moveX.RemoveAt (removeIndex [i]);
				moveY.RemoveAt (removeIndex [i]);
			}
		}

		// evaluation of the circuit: evaluate the position of the toy over each step
		// the circuit always starts in the position (0,0)
		posX.Add (0);
		posY.Add (0);
		int lastx = 0;
		int lasty = 0;
		int px = 0;
		int py = 0;

		for (i = 0; i < moveX.Count; i++) {
			px = lastx + moveX [i];
			py = lasty + moveY [i];
			posX.Add (px);
			posY.Add (py);
			lastx = px;
			lasty = py;
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

	public void startGame() {
		UnityEngine.Debug.Log ("Now the game starts");
		startButton.GetComponent<ButtonHandler> ().DisableClick ();
		Application.LoadLevel (GameData.data.sceneToBeLoaded);
	}
}
