using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using System;
using System.Linq;

public class NetworkCon : MonoBehaviour
{
	// Connection Thread and vars
	Thread mThread_get;
	bool recieving;
	public int connectionPort = 9005;
	public string ipEndPoint;
	IPEndPoint localEndPoint;
	Socket sender;

	// Shared date over Server and Client connection
	Manager manager;


	private void Start()
	{
		manager = gameObject.GetComponent<Manager>();

		// Connection Thread and vars
		localEndPoint = new IPEndPoint(GetLocalIPAddress(), connectionPort);
		ipEndPoint = localEndPoint.ToString();
		ThreadStart ts_get = new ThreadStart(GetInfo);
		mThread_get = new Thread(ts_get);
		mThread_get.Start();
		recieving = false;
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
		recieving = true;
		while (recieving)
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

/*
public class NetworkCon : MonoBehaviour
{
	public int connectionPort = 9005;
	//public string connectionHost = "192.168.0.17";
	public string connectionHost = "192.168.0.5";
	public IPAddress address;
	private TcpClient socketConnection;
	private Thread clientReceiveThread;

	public byte[] imageBufferBytesArray;
	ContextDetection contextDetection;

	private void Start()
	{
		contextDetection = gameObject.GetComponent<ContextDetection>();
		Parse(connectionHost);
		print("Start: " + address);
		ConnectToTcpServer();
		
	}

	private void Parse(string ipAddress)
	{
		try
		{
			// Create an instance of IPAddress for the specified address string (in
			// dotted-quad, or colon-hexadecimal notation).
			address = IPAddress.Parse(ipAddress);

			// Display the address in standard notation.
			Console.WriteLine("Parsing your input string: " + "\"" + ipAddress + "\"" + " produces this address (shown in its standard notation): " + address.ToString());
		}

		catch (ArgumentNullException e)
		{
			Console.WriteLine("ArgumentNullException caught!!!");
			Console.WriteLine("Source : " + e.Source);
			Console.WriteLine("Message : " + e.Message);
		}

		catch (FormatException e)
		{
			Console.WriteLine("FormatException caught!!!");
			Console.WriteLine("Source : " + e.Source);
			Console.WriteLine("Message : " + e.Message);
		}

		catch (Exception e)
		{
			Console.WriteLine("Exception caught!!!");
			Console.WriteLine("Source : " + e.Source);
			Console.WriteLine("Message : " + e.Message);
		}
	}

	void OnDestroy()
	{
		socketConnection.Dispose();
	}


	// Update is called once per frame
	void Update()
	{

		byte[] bytes = new byte[socketConnection.ReceiveBufferSize];
		while (true)
		{
			// Get a stream object for writing. 			
			NetworkStream stream1 = socketConnection.GetStream();
			if (stream1.CanWrite)
			{
				// Write byte array to socketConnection stream.                 
				stream1.Write(imageBufferBytesArray, 0, imageBufferBytesArray.Length);
			}

			// Get a stream object for reading 				
			using (NetworkStream stream = socketConnection.GetStream())
			{
				int length;
				// Read incomming stream into byte arrary. 					
				while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
				{
					var incommingData = new byte[length];
					Array.Copy(bytes, 0, incommingData, 0, length);
					// Convert byte array to string message. 						
					string serverMessage = Encoding.ASCII.GetString(incommingData);
					Debug.Log("server message received as: " + serverMessage);

					string dataReceived = Encoding.ASCII.GetString(incommingData);

					contextDetection.num_faces = 0;
					contextDetection.faces_box = new List<List<int>>();
					if (dataReceived != null && dataReceived != " ")
					{
						List<float> numbers = Array.ConvertAll(dataReceived.Split(','), float.Parse).ToList();
						for (int i = 0; i < numbers.Count; i += 4)
						{
							contextDetection.faces_box.Add(new List<int> { Convert.ToInt32(numbers[i]), Convert.ToInt32(numbers[i + 1]), Convert.ToInt32(numbers[i + 2]), Convert.ToInt32(numbers[i + 3]) });
							contextDetection.num_faces++;
						}
					}

				}
			}
		}
		//SendMessage();
	}
	/// <summary> 	
	/// Setup socket connection. 	
	/// </summary> 	
	private void ConnectToTcpServer()
	{
		try
		{

			clientReceiveThread = new Thread(new ThreadStart(ListenForData));
			clientReceiveThread.IsBackground = true;
			clientReceiveThread.Start();
		}
		catch (Exception e)
		{
			Debug.Log("On client connect exception " + e);
		}
	}
	/// <summary> 	
	/// Runs in background clientReceiveThread; Listens for incomming data. 	
	/// </summary>     
	private void ListenForData()
	{
		print("ListenForData");
		try
		{
			IPEndPoint localEndPoint = new IPEndPoint(address, connectionPort);
			print("ListenForData: localEndPoint is " + localEndPoint);
			socketConnection = new TcpClient(localEndPoint);
			socketConnection.Connect(localEndPoint);
			byte[] bytes = new byte[socketConnection.ReceiveBufferSize];
			while (true)
			{
				// Get a stream object for writing. 			
				NetworkStream stream1 = socketConnection.GetStream();
				if (stream1.CanWrite)
				{
					// Write byte array to socketConnection stream.                 
					stream1.Write(imageBufferBytesArray, 0, imageBufferBytesArray.Length);
				}

				// Get a stream object for reading 				
				using (NetworkStream stream = socketConnection.GetStream())
				{
					int length;
					// Read incomming stream into byte arrary. 					
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
					{
						var incommingData = new byte[length];
						Array.Copy(bytes, 0, incommingData, 0, length);
						// Convert byte array to string message. 						
						string serverMessage = Encoding.ASCII.GetString(incommingData);
						Debug.Log("server message received as: " + serverMessage);

						string dataReceived = Encoding.ASCII.GetString(incommingData);

						contextDetection.num_faces = 0;
						contextDetection.faces_box = new List<List<int>>();
						if (dataReceived != null && dataReceived != " ")
						{
							List<float> numbers = Array.ConvertAll(dataReceived.Split(','), float.Parse).ToList();
							for (int i = 0; i < numbers.Count; i += 4)
							{
								contextDetection.faces_box.Add(new List<int> { Convert.ToInt32(numbers[i]), Convert.ToInt32(numbers[i + 1]), Convert.ToInt32(numbers[i + 2]), Convert.ToInt32(numbers[i + 3]) });
								contextDetection.num_faces++;
							}
						}

					}
				}
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("ListenForData Socket exception: " + socketException);
		}
	}
	/// <summary> 	
	/// Send message to server using socket connection. 	
	/// </summary> 	
	private void SendMessage()
	{
		if (socketConnection == null)
		{
			print("SendMessage: socketConnection == null");
			return;
		}
		try
		{
			// Get a stream object for writing. 			
			NetworkStream stream = socketConnection.GetStream();
			if (stream.CanWrite)
			{
				// Write byte array to socketConnection stream.                 
				stream.Write(imageBufferBytesArray, 0, imageBufferBytesArray.Length);
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}

}

*/
/// <summary>
/// The working one :
/// </summary>
