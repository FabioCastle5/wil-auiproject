using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;

public class DataManager : MonoBehaviour {

	public TextAsset measures;


	void Awake() {
		Debug.Log ("Starting awake");

		string newFile = "circuit-" + System.DateTime.Now.ToString("dd-MM-yyyy") + System.DateTime.Now.ToString("hh-mm-ss") + ".txt";
		string newPath = Application.dataPath + "/Files/" + newFile;
		File.WriteAllText(newPath, String.Empty);
		AssetDatabase.Refresh();

		ElaborateRawData (newPath);
	}


	private int RoundToZero(float value) {
		int i;

		if (value > 0)
			i = (int) Mathf.Floor (value);
		else if (value < 0)
			i = (int) Mathf.Ceil (value);
		else
			i = 0;

		return i;
	
	}


	private float Hypot(float a, float b) {
		return (float) Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
	}


	public void ElaborateRawData(string newPath) {

		Debug.Log ("Starting ElaborateRawData");

		List<float> xList = new List<float> ();
		List<float> yList = new List<float> ();
		List<int> circuitX = new List<int> ();
		List<int> circuitY = new List<int> ();

		string inputPath = AssetDatabase.GetAssetPath (measures);
		string outputPath = newPath;

		FileStream inputStream = File.Open (inputPath, FileMode.Open, FileAccess.Read);
		FileStream outputStream = File.Open (outputPath, FileMode.Open, FileAccess.Write);
		StreamReader reader = new StreamReader (inputStream);
		StreamWriter writer = new StreamWriter (outputStream);

		string entry = reader.ReadLine ();
		float x, y;
		while (entry != null) {
			if (entry.StartsWith ("x")) {
				entry.Replace (" ", "");
				string[] split = entry.Split ('|');
				string[] splitx = split [0].Split ('=');
				string[] splity = split [1].Split ('=');
				x = float.Parse (splitx [1]);
				y = float.Parse (splity [1]);
				xList.Add (x);
				yList.Add (y);
			}
			entry = reader.ReadLine ();
		}


		// 1st operation: Removal of overlapping points
		List<int> removeIndex = new List<int>();

		for (int i = 1, j = 0; i < xList.Count; i++) {
			if (xList [i] == xList [j] &&
				yList [i] == yList [j])
				removeIndex.Add (i);
			else {
				j = i;
			}
		}

		removeIndex.Reverse ();
		for (int i = 0, j = 0; i < removeIndex.Count; i++) {
			j = removeIndex [i];
			xList.RemoveAt (j);
			yList.RemoveAt (j);
		}
		Debug.Log ("Finished operation 1");

		// 2nd operation: removal of the too much high change in direction
		removeIndex = new List<int> ();
		int maxAngle = 90;

		// first direction in between p0 and p1
		float a0 = (Mathf.Atan2 (yList [1] - yList [0], xList [1] - xList [0])) * Mathf.Rad2Deg;
		float a;

		for (int i = 2, j = 1; i < xList.Count; i++) {
			// evaluate the angle respect to the previous point
			a = (Mathf.Atan2 (yList [i] - yList [j], xList [i] - xList [j])) * Mathf.Rad2Deg;
			// if the angle is too high, that point will be deleted
			if (Mathf.Abs (a - a0) > maxAngle)
				removeIndex.Add (i);
			else {
				a0 = a;
				j = i;
			}
		}
		Debug.Log ("Finished operation 2");

		removeIndex.Reverse ();
		for (int i = 0, j = 0; i < removeIndex.Count; i++) {
			j = removeIndex [i];
			xList.RemoveAt (j);
			yList.RemoveAt (j);
		}


		// 3rd operation: Removal of false changes in direction
		removeIndex = new List<int>();

		for (int i = 1; i < xList.Count - 1; i++) {
			if (xList [i - 1] < xList [i + 1] && xList [i] < xList [i - 1] ||
				xList [i - 1] > xList [i + 1] && xList [i] > xList [i - 1] ||
				yList [i - 1] < yList [i + 1] && yList [i] < yList [i - 1] ||
				yList [i - 1] > yList [i + 1] && yList [i] > yList [i - 1])
					removeIndex.Add (i);
		}

		removeIndex.Reverse ();
		for (int i = 0, j = 0; i < removeIndex.Count; i++) {
			j = removeIndex [i];
			xList.RemoveAt (j);
			yList.RemoveAt (j);
		}
		Debug.Log ("Finished operation 3");


		//4th operation: evaluate the mean distance between the points and scale them
		float mean_distance = Hypot(xList[1] - xList[0], yList[1] - yList[0]);
		float distance;

		for (int i = 2; i < xList.Count; i++) {
			distance = Hypot (xList [i] - xList [i - 1], yList [i] - yList [i - 1]);
			mean_distance = (distance + mean_distance * (i - 1)) / i;
		}
		// mean distance must be a scaling factor for the points

		circuitX.Add (RoundToZero(xList [0]));
		circuitY.Add (RoundToZero(yList [0]));
		float module, angle, deltaX, deltaY;
		int scaleModule;

		for (int i = 1, j = 0; i < xList.Count; i++) {
			module = Hypot (xList[i] - xList[j], yList[i] - yList[j]);
			angle = Mathf.Atan2 (yList[i] - yList[j], xList[i] - xList[j]) * Mathf.Rad2Deg;
			scaleModule = RoundToZero (module / mean_distance);
			if (scaleModule != 0) {
				if (Mathf.Abs (angle) == 90.0f)
					deltaX = 0.0f;
				else
					deltaX = scaleModule * Mathf.Cos (angle * Mathf.Deg2Rad);
				if (angle == 0.0f || Mathf.Abs (angle) == 180.0f)
					deltaY = 0.0f;
				else
					deltaY = scaleModule * Mathf.Sin (angle * Mathf.Deg2Rad);
				circuitX.Add (RoundToZero(xList [j] + deltaX));
				circuitY.Add (RoundToZero(yList [j] + deltaY));
				j = i;
			}
		}
		Debug.Log ("Finished operation 4");

		Debug.Log ("Number of entries for the circuit: " + circuitX.Count);
		// In the end, prints the results in the circuit file
		for (int i = 0; i < circuitX.Count; i++) {
			writer.WriteLine ("x = " + circuitX[i] + "|" + " y= " + circuitY[i]);
		}
		writer.Flush();
		AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh ();

		writer.Close ();
		reader.Close ();
		outputStream.Close ();
		inputStream.Close ();
	}

