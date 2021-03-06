﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ViewCircuitLoader : MonoBehaviour {

	public GameObject circuitBuilder;
	public Camera main_camera;

	// Use this for initialization
	void Start () {

		// loads the circuit
		circuitBuilder.GetComponent<ViewDataManager> ().BuildCircuit ();

		GameObject circuit = circuitBuilder.GetComponent<ViewDataManager> ().circuitInstance;
		circuit.GetComponent<ViewCircuitManager> ().StartDrawing ();

		GameData.data.circuitMananger = circuit;

		float height = 0f;
		float width = 0f;
		float left = 0f;
		float top = 0f;
		circuit.GetComponent<ViewCircuitManager> ().EvaluateCameraDimensions (ref left, ref top, ref height, ref width);

		float cameraX = left + (width / 2);
		float cameraY = top - (height / 2);
		Vector3 cameraPosition = new Vector3 (cameraX, cameraY, -10f);

		main_camera.orthographic = true;
		main_camera.orthographicSize = Mathf.Max(height / 2, width / 2);
		main_camera.transform.position = cameraPosition;
	}

	public void RestartGame() {
		Application.LoadLevel (0);
	}
}
