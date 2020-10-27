using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SignalRTestClient
{
    class Program
    {
        private const string ServerUrl = "https://localhost:44362/";

        static async Task Main(string[] args)
        {
            await RunConnectionTest();
        }

        static async Task RunConnectionTest()
        {
            var connections = new List<HubConnection>();

            try
            {
                var connectionCount = 0;
                while (connectionCount < 4900)
                {
                    connections.Add(await CreateConnection("signalr/hubs/hub1"));
                    connections.Add(await CreateConnection("signalr/hubs/hub2"));
                    connections.Add(await CreateConnection("signalr/hubs/hub3"));
                    connections.Add(await CreateConnection("signalr/hubs/hub4"));
                    connections.Add(await CreateConnection("signalr/hubs/hub5"));
                    connectionCount = connections.Count;
                    if (connectionCount % 25 == 0)
                    {
                        Console.WriteLine($"Number of connections: {connectionCount}");
                    }
                }
                await CloseConnections(connections);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static async Task CloseConnections(List<HubConnection> connections)
        {
            var connectionCount = connections.Count;
            var disconnectedCount = 0;
            foreach (var connection in connections)
            {
                try
                {
                    if (connection.State != HubConnectionState.Disconnected)
                    {
                        await connection.StopAsync();
                        disconnectedCount++;
                    }
                }
                catch (Exception ex)
                {
                    Console.Write(ex);
                }
                Console.Write("-");
            }

            Console.WriteLine($"Number of opened connections: {connectionCount}. Number of closed connections: {disconnectedCount}");
        }

        static async Task<HubConnection> CreateConnection(string hubPath)
        {
            var hubUrl = $"{ServerUrl}{hubPath}";

            var hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.SkipNegotiation = true;
                    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                })
                .Build();

            hubConnection.On<int>("SomethingHappened", id =>
            {
                Console.Write("*");
            });

            hubConnection.Closed += async (error) =>
            {
                var errorMessage = error == null
                    ? "x"
                    : $"Error: {error}";

                await Task.CompletedTask;
            };

            if (hubConnection.State != HubConnectionState.Connected)
            {
                await hubConnection.StartAsync();
            }
            await hubConnection.SendAsync("Subscribe", 1);

            return hubConnection;
        }
    }
}
