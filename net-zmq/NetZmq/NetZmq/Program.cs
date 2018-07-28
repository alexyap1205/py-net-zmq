using System;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace NetZmq
{
    class Program
    {
        static void Main(string[] args)
        {
            var subscriberTask = Task.Run(() =>
            {
                using (var subSocket = new SubscriberSocket())
                {
                    var tcpLocalhost = "tcp://127.0.0.1:61044";
                    subSocket.Connect(tcpLocalhost);
                    subSocket.SubscribeToAnyTopic();

                    while (true)
                    {
                        var multipartMesssage = subSocket.ReceiveMultipartMessage();

                        if (multipartMesssage.Count() > 1)
                        {
                            Console.WriteLine($"Topic: {multipartMesssage[0].ConvertToString(Encoding.UTF8)}");
                            Console.WriteLine($"Message: {multipartMesssage[1].ConvertToString(Encoding.UTF8)}");
                        }
                        else
                        {
                            Console.WriteLine($"Bytes: {BitConverter.ToString(multipartMesssage[0].ToByteArray())}");
                        }
                    }
                }
            });

            var requesterTask = Task.Run(() =>
            {
                using (var reqSocket = new RequestSocket("tcp://127.0.0.1:8888"))
                {
                    for (var i = 0; i < 20; i++)
                    {
                        reqSocket.SendFrame("Test Message");
                        reqSocket.ReceiveFrameString(Encoding.UTF8);
                        Thread.Sleep(500);
                    }
                }
            });

            Task.WaitAll(subscriberTask, requesterTask);
            Console.ReadLine();
        }
    }
}