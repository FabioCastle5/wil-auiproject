using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Diagnostics;

public class DataManager : MonoBehaviour {

	public TextAsset measures;

	private List<string> entries;

	private volatile bool finished;
	private const int readingTime = 30;


	void Awake() {
		UnityEngine.Debug.Log ("Starting awake");
		entries = new List<string>();
		finished = false;

		// connect to toy and fill entries with the strings received by it
		ConnectAndReadData ();
		
		// create a new file to save the new cicuit
		string newFile = "circuit-" + System.DateTime.Now.ToString("dd-MM-yyyy") + System.DateTime.Now.ToString("hh-mm-ss") + ".txt";
		string newPath = Application.dataPath + "/Files/" + newFile;
		File.WriteAllText(newPath, String.Empty);
		AssetDatabase.Refresh();

		// elaborate entries and save data onto the new circuit file
		ElaborateRawData (newPath);
		// build the circuit starting from the circuit file created
		BuildCircuit (newPath);
	}


	private void ConnectAndReadData() {
		UnityEngine.Debug.Log ("Starting ConnectAndReadData");

		TcpClient client = new TcpClient();

		Thread timeThread = new Thread (() => {
			Thread.Sleep (readingTime * 1000);
			finished = true;
		});

		client.Connect("ESP8266-WIL", 80);
		StreamReader stream = new StreamReader(client.GetStream());
		timeThread.Start ();
		// buffer hosts the read string until it isn't finished
		var buffer = new List<byte>();
		// (-1|0|1);(0|1)\r - format of the received string
		while (client.Connected && !finished)
		{
			// Read the next byte
			var read = stream.Read();
			// a reading is split with the others by the carriage return, symbol = 13
			if (read == 13)
			{
				// reading is finished, convert our buffer to a string and add it to entries
				string str = Encoding.ASCII.GetString(buffer.ToArray());
				entries.Add (str);
				// Clear the buffer ready for another reading
				buffer.Clear();
			}
			else
				// If this wasn't the end of a reading, then just add this new byte to our buffer
				buffer.Add((byte)read);
		}
	}


	public void ElaborateRawData(string newPath) {

		UnityEngine.Debug.Log ("Starting ElaborateRawData");

		List<int> moveX = new List<int> ();
		List<int> moveY = new List<int> ();
		List<int> posX = new List<int> ();
		List<int> posY = new List<int> ();

		int i = 0;
		int j = 0;

		string outputPath = newPath;

		FileStream outputStream = File.Open (outputPath, FileMode.Open, FileAccess.Write);
		StreamWriter writer = new StreamWriter (outputStream);

		string entry;
		// (-1|0|1);(0|1) - format of the single entry in entries
		for (i = 0; i < entries.Count; i++) {
			entry = entries [i];
			if (entry != null) {
				string[] split = entry.Split (';');
				// split[0] contains (-1|0|1); split[1] contains (0|1)
				moveX.Add (int.Parse (split [0]));
				moveY.Add (int.Parse (split [1]));
			}
		}

		// 1st operation: removes the unsupported change in direction, that are the ones
		// that makes the player going back on its current path(180° curves)
		i = 0;
		j = 0;

		while (i < moveX.Count - 1) {
			if (moveY [i] == 0 && moveX [i] != 0) {
				for (j = i + 1; j < moveX.Count && moveY [j] != 1; j++)
					if (moveX [j] == -moveX [i])
						moveX [j] = 0;
				i = j;
			} else
				i += 1;
		}

		// 2nd operation: removes the (0,0) moves, which are not useful to evaluate the circuit
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
		UnityEngine.Debug.Log ("Number of entries for the circuit: " + posX.Count);
		for (i = 0; i < posX.Count; i++) {
			writer.WriteLine ("x = " + posX[i] + " | " + " y = " + posY[i]);
		}
		writer.Flush();
		AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh ();

		writer.Close ();
		outputStream.Close ();
	}


	public void BuildCircuit (string circuitPath) {

		UnityEngine.Debug.Log ("Started BuildCircuit");

		FileStream inputStream = File.Open (circuitPath, FileMode.Open, FileAccess.Read);
		StreamReader reader = new StreamReader (inputStream);
		List<int> circuitX = new List<int> ();
		List<int> circuitY = new List<int> ();

		string entry = reader.ReadLine ();
		int x, y;
		// the format of an entry is: "x = (int) | y = (int)"
		while (entry != null) {
			if (entry.StartsWith ("x")) {
				entry.Replace (" ", "");
				string[] split = entry.Split ('|');
				string[] splitx = split [0].Split ('=');
				string[] splity = split [1].Split ('=');
				x = int.Parse (splitx [1]);
				y = int.Parse (splity [1]);
				circuitX.Add (x);
				circuitY.Add (y);
			}
			entry = reader.ReadLine ();
		}

		DrawStart (circuitX [0], circuitY [0], circuitX [1], circuitY [1]);

		int i = 0;

		for (; i < circuitX.Count - 1; i++) {
			DrawCurve (circuitX [i - 1], circuitY [i - 1], circuitX [i], circuitY [i], circuitX [i + 1], circuitY [i + 1]);
			DrawStep (circuitX [i], circuitY [i], circuitX [i + 1], circuitY [i + 1]);
		}

		drawFinish (circuitX [i], circuitY [i]);

		reader.Close ();
		inputStream.Close ();
	}

	void DrawStart(int x0, int y0, int x1, int y1) {
	}

	void DrawStep(int x0, int y0, int x1, int y1) {
	}

	void DrawCurve(int x0, int y0, int x1, int y1, int x2, int y2) {
	}

	void drawFinish(int Xf, int yf) {
	}
}
