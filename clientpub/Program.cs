using System;
using System.IO;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Tasks;

using Azure.Messaging.WebPubSub;

using Websocket.Client;

namespace clientpub
{
    class Program
    {
        static async Task Main()
        {
            //if (args.Length != 2)
            //{
            //    Console.WriteLine("Usage: clientpub <connectionString> <hub>");
            //    return;
            //}
            var connectionString = "Endpoint=https://spheraumapubsub.webpubsub.azure.com;AccessKey=tJMVtq9G5PXef6AspMHShPENg+1mQ9Qfcr3JiyX0sG0=;Version=1.0;";
            var hub = "chat";

            // Either generate the URL or fetch it from server or fetch a temp one from the portal
            var serviceClient = new WebPubSubServiceClient(connectionString, hub);
            var url = serviceClient.GenerateClientAccessUri(userId: "user1", roles: new string[] {"webpubsub.joinLeaveGroup", "webpubsub.sendToGroup"});

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
                int ackId = 1;
                /* Send to group `demogroup` */
                Console.WriteLine("Please input group name you want connect:");
                var group = Console.ReadLine();
                Console.WriteLine("Please input text:");
                var streaming = Console.ReadLine();
                while (streaming != null)
                {
                    client.Send(JsonSerializer.Serialize(new
                    {
                        type = "sendToGroup",
                        group = group,
                        dataType = "text",
                        data = streaming,
                        ackId = ackId++
                    }));
                    Console.WriteLine("Please input group name you want connect:");
                    group = Console.ReadLine();
                    Console.WriteLine("Please input text:");
                    streaming = Console.ReadLine();
                }

                Console.WriteLine("done");
                /*  ------------  */
            }
        }
    }
}
