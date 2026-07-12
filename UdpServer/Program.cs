using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

class UdpServer
{
    static void Main(string[] args)
    {
        // Define the target IP and Port. 
        // IPAddress.Broadcast (255.255.255.255) sends to everyone on the local network.
        string targetIp = "255.255.255.255";
        int targetPort = 11000;

        using UdpClient udpServer = new UdpClient();
        // Enable broadcasting on the socket
        udpServer.EnableBroadcast = true;

        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(targetIp), targetPort);

        Console.WriteLine($"UdpServer started. Broadcasting to {targetIp}:{targetPort} every 2 seconds...\n");

        while (true)
        {
            // 1. Create the payload
            var message = new StatusMessage
            {
                Status = "running",
                Timestamp = DateTime.Now
            };

            // 2. Serialize to JSON string, then convert to byte array
            string jsonString = JsonSerializer.Serialize(message);
            byte[] bytesToSend = Encoding.UTF8.GetBytes(jsonString);

            // 3. Send the data
            udpServer.Send(bytesToSend, bytesToSend.Length, remoteEndPoint);

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Sent: {jsonString}");

            // 4. Wait for 2 seconds
            Thread.Sleep(2000);
        }
    }
}