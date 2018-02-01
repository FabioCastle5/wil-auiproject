using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Diagnostics;

public class InputController : MonoBehaviour {

	public float filterFactor;

	private Thread inputThread;
	private TcpClient client;
	private string IP = "192.168.4.1";
	private int port = 80;
	private IAsyncResult result;
	private StreamReader stream;
//	private LowPassFilter filterX;
//	private LowPassFilter filterY;

	void Start() {
//		filterX = new LowPassFilter (filterFactor);
//		filterY = new LowPassFilter (filterFactor);
		client = new TcpClient ();
		inputThread = new Thread (ConnectAndRead);
	}


	public void Activate () {
		inputThread.Start ();
	}


	void ConnectAndRead () {
		UnityEngine.Debug.Log ("Connecting to the toy...");
		// start a connection with the toy
		result = client.BeginConnect(IP, port, null, null);
		// set the timeout after 1 second
		bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
		// while you can't connect to the toy try again after 1 second
		while (!success) {
			success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
		}

		// here the connection has been enstabilished
		UnityEngine.Debug.Log ("Connected to the toy!");
		GameDataHandler.dataHandler.connected = true;
		stream = new StreamReader (client.GetStream ());

		// buffer hosts the read string until it isn't finished
		List<byte> buffer = new List<byte> ();

		// ax(f);ay(f)\r - format of the received string
		while (client.Connected) {
			// Read the next byte
			var read = stream.Read ();
			// a reading is split with the others by the carriage return, symbol = 13
			if (read == 13) {
				// reading is finished, convert our buffer to a string and add it to entries
				string entry = Encoding.ASCII.GetString (buffer.ToArray ());
				string[] split = entry.Split (';');
				// split[0] contains ax; split[1] contains ay
				UnityEngine.Debug.Log("x read: " + split[0]);
				UnityEngine.Debug.Log ("y read: " + split [1]);
				var ax = float.Parse (split [0]);
				var ay = float.Parse (split [1]);
				GameDataHandler.dataHandler.actualAccX = ax;
				GameDataHandler.dataHandler.actualAccY = ay;
				// Clear the buffer ready for another reading
				buffer.Clear ();
			} else {
				// If this wasn't the end of a reading, then just add this new byte to our buffer
				buffer.Add ((byte)read);
			}
		}
		// end connection
		stream.Close ();
		client.EndConnect (result);
		GameDataHandler.dataHandler.connected = false;
	}

	void OnApplicationQuit() {
		if (result.IsCompleted) {
			//end connection and abort
			client.EndConnect (result);
			inputThread.Abort ();
		}
	}
}
