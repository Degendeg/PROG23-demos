using System.Net;
using System.Net.WebSockets;
using System.Text;

/*
 * 
 * Ladda ned Websocket King Client för att testa: https://t.ly/taMos
 * https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API
 * 
*/

async Task MainAsync()
{
    HttpListener httpListener = new HttpListener();
    httpListener.Prefixes.Add("http://localhost:8080/");
    httpListener.Start();
    Console.WriteLine("Waiting for WebSocket connections...");

    while (true)
    {
        HttpListenerContext context = await httpListener.GetContextAsync();

        if (context.Request.IsWebSocketRequest)
        {
            HttpListenerWebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
            WebSocket webSocket = wsContext.WebSocket;

            await HandleWebSocketConnection(webSocket);
        }
        else
        {
            context.Response.StatusCode = 400; // bad req
            context.Response.Close();
        }
    }
}

async Task HandleWebSocketConnection(WebSocket webSocket)
{
    byte[] buffer = new byte[1024 * 4];

    while (webSocket.State == WebSocketState.Open)
    {
        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        if (result.MessageType == WebSocketMessageType.Close)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            Console.WriteLine("Connection closed.");
        }
        else
        {
            string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            Console.WriteLine("Received: " + message);

            string response = $"Echo: {message}";
            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
            await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}

await MainAsync();