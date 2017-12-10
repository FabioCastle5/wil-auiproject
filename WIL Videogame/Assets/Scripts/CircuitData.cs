using UnityEngine;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class CircuitData : MonoBehaviour {

	public static CircuitData data;

	public string circuitName;

	public List<int> xList;
	public List<int> yList;


	// Singleton and data accessible from everywhere
	void Awake () {
		if (data == null) {
			DontDestroyOnLoad (gameObject);
			data = this;
			xList = new List<int> ();
			yList = new List<int> ();
			circuitName = String.Empty;
		} else if (data != this) {
			Destroy (gameObject);
		}
	}


	public bool BadCircuit() {
		Debug.Log ("Entered bad circuit");
		bool answer = false;;
		if (xList.Count < 2)
			answer = true;
		return answer;
	}


	public void SaveCircuit() {
		if (circuitName != String.Empty) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Create(Application.persistentDataPath + "/" + circuitName + ".dat");

			Circuit c = new Circuit ();
			c.xList = xList;
			c.yList = yList;

			bf.Serialize (file, c);
			file.Close ();
		}
	}


	public void LoadCircuit(string fileName) {
		string name = Application.persistentDataPath + "/" + fileName + ".dat";
		if (File.Exists (name)) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (name, FileMode.Open);

			Circuit circuit = (Circuit)bf.Deserialize (file);
			file.Close ();

			xList = circuit.xList;
			yList = circuit.yList;
			circuitName = fileName;
		}
	}
}

[Serializable]
class Circuit {
	public List<int> xList;
	public List<int> yList;
}