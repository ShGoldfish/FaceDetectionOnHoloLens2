using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Setup socket connection.
/// </summary>
//public class NetworkConClient : MonoBehaviour
//{
//	public int connectionPort = 9005;
//	public IPAddress address;
//	private TcpClient socketConnection;
//	private Thread clientReceiveThread;
//	Manager manager;

//	private void Start()
//	{
//		manager = gameObject.GetComponent<Manager>();
//		manager.ipEndPoint = "192.168.0.6";

//		Parse();
//		print("Client from HL is trying to reach the Server on: " + address);
//		ConnectToTcpServer();
//	}


//	private void Parse()
//	{
//		try
//		{
//			address = IPAddress.Parse(manager.ipEndPoint);

//			Console.WriteLine("Parsing your input string: " + "\"" + manager.ipEndPoint + "\"" +
//				" produces this address (shown in its standard notation): " + address.ToString());
//		}

//		catch (ArgumentNullException e)
//		{
//			Console.WriteLine("ArgumentNullException caught!!!");
//			Console.WriteLine("Source : " + e.Source);
//			Console.WriteLine("Message : " + e.Message);
//		}

//		catch (FormatException e)
//		{
//			Console.WriteLine("FormatException caught!!!");
//			Console.WriteLine("Source : " + e.Source);
//			Console.WriteLine("Message : " + e.Message);
//		}

//		catch (Exception e)
//		{
//			Console.WriteLine("Exception caught!!!");
//			Console.WriteLine("Source : " + e.Source);
//			Console.WriteLine("Message : " + e.Message);
//		}
//	}


//	/// <summary> 	
//	/// Setup socket connection. 	
//	/// </summary> 	
//	private void ConnectToTcpServer()
//	{
//		try
//		{

//			clientReceiveThread = new Thread(new ThreadStart(ListenForData));
//			clientReceiveThread.IsBackground = true;
//			clientReceiveThread.Start();
//		}
//		catch (Exception e)
//		{
//			Debug.Log("On client connect exception " + e);
//		}
//	}


//	/// <summary> 	
//	/// Runs in background clientReceiveThread; Listens for incomming data. 	
//	/// </summary>     
//	private void ListenForData()
//	{
//		print("ListenForData");
//		try
//		{
//			IPEndPoint localEndPoint = new IPEndPoint(address, connectionPort);
//			print("ListenForData: localEndPoint is " + localEndPoint);
//			socketConnection = new TcpClient(localEndPoint);
//			socketConnection.Connect(localEndPoint);
//			byte[] bytes = new byte[socketConnection.ReceiveBufferSize];
//			while (true)
//			{
//				// Get a stream object for writing. 			
//				NetworkStream stream1 = socketConnection.GetStream();
//				if (stream1.CanWrite)
//				{
//					// Write byte array to socketConnection stream.       
//					byte[] sending = manager.imageBufferBytesArray;
//					stream1.Write(sending, 0, sending.Length);
//				}

//				// Get a stream object for reading 				
//				using (NetworkStream stream = socketConnection.GetStream())
//				{
//					int length;
//					// Read incomming stream into byte arrary. 					
//					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
//					{
//						var incommingData = new byte[length];
//						Array.Copy(bytes, 0, incommingData, 0, length);
//						// Convert byte array to string message. 						

//						string dataReceived = Encoding.UTF8.GetString(incommingData);
//						Debug.Log("server message received as: " + dataReceived);

//						manager.num_faces = 0;
//						manager.faces_box = new List<List<int>>();
//						if (dataReceived != null && dataReceived != " ")
//						{
//							List<float> numbers = Array.ConvertAll(dataReceived.Split(','), float.Parse).ToList();
//							for (int i = 0; i < numbers.Count; i += 4)
//							{
//								manager.faces_box.Add(new List<int> { Convert.ToInt32(numbers[i]), Convert.ToInt32(numbers[i + 1]), Convert.ToInt32(numbers[i + 2]), Convert.ToInt32(numbers[i + 3]) });
//								manager.num_faces++;
//							}
//						}

//					}
//				}
//			}
//		}
//		catch (SocketException socketException)
//		{
//			Debug.Log("ListenForData Socket exception: " + socketException);
//		}
//	}


//	/// <summary> 	
//	/// Send message to server using socket connection. 	
//	/// </summary> 	
//	private void SendMessage()
//	{
//		if (socketConnection == null)
//		{
//			print("SendMessage: socketConnection == null");
//			return;
//		}
//		try
//		{
//			// Get a stream object for writing. 			
//			NetworkStream stream = socketConnection.GetStream();
//			if (stream.CanWrite)
//			{
//				// Write byte array to socketConnection stream.                 
//				byte[] sending = manager.imageBufferBytesArray;
//				stream.Write(sending, 0, sending.Length);
//			}
//		}
//		catch (SocketException socketException)
//		{
//			Debug.Log("Socket exception: " + socketException);
//		}
//	}


//	void OnDestroy()
//	{
//		socketConnection.Dispose();
//	}
//}
