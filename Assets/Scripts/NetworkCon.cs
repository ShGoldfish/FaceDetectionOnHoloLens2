using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using UnityEngine;
using System.Linq;
using System.Net;
using System;

/// <summary>
/// ORIGINAL Working code: here Hololens is as the Server. Idealy we want it ro be client
/// </summary> 
public class NetworkCon : MonoBehaviour
{
	// Connection Thread and vars
	Thread mThread_get;
	public int connectionPort = 9005;
	IPEndPoint localEndPoint;
	Socket sender;

	// Shared date over Server and Client connection
	Manager manager;


	private void Start()
	{
		manager = gameObject.GetComponent<Manager>();

		// Connection Thread and vars
		localEndPoint = new IPEndPoint(GetLocalIPAddress(), connectionPort);
		manager.ipEndPoint = localEndPoint.ToString();
		ThreadStart ts_get = new ThreadStart(GetInfo);
		mThread_get = new Thread(ts_get);
		mThread_get.Start();
		//Debug.Log("Shakiba's app trying net connection.");
	}


	public static IPAddress GetLocalIPAddress()
	{
		var host = Dns.GetHostEntry(Dns.GetHostName());
		foreach (var ip in host.AddressList)
		{
			if (ip.AddressFamily == AddressFamily.InterNetwork)
			{
				return ip;
			}
		}
		throw new Exception("No network adapters with an IPv4 address in the system!");
	}


	void GetInfo()
	{
		Socket listenerSoc = new Socket(GetLocalIPAddress().AddressFamily,
				   SocketType.Stream, ProtocolType.Tcp);
		print(localEndPoint);
		listenerSoc.Bind(localEndPoint);
		listenerSoc.Listen(10);
		sender = listenerSoc.Accept();
		// Do not remove recieving var. doesn't make logical sense but should be here
		while (true)
			ConnectionRec();
	}


	void ConnectionRec()
	{
		byte[] buffer = new byte[sender.ReceiveBufferSize];
		byte[] sending = manager.imageBufferBytesArray;
		sender.Send(sending, sending.Length, 0);

		int bytesRead = sender.Receive(buffer);
		string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);
		
		manager.num_faces = 0;
		manager.faces_box = new List<List<int>>();
		if (dataReceived != null && dataReceived != " ")
		{
			List<float> numbers = Array.ConvertAll(dataReceived.Split(','), float.Parse).ToList();
			for (int i = 0; i < numbers.Count; i += 4)
			{
				manager.faces_box.Add(new List<int> { Convert.ToInt32(numbers[i]), Convert.ToInt32(numbers[i + 1]), Convert.ToInt32(numbers[i + 2]), Convert.ToInt32(numbers[i + 3]) });
				manager.num_faces++;
			}
		}
	}


	void OnDestroy()
	{
		sender.Close();
	}
}

