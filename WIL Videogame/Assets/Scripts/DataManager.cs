using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class DataManager : MonoBehaviour {

	public TextAsset measures;
	public TextAsset circuitPoints;


	void Awake() {
		Debug.Log ("Starting awake");
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


	private float Hypot(int a, int b) {
		return (float) Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
	}


	public void ElaborateRawData() {

		Debug.Log ("Starting ElaborateRawData");

		List<float> xList = new List<float> ();
		List<float> yList = new List<float> ();
		List<int> circuitX = new List<int> ();
		List<int> circuitY = new List<int> ();

		string inputPath = AssetDatabase.GetAssetPath (measures);
		string outputPath = AssetDatabase.GetAssetPath (circuitPoints);

		FileStream inputStream = File.Open (inputPath, FileMode.Open, FileAccess.Read);
		FileStream outputStream = File.Open (outputPath, FileMode.Open, FileAccess.Write);
		StreamReader reader = new StreamReader (inputStream);
		StreamWriter writer = new StreamWriter (outputStream);
		
//		string pattern = @"[-+]?[.]?[\d]+(?:,\d\d\d)*[\.]?\d*(?:[eE][-+]?\d+)?";
//
//		// Instantiate the regular expression object.
//		Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
//
//		string entry = reader.ReadLine ();
//		while (entry.Length > 0) {
//			if (entry.StartsWith ("x")) {
//				// Match the regular expression pattern against a text string.
//				Match m = r.Match (entry);
//				for (int i = 0; i < m.Groups.Count; i++) {
//					CaptureCollection xy = m.Groups [i].Captures;
//					// xy[0] = x; xy[1] = y
//					xList.Add (float.Parse(xy [0].ToString));
//					yList.Add (float.Parse(xy [1].ToString));
//				}
//			}
//		}

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

		for (int i = 0; i < xList.Count; i++) {
			int intX = RoundToZero (xList [i]);
			circuitX.Add (intX);
			int intY = RoundToZero (yList [i]);
			circuitY.Add (intY);
		}


		// 1st operation: Removal of overlapping points
		List<int> removeIndex = new List<int>();

		for (int i = 1, j = 0; i < circuitX.Count; i++) {
			if (circuitX [i] == circuitX [j] &&
				circuitY [i] == circuitY [j])
				removeIndex.Add (i);
			else {
				j = i;
			}
		}

		removeIndex.Reverse ();
		for (int i = 0, j = 0; i < removeIndex.Count; i++) {
			j = removeIndex [i];
			circuitX.RemoveAt (j);
			circuitY.RemoveAt (j);
		}


		// 2nd operation: removal of the too much high change in direction
		removeIndex = new List<int> ();
		int maxAngle = 90;

		// first direction in between p0 and p1
		float a0 = (Mathf.PI + Mathf.Atan2 (circuitY [1] - circuitY [0], circuitX [1] - circuitX [0])) * Mathf.Rad2Deg;
		float a;

		for (int i = 2, j = 1; i < circuitX.Count; i++) {
			// evaluate the angle respect to the previous point
			a = (Mathf.PI + Mathf.Atan2 (circuitY [i] - circuitY [j], circuitX [i] - circuitX [j])) * Mathf.Rad2Deg;
			// if the angle is too high, that point will be deleted
			if (Mathf.Abs (a - a0) > maxAngle)
				removeIndex.Add (i);
			else {
				a0 = a;
				j = i;
			}
		}

		removeIndex.Reverse ();
		for (int i = 0, j = 0; i < removeIndex.Count; i++) {
			j = removeIndex [i];
			circuitX.RemoveAt (j);
			circuitY.RemoveAt (j);
		}


		// 3rd operation: Removal of false changes in direction
		removeIndex = new List<int>();

		for (int i = 1; i < circuitX.Count - 1; i++) {
			if (circuitX [i - 1] < circuitX [i + 1] && circuitX [i] < circuitX [i - 1] ||
			    circuitX [i - 1] > circuitX [i + 1] && circuitX [i] > circuitX [i - 1] ||
			    circuitY [i - 1] < circuitY [i + 1] && circuitY [i] < circuitY [i - 1] ||
			    circuitY [i - 1] > circuitY [i + 1] && circuitY [i] > circuitY [i - 1])
					removeIndex.Add (i);
		}

		removeIndex.Reverse ();
		for (int i = 0, j = 0; i < removeIndex.Count; i++) {
			j = removeIndex [i];
			circuitX.RemoveAt (j);
			circuitY.RemoveAt (j);
		}


		//4th operation: evaluate the mean distance between the points
		float mean_distance = Hypot(circuitX[1] - circuitX[0], circuitY[1] - circuitY[0]);
		float distance;

		for (int i = 2; i < circuitX.Count; i++) {
			distance = Hypot (circuitX [i] - circuitX [i - 1], circuitY [i] - circuitY [i - 1]);
			mean_distance = (distance + mean_distance * (i - 1)) / i;
		}
		// mean distance must be a scaling factor for the tiles


		// In the end, prints the results in the circuit file
		for (int i = 0; i < circuitX.Count; i++) {
			writer.WriteLine (i + " x:" + circuitX[i] + " y:" + circuitY[i]);
		}
		writer.Flush();
		AssetDatabase.Refresh ();
	}
}
