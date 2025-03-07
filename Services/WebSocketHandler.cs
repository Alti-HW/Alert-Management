using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Alert_Management.Services
{
    public class WebSocketHandler
    {
        private readonly WebSocketManager _webSocketManager;

        public WebSocketHandler(WebSocketManager webSocketManager)
        {
            _webSocketManager = webSocketManager;
        }

        public async Task HandleWebSocketAsync(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400; // Bad Request
                await context.Response.WriteAsync("WebSocket connection expected.");
                return;
            }

            using WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await _webSocketManager.AddSocket(webSocket);

            var buffer = new byte[1024 * 4];
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocketManager.RemoveSocket(webSocket);
                    break;
                }
            }
        }
    }
}
