using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;

namespace Klipper_Calibration_Tool.Classes
{
    public delegate void MessageReceivedHandler(string message);

    internal static class PseudoSerial
    {
        private static SshClient _client;
        private static ShellStream _ss;

        private static readonly Queue<Tuple<string, int>> MessageQueue = new Queue<Tuple<string, int>>();

        static volatile int _waitingresponse;

        public static event MessageReceivedHandler MessageReceived;

        private static void OnMessage(string message)
        {
            MessageReceived?.Invoke(message);
        }

        public static void Init()
        {
            _client = new SshClient("192.168.76.239", "pi", "raspberry");
            _client.Connect();
            _ss = _client.CreateShellStream("fakeshell", 80, 200, 50, 100, 80000);
            Task readTask = new Task(ReadThread);
            readTask.Start();
        }

        public static void ReadThread()
        {
            Console.WriteLine("connecting");

            while (!_ss.ReadLine().Contains("$"))
            {
                _ss.WriteLine("\n");
                Thread.Sleep(500);
            }
            Console.WriteLine("connected, starting printer stream");
            _ss.WriteLine("cat /tmp/printer");
            while (true)
            {
                string line = _ss.ReadLine();
                _waitingresponse--;
                Debug.WriteLine(line);
                OnMessage(line);
            }
        }

        public static void WriteLine(string text, int responselength)
        {
            MessageQueue.Enqueue(new Tuple<string, int>(text, responselength));
            if (!Queueactive)
                ProcessQueue();
        }
        public static volatile bool Queueactive;
        private static void ProcessQueue()
        {
            Queueactive = true;

            if (_waitingresponse > 0)
            {
                Task.Delay(100).ContinueWith(t => { ProcessQueue(); });
                return;
            }

            Tuple<string, int> nextmessage = MessageQueue.Dequeue();
            _waitingresponse = nextmessage.Item2;
            Debug.WriteLine("Sending " + nextmessage.Item1);
            _client.RunCommand($"echo {nextmessage.Item1} > /tmp/printer");

            if (MessageQueue.Count > 0)
                Task.Delay(100).ContinueWith(t => { ProcessQueue(); });
            else
                Queueactive = false;
        }
    }
}