	public void BuildCircuit (string circuitPath) {

		Debug.Log ("Started building circuit");

		FileStream inputStream = File.Open (circuitPath, FileMode.Open, FileAccess.Read);
		StreamReader reader = new StreamReader (inputStream);
		List<int> circuitX = new List<int> ();
		List<int> circuitY = new List<int> ();

		string entry = reader.ReadLine ();
		int x, y;
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

		int angle;

		//evaluate the angle between the starting point and the next one
		angle = RoundToZero((Mathf.Atan2 (circuitY [1] - circuitY [0], circuitX [1] - circuitX [0])) * Mathf.Rad2Deg);
		// draw the starting tile
		drawStart (angle);

		for (int i = 1; i < circuitX.Count - 1; i++) {
			// evaluate the angle respect to the previous point
			angle = RoundToZero((Mathf.Atan2 (circuitY [i] - circuitY [i + 1], circuitX [i] - circuitX [i + 1])) * Mathf.Rad2Deg);
			// draw another tile
			drawStep (angle);
		}

		// note: angle contains the angle between the last point and the previous one
		// draw the finish tile
		drawFinish(angle);

		reader.Close ();
		inputStream.Close ();
	}

	void drawStart(int angle) {
	}

	void drawStep(int angle) {
	}

	void drawFinish(int angle) {
	}
}
