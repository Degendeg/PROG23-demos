using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class ChatServer
{
    static void Main()
    {
        // Skapar en TcpListener för att lyssna på alla IP-adresser (IPAddress.Any) på port 5000
        TcpListener server = new TcpListener(IPAddress.Any, 5000);
        server.Start(); // Startar servern och börjar lyssna efter inkommande anslutningar
        Console.WriteLine("Server started...");

        // Väntar tills en klient ansluter och accepterar den anslutningen
        TcpClient client = server.AcceptTcpClient();
        NetworkStream stream = client.GetStream(); // Hämtar nätverksströmmen för att kunna skicka och ta emot data

        // Buffert för att lagra mottagna data från klienten
        byte[] buffer = new byte[1024];
        string message = "";

        while (message != "close")
        {
            // Läser data som skickats från klienten och lagrar det i bufferten
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Console.WriteLine("Client: " + message);

            if (message == "close")
            {
                break;
            }

            // Servern skriver sitt svar i konsolen, som sedan skickas till klienten
            Console.Write("Server: ");
            string response = Console.ReadLine();
            byte[] responseBytes = Encoding.UTF8.GetBytes(response); // Konverterar svaret till byte-array
            stream.Write(responseBytes, 0, responseBytes.Length); // Skickar svaret till klienten
        }

        Console.WriteLine("Closing connection...");
        client.Close();
        server.Stop();
    }
}
