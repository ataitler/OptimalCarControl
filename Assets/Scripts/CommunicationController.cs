using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;

public class CommunicationController : MonoBehaviour {

	private Socket connection;
	private bool connected;
	public int port;

	public CarController carController;
	public GameController gameManager;
	public RewardManager rewardManager;

	// Use this for initialization
	void Start () {
		// init socker parameters
		connected = false;
		IPAddress IpAddress = IPAddress.Loopback;
		IPEndPoint localendPoint = new IPEndPoint (IpAddress, port);

		// create a TCP socket
		Socket listener = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		// bind the socket to the end point and listen for incoming connections
		try {
			listener.Bind(localendPoint);
			listener.Listen(100);
			listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
		}
		catch (Exception) {
			Debug.Log("Couldn't setup the TCP listner");
		}
	}

	void FixedUpdate () {
		if (connection != null && !IsConnected(connection) && connected) {
			gameManager.UpdateState(TrainerState.Disconnected);
			connected = false;
		}
	}

	private void AcceptCallback(IAsyncResult ar) {
		// Get the socket that handles the client request.
		Socket listener = (Socket)ar.AsyncState;
		//Socket handler = listener.EndAccept(ar);
		connection = listener.EndAccept(ar);
		connected = true;
		
		try {
			// Create the state object.
			StateObject state = new StateObject();
			//state.workSocket = handler;
			state.workSocket = connection;
			//handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
			connection.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
			
			// notify game manager an agents has connected
			gameManager.UpdateState (TrainerState.Idle);
		}
		catch {
			Debug.Log("Error listenning to incoming connection");
		}
	}

	private void ReadCallback(IAsyncResult ar) {
		// Retrieve the state object and the handler socket
		// from the asynchronous state object.
		StateObject state = (StateObject)ar.AsyncState;
		Socket handler = state.workSocket;
		
		if (handler.Connected) {
			// Read data from the client socket. 
			int bytesRead = handler.EndReceive (ar);
			string msg = Encoding.ASCII.GetString(state.buffer,0,bytesRead);
			
			// send action to agent
			string pattern = @"(\-?\d+\.?\d*)";
			Match m = Regex.Match(msg, pattern);
			if (m.Success) {
				float action = float.Parse(m.Groups[1].Value);
				carController.SetAction(action);
			}
			// Continue listenning
			handler.BeginReceive (state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback (ReadCallback), state);
		}
	}

	public void Send(Socket handler, String data) {
		// Convert the string data to byte data using ASCII encoding.
		byte[] byteData = Encoding.ASCII.GetBytes(data);
		
		try {
			// Begin sending the data to the remote device.
			handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
		}
		catch {
			Debug.Log("Dropped connection");
		}
	}

	public void Send(String data) {
		// Convert the string data to byte data using ASCII encoding.
		byte[] byteData = Encoding.ASCII.GetBytes(data);
		
		try {
			// Begin sending the data to the remote device.
			this.connection.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), this.connection);
		}
		catch {
			Debug.Log("Dropped connection in Send");
		}
	}

	private void SendCallback(IAsyncResult ar) {
		try {
			// Retrieve the socket from the state object.
			Socket handler = (Socket)ar.AsyncState;
			
			if (handler.Connected) {
				// Complete sending the data to the remote device.
				int bytesSent = handler.EndSend(ar);
				Debug.Log("Sent " + bytesSent.ToString() + " bytes to client.");
			}
		}
		catch (Exception e) {
			Debug.Log(e.ToString());
		}
	}

	/// <summary>
	/// Shuts down.
	/// </summary>
	/// <param name="onlyListen">If set to <c>true</c> only listen.</param>
	public void ShutDown(bool onlyListen) {
		try {
			if (onlyListen == false) {
				connection.Shutdown (SocketShutdown.Both);
				connection.Close ();
			}
		}
		catch {
			Debug.Log("Error shutting down connection on the server side");
		}
		
		// relisten for incomming connections
		IPAddress IpAddress = IPAddress.Loopback;
		IPEndPoint localendPoint = new IPEndPoint (IpAddress, port);
		connection = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		try {
			connection.Bind(localendPoint);
			connection.Listen(100);
			connection.BeginAccept(new AsyncCallback(AcceptCallback), connection);
		}
		catch (Exception) {
			Debug.Log("Couldn't setup the TCP listner");
		}
	}

	/// <summary>
	/// Determines whether this instance is connected the specified socket.
	/// </summary>
	/// <returns><c>true</c> if this instance is connected the specified socket; otherwise, <c>false</c>.</returns>
	/// <param name="socket">Socket.</param>
	private bool IsConnected(Socket socket) {
		try {
			//return !(socket.Poll(1,SelectMode.SelectRead) && socket.Available == 0);
			return socket.Connected;
		}
		catch (SocketException) {
			return false;
		}
	}

}
