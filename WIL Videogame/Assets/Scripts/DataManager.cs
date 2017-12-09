using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Diagnostics;

public class DataManager : MonoBehaviour {

	public GameObject[] tiles;
	public GameObject[] flags;

	public GameObject circuit;
	public GameObject circuitInstance;

	private const float offsetUp45 = 3.47f;
	private const int verticalOffset = 12;
	private const float horizontalOffset = 19.19f;


	public void BuildCircuit () {

		UnityEngine.Debug.Log ("Started BuildCircuit");

		// if the circuit is not that good, draw a default one
		if (CircuitData.data.BadCircuit ()) {
			UnityEngine.Debug.Log("Bad circuit: changing it");
			CircuitData.data.xList.Clear ();
			CircuitData.data.yList.Clear ();
			for (int j = 0; j < 5; j++) {
				CircuitData.data.xList.Add (j);
				CircuitData.data.yList.Add (0);
			}
		}

		List<int> circuitX = CircuitData.data.xList;
		List<int> circuitY = CircuitData.data.yList;
		circuitInstance = Instantiate (circuit, Vector3.zero, Quaternion.identity) as GameObject;
		circuitInstance.GetComponent<CircuitManager> ().Reset ();

		float x = 0f;
		float y = 0f;
		DrawStart (circuitX [0], circuitY [0], circuitX [1], circuitY [1], ref x, ref y);

		int i = 1;
		for (; i < circuitX.Count - 1; i++) {
			DrawCurve (circuitX [i - 1], circuitY [i - 1], circuitX [i], circuitY [i], circuitX [i + 1], circuitY [i + 1], ref x, ref y);
			if (i != circuitX.Count - 2)
				DrawStep (circuitX [i], circuitY [i], circuitX [i + 1], circuitY [i + 1], ref x, ref y);
		}

		drawFinish (circuitX [i - 1], circuitY [i - 1], circuitX [i], circuitY [i], x, y);
	}


	/* tile order:
	 * number 0 and 1 are the straight way vertical and horizontal
	 * number 2 and 3 are the 45° turned way right and left
	 * number 4 to 7 are the 90° curves: NE, NW, EN, WN
	 * 
	 * tiles' origin point is the center of the tile itself
	 * 
	 * as y grows only in the positive axes, the downway directions are not possible
	*/
	void DrawStart(int x0, int y0, int x1, int y1, ref float x, ref float y) {
		DrawStep (x0, y0, x1, y1, ref x, ref y);
		int direction = 0;
		if (y0 == y1) {
			if (x0 < x1)
				direction = 0;
			else
				direction = 180;
		} else
			direction = 90;
		int flagIndex = 0;
		if (direction != 90)
			flagIndex = 1;
		GameObject flag = Instantiate (flags[flagIndex], Vector3.zero, flags[flagIndex].transform.rotation) as GameObject;
		flag.GetComponent<BoxCollider2D> ().enabled = false;
		circuitInstance.GetComponent<CircuitManager> ().SetInitialDirection (direction);
	}

	void DrawStep(int x0, int y0, int x1, int y1, ref float x, ref float y) {
		// choose the tile to be instantiated
		int chosenTileIndex = 0;
		bool up = false;
		bool right = false;
		if (x0 == x1) {
			chosenTileIndex = 0;
			if (y1 > y0)
				up = true;
		} else if (y0 == y1) {
			chosenTileIndex = 1;
			if (x1 > x0)
				right = true;
		} else if (x1 > x0 && y1 > y0) {
			chosenTileIndex = 2;
			x += offsetUp45;
			right = true;
		} else if (x1 < x0 && y1 > y0) {
			chosenTileIndex = 3;
			x -= offsetUp45;
		} else if (x1 > x0 && y1 < y0) {
			chosenTileIndex = 3;
			x += offsetUp45;
			right = true;
		} else {
			chosenTileIndex = 2;
			x -= offsetUp45;
		}
		// add it to circuit manager
		circuitInstance.GetComponent<CircuitManager> ().AddTile (x, y, tiles[chosenTileIndex]);
		// update position for the next tile to be instantiated
		if (chosenTileIndex == 0) {
			if (up)
				y += verticalOffset;
			else
				y -= verticalOffset;
		}
		else if (chosenTileIndex == 1) {
			if (right)
				x += horizontalOffset;
			else
				x -= horizontalOffset;
		} else if (chosenTileIndex == 2) {
			y += verticalOffset;
			if (right)
				x += offsetUp45;
			else
				x -= offsetUp45;
		} else {
			y += verticalOffset;
			if (right)
				x += offsetUp45;
			else
				x -= offsetUp45;
		}
	}

