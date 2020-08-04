using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using System.Globalization;

public class NetworkCon : MonoBehaviour
{
	Thread mThread_get;
	public int connectionPort = 9005; 
	Socket sender;

	public byte[] imageBufferBytesArray;
	bool recieving;
	ContextDetection contextDetection;

	private void Start()
	{
		contextDetection = gameObject.GetComponent<ContextDetection>();
		recieving = false;
		ThreadStart ts_get = new ThreadStart(GetInfo);
		mThread_get = new Thread(ts_get);
		mThread_get.Start();
		Debug.Log("Shakiba's app trying net connection.");
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
		throw new System.Exception("No network adapters with an IPv4 address in the system!");
	}

	void GetInfo()
	{
		IPEndPoint localEndPoint = new IPEndPoint(GetLocalIPAddress(), connectionPort);
		Socket listenerSoc = new Socket(GetLocalIPAddress().AddressFamily,
				   SocketType.Stream, ProtocolType.Tcp);
		print(localEndPoint);
		listenerSoc.Bind(localEndPoint);
		listenerSoc.Listen(10);
		sender = listenerSoc.Accept();
			   
		recieving = true;
		while (recieving)
		{
			ConnectionRec();
		}
	}

	void ConnectionRec()
	{						 
		byte[] buffer = new byte[sender.ReceiveBufferSize];
		
		sender.Send(imageBufferBytesArray, imageBufferBytesArray.Length, 0);

		int bytesRead = sender.Receive(buffer);
		string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);

		if (dataReceived != null)
		{
			if (dataReceived == "stop")
			{
				recieving = false;
			}
			else
			{

				int o;
				if (int.TryParse(dataReceived, NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat, out o))
					contextDetection.num_faces = o;
				else
					print("can't parse");
				print("Number of Detected Faces: " + dataReceived.ToString());
				
			}
		}
	}

	void OnDestroy()
	{
		sender.Close();
	}
}
