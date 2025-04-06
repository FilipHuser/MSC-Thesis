using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Text;
using FHMA.Models.ExternalModels;
using System.Text.Json;

class Program
{
    static void Main(string[] args)
    {
        var model = new KMModel() { Val0 = 420 , Val1 = "asdf" , Val2 = 69 };
        var json = JsonSerializer.Serialize(model);




        while (true)
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPAddress serverAddr = IPAddress.Parse("192.168.2.255");

            IPEndPoint endPoint = new IPEndPoint(serverAddr, 12345);

            byte[] payload = Encoding.ASCII.GetBytes(json);

            sock.SendTo(payload, endPoint);
            Console.WriteLine($"Sent => {json}");

            Thread.Sleep(500);
        }
    }
}
