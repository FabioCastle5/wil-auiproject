using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ViewCircuitManager : MonoBehaviour {

	private List<ViewTileData> tiles;
	private int nextTile;

	private const float offsetUp45 = 3.47f;
	private const int verticalOffset = 12;
	private const float horizontalOffset = 19.19f;

	private float minX;
	private float maxX;
	private float minY;
	private float maxY;

	public void AddTile (float x, float y, GameObject tile) {
		if (tiles == null)
			tiles = new List<ViewTileData> ();
		ViewTileData newTile = new ViewTileData();
		newTile.x = x;
		newTile.y = y;
		newTile.tileObject = tile;
		tiles.Add (newTile);
	}

	public void Reset () {
		tiles = new List<ViewTileData>();
		nextTile = 0;
	}

	public void DrawFinalFlag (GameObject flag) {
		int i = 0;
		while (i < tiles.Count - 1)
			i++;
		float x = tiles[i].x;
		float y = tiles[i].y;
		GameObject instance = Instantiate (flag, new Vector3(x,y,0f), flag.transform.rotation) as GameObject;
		instance.GetComponent<BoxCollider2D> ().enabled = true;
	}

	public void StartDrawing () {
		for (; nextTile < tiles.Count; nextTile++) {
			ViewTileData newTileData = tiles [nextTile];
			Instantiate (newTileData.tileObject, new Vector3 (newTileData.x, newTileData.y, 0f), Quaternion.identity);
		}
	}

	public void EvaluateCameraDimensions(ref float left, ref float top, ref float height, ref float width) {
		minX = 0f;
		maxX = 0f;
		minY = 0f;
		maxY = 0f;
		float newx = 0f;
		float newy = 0f;

		for (int i = 0; i < tiles.Count; i++) {
			newx = tiles [i].x;
			newy = tiles [i].y;

			if (newx > maxX)
				maxX = newx;
			else if (newx < minX)
				minX = newx;

			if (newy > maxY)
				maxY = newy;
			else if (newy < minY)
				minY = newy;
		}
		left = minX- horizontalOffset;
		top = maxY + verticalOffset;
		height = maxY - minY + 2 * verticalOffset;
		width = maxX - minX + 2 * horizontalOffset;
	}
}

class ViewTileData {
	public float x, y;
	public GameObject tileObject;
}