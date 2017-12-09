using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Diagnostics;

public class MeasureManager : MonoBehaviour {

	public volatile bool finished;
	public List<string> entries;

	Thread clientThread;

	void Start () {
		finished = false;
		entries = new List<string> ();
		clientThread = new Thread (ConnectAndRead);
		clientThread.IsBackground = true;
	}

	// called by the main thread to start the measure communication
	public void ConnectAndReadData() {

		UnityEngine.Debug.Log ("Setting up the measure thread");
		clientThread.Start ();
	}

	void ConnectAndRead () {
		TcpClient client = new TcpClient ();

		UnityEngine.Debug.Log ("Connecting to the toy...");
		// start a connection with the toy
		IAsyncResult result = client.BeginConnect("192.168.4.1", 80, null, null);
		// set the timeout after 1 second
		bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
		// while you can't connect to the toy try again after 1 second
		while (!success && !finished) {
			success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));	
		}

		if (!finished) {
			// here the connection has been enstabilished iif countdown hasn't finished yet
			UnityEngine.Debug.Log ("Connected to the toy!");
			StreamReader stream = new StreamReader (client.GetStream ());

			// buffer hosts the read string until it isn't finished
			var buffer = new List<byte> ();

			// (-1|0|1);(0|1)\r - format of the received string
			while (client.Connected && !finished) {
				// Read the next byte
				var read = stream.Read ();
				// a reading is split with the others by the carriage return, symbol = 13
				if (read == 13) {
					// reading is finished, convert our buffer to a string and add it to entries
					string str = Encoding.ASCII.GetString (buffer.ToArray ());
					entries.Add (str);
					// Clear the buffer ready for another reading
					buffer.Clear ();
				} else {
					// If this wasn't the end of a reading, then just add this new byte to our buffer
					buffer.Add ((byte)read);
				}
			}
			stream.Close ();
		}

		client.EndConnect (result);
	}

	// called by the main thread to stop it
	public void SetFinished (bool value) {
		finished = value;
	}

	public List<string> GetEntries () {
		if (clientThread.IsAlive) {
			clientThread.Abort ();
		}
		return entries;
	}

	void OnDestroy() {
		if (clientThread.IsAlive)
			clientThread.Abort ();
	}
}
