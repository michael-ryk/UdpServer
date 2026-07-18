using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

class UdpServer
{
    // Define the target IP and Port. 
    // IPAddress.Broadcast (255.255.255.255) sends to everyone on the local network.
    private const string BroadcastIP = "255.255.255.255";
    private const int BroadcastPort = 11000;
    private const int ListenPort = 12000;

    static async Task Main(string[] args)
    {
        Console.Title = "UDP Server Manager";
        Console.WriteLine("Initializing UDP Server Modules...");

        // Start background worker to listen for incoming client actions
        Task listeningTask = Task.Run(() => StartListeningForClients());

        // Start background worker to constantly broadcast server heartbeat statuses
        Task broadcastingTask = Task.Run(() => StartBroadcastingStatus());

        // Keep the application running indefinitely
        await Task.WhenAll(listeningTask, broadcastingTask);
    }

    // TASK A: Constantly Broadcast status to client's listener window
    static void StartBroadcastingStatus()
    {
        using UdpClient udpServer = new UdpClient();
        
        // Enable broadcasting on the socket
        udpServer.EnableBroadcast = true;

        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(BroadcastIP), BroadcastPort);

        Console.WriteLine($"UdpServer started. Broadcasting to {BroadcastIP}:{BroadcastPort} every 2 seconds...\n");

        while (true)
        {
            // 1. Create the payload
            var msg = new StatusMessage
            {
                Status = "running",
                Timestamp = DateTime.Now
            };
            
            // 2. Serialize to JSON string, then convert to byte array
            string jsonString = JsonSerializer.Serialize(msg);
            byte[] bytesToSend = Encoding.UTF8.GetBytes(jsonString);

            // 3. Send the data
            udpServer.Send(bytesToSend, bytesToSend.Length, remoteEndPoint);

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Sent: {jsonString}");

            // 4. Wait for 2 seconds
            Thread.Sleep(2000);
        }
    }

    static void StartListeningForClients()
    {
        using UdpClient udpListener = new UdpClient(ListenPort);
        IPEndPoint ClientEndPoint = new IPEndPoint(IPAddress.Any, 0);

        Console.WriteLine($"Listening for client commands on port {ListenPort}...\n");

        while (true)
        {
            try
            { 
                byte[] receivedBytes = udpListener.Receive(ref ClientEndPoint);
                string receivedJson = Encoding.UTF8.GetString(receivedBytes);

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Received from Client ({ClientEndPoint.Address}): {receivedJson}");

                // Parse the client command safely
                CommandMessage? command = JsonSerializer.Deserialize<CommandMessage>(receivedJson);

                if (command != null)
                {
                    // Formulate a unique contextual response structure back to the client
                    var reaction = new
                    {
                        Event = "SERVER_REACTION",
                        ReceivedCommand = command.Command,
                        ServerMessage = $"Acknowledged! Processed payload: '{command.Payload.ToUpper()}'",
                        ProcessedAt = DateTime.Now
                    };

                    byte[] replyBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(reaction));

                    // CRITICAL: We reply directly back to the client's listening terminal window (Port 11000) 
                    // instead of their sending terminal port.
                    IPEndPoint clientResponseEP = new IPEndPoint(ClientEndPoint.Address, BroadcastPort);

                    udpListener.Send(replyBytes, replyBytes.Length, clientResponseEP);
                    Console.WriteLine($" -> Dispatched unique reaction to {clientResponseEP}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Listener Error: {ex.Message}");
            }
        }
    }
}