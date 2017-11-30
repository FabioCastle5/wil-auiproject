using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;

public class DataManager : MonoBehaviour {

	public TextAsset measures;
	public TextAsset circuitPoints;


	void Awake() {
		Debug.Log ("Starting awake");
		ElaborateRawData ();
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


	public void ElaborateRawData() {

		Debug.Log ("Starting ElaborateRawData");

		List<float> xList = new List<float> ();
		List<float> yList = new List<float> ();
		List<float> circuitX = new List<float> ();
		List<float> circuitY = new List<float> ();

		string inputPath = AssetDatabase.GetAssetPath (measures);
		string outputPath = AssetDatabase.GetAssetPath (circuitPoints);

		FileStream inputStream = File.Open (inputPath, FileMode.Open, FileAccess.Read);
		File.WriteAllText(outputPath, String.Empty);
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

		circuitX.Add (xList [0]);
		circuitY.Add (yList [0]);
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
				circuitX.Add (xList [j] + deltaX);
				circuitY.Add (yList [j] + deltaY);
				j = i;
			}
		}
		Debug.Log ("Finished operation 4");

		Debug.Log ("Number of entries for the circuit: " + circuitX.Count);
		// In the end, prints the results in the circuit file
		for (int i = 0; i < circuitX.Count; i++) {
			writer.WriteLine (i + " x:" + circuitX[i] + " y:" + circuitY[i]);
		}
		writer.Flush();
		AssetDatabase.Refresh ();
	}
}
