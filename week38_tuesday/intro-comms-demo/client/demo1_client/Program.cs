using System;
using System.Net.Sockets;
using System.Text;

class ChatClient
{
    static void Main()
    {
        // Skapar en ny TCP-klient som ansluter till servern på localhost (127.0.0.1) och port 5000
        TcpClient client = new TcpClient("127.0.0.1", 5000);

        // Hämtar nätverksströmmen som används för att läsa och skriva data mellan klienten och servern
        NetworkStream stream = client.GetStream();

        // Skapar en buffert på 1024 bytes för att lagra inkommande data
        byte[] buffer = new byte[1024];

        string message = "";

        while (message != "close")
        {
            Console.Write("Client: ");
            message = Console.ReadLine();

            // Konverterar meddelandet från sträng till byte-array för att kunna skicka det via nätverket
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            stream.Write(messageBytes, 0, messageBytes.Length);

            if (message == "close")
            {
                break;
            }

            // Läser data från servern till bufferten och lagrar antalet lästa bytes
            int bytesRead = stream.Read(buffer, 0, buffer.Length);

            // Konverterar de mottagna bytes till en sträng och visar serverns svar i konsolen
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine("Server: " + response);
        }

        Console.WriteLine("Closing connection...");

        client.Close();
    }
}
