using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Alert_Management.Models;
using Alert_Management.Infterfaces;  // Import IAlertService

namespace Alert_Management.Services
{
    public class WebSocketManager
    {
        private readonly List<WebSocket> _sockets = new List<WebSocket>();
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1); // Thread safety
        private readonly IAlertService _alertService;  // Inject Alert Service

        public WebSocketManager(IAlertService alertService)
        {
            _alertService = alertService;
        }

        public async Task AddSocket(WebSocket socket)
        {
            await _lock.WaitAsync();
            try
            {
                _sockets.Add(socket);

                // Fetch the latest alerts when a new connection is established
                var alerts = await _alertService.GetAllAlertsAsync();
                await SendMessageAsync(socket, "get_alerts", alerts);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task RemoveSocket(WebSocket socket)
        {
            await _lock.WaitAsync();
            try
            {
                if (_sockets.Contains(socket))
                {
                    _sockets.Remove(socket);
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task BroadcastMessageAsync(string eventType, object data)
        {
            string message = JsonSerializer.Serialize(new { eventType, data });
            var buffer = Encoding.UTF8.GetBytes(message);

            await _lock.WaitAsync();
            try
            {
                List<WebSocket> closedSockets = new List<WebSocket>();

                foreach (var socket in _sockets)
                {
                    if (socket.State == WebSocketState.Open)
                    {
                        try
                        {
                            await socket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length),
                                WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                        catch
                        {
                            closedSockets.Add(socket);
                        }
                    }
                    else
                    {
                        closedSockets.Add(socket);
                    }
                }

                // Remove closed sockets
                foreach (var closedSocket in closedSockets)
                {
                    _sockets.Remove(closedSocket);
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task SendMessageAsync(WebSocket socket, string eventType, object data)
        {
            if (socket.State == WebSocketState.Open)
            {
                string message = JsonSerializer.Serialize(new { eventType, data });
                var buffer = Encoding.UTF8.GetBytes(message);

                await socket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length),
                    WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