	void DrawCurve(int x0, int y0, int x1, int y1, int x2, int y2, ref float x, ref float y) {
		// choose wether a curve is needed or not
		if (y2 == y0 && y0 != y1) {
			// two curves are needed: it's a going-back
			if (y1 > y0) {
				if (x0 > x2) {
					circuitInstance.GetComponent<CircuitManager> ().AddTile (x, y, tiles [5]);
					x -= horizontalOffset;
					circuitInstance.GetComponent<CircuitManager> ().AddTile (x, y, tiles [4]);
					y -= verticalOffset;
				} else if (x0 < x2) {
					circuitInstance.GetComponent<CircuitManager> ().AddTile (x, y, tiles [4]);
					x += horizontalOffset;
					circuitInstance.GetComponent<CircuitManager> ().AddTile (x, y, tiles [5]);
					y -= verticalOffset;
				}
			} else if (y1 < y0) {
				if (x0 > x2) {
					circuitInstance.GetComponent<CircuitManager> ().AddTile (x, y, tiles [6]);
					x -= horizontalOffset;
					circuitInstance.GetComponent<CircuitManager> ().AddTile (x, y, tiles [7]);
					y += verticalOffset;
				} else if (x0 < x2) {
					circuitInstance.GetComponent<CircuitManager> ().AddTile (x, y, tiles [7]);
					x += horizontalOffset;
					circuitInstance.GetComponent<CircuitManager> ().AddTile (x, y, tiles [6]);
					y += verticalOffset;
				}
			}
		}
		if (y2 == y0 + 1 || y2 == y0 - 1) {
			// one curve is needed: it's a change in direction
			if (y2 > y1) {
				if (x0 < x1) {
					// curve EN
					circuitInstance.GetComponent<CircuitManager> ().AddTile (x, y, tiles[6]);
				} else {
					// curve WN
					circuitInstance.GetComponent<CircuitManager> ().AddTile (x, y, tiles[7]);
				}
				y += verticalOffset;
			} else if (y2 < y1) {
				if (x0 < x1) {
					// curve NW
					circuitInstance.GetComponent<CircuitManager> ().AddTile (x, y, tiles[5]);
				} else {
					// curve NE
					circuitInstance.GetComponent<CircuitManager> ().AddTile (x, y, tiles[4]);
				}
				y -= verticalOffset;
			} else { // y1 == y2
				if (x2 < x1) {
					if (y1 > y0) {
						// curve NW
						circuitInstance.GetComponent<CircuitManager> ().AddTile (x, y, tiles[5]);
					} else {
						// curve EN
						circuitInstance.GetComponent<CircuitManager> ().AddTile (x, y, tiles[6]);
					}
					x -= horizontalOffset;
				} else { // x2 > x1
					if (y1 > y0) {
						// curve NE
						circuitInstance.GetComponent<CircuitManager> ().AddTile (x, y, tiles[4]);
					} else {
						// curve WN
						circuitInstance.GetComponent<CircuitManager> ().AddTile (x, y, tiles[7]);
					}
					x += horizontalOffset;
				}
			}
		}
	}

	void drawFinish(int x0, int y0, int x1, int y1, float x, float y) {
		DrawStep (x0, y0, x1, y1, ref x, ref y);
		int flagIndex = 0;
		if (y0 == y1) {
			flagIndex = 1;
		}
		circuitInstance.GetComponent<CircuitManager> ().DrawFinalFlag (flags [flagIndex]);
	}
}
