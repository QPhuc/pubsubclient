using System;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Tasks;

using Azure.Messaging.WebPubSub;

using Websocket.Client;

namespace clientsub
{
    class Program
    {
        static async Task Main()
        {
            //if (args.Length != 2)
            //{
            //    Console.WriteLine("Usage: clientsub <connectionString> <hub>");
            //    return;
            //}
            var connectionString = "Endpoint=https://spheraumapubsub.webpubsub.azure.com;AccessKey=tJMVtq9G5PXef6AspMHShPENg+1mQ9Qfcr3JiyX0sG0=;Version=1.0;";
            var hub = "chat";

            // Either generate the URL or fetch it from server or fetch a temp one from the portal
            var serviceClient = new WebPubSubServiceClient(connectionString, hub);
            var url = serviceClient.GenerateClientAccessUri(userId: "user1", roles: new string[] { "webpubsub.joinLeaveGroup", "webpubsub.sendToGroup" });

            using (var client = new WebsocketClient(url, () =>
            {
                var inner = new ClientWebSocket();
                inner.Options.AddSubProtocol("json.webpubsub.azure.v1");
                return inner;
            }))
            {
                // Disable the auto disconnect and reconnect because the sample would like the client to stay online even no data comes in
                client.ReconnectTimeout = null;
                client.MessageReceived.Subscribe(msg => Console.WriteLine($"Message received: {msg}"));
                await client.Start();
                Console.WriteLine("Connected.");
                Console.WriteLine("Please input group name:");
                var group = Console.ReadLine();
                client.Send(JsonSerializer.Serialize(new
                {
                    type = "joinGroup",
                    group = group,
                    ackId = 1
                }));
                Console.Read();
            }
        }
    }
}
