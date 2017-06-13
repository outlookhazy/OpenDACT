using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Klipper_Calibration_Tool.Classes
{
    public delegate void MessageReceivedHandler(string message);

    static class PseudoSerial
    {
        static SshClient client;
        static ShellStream ss;

        static Queue<Tuple<string, int>> messageQueue = new Queue<Tuple<string,int>>();

        volatile static int waitingresponse = 0;

        public static event MessageReceivedHandler MessageReceived;

        private static void OnMessage(string message)
        {
            if (MessageReceived != null)
                MessageReceived(message);
        }

        public static void Init()
        {
            client = new SshClient("192.168.76.239", "pi", "raspberry");
            client.Connect();
            ss = client.CreateShellStream("fakeshell", 80, 200, 50, 100, 80000);
            Task readTask = new Task(new Action(() => { ReadThread(); }));
            readTask.Start();
        }

        public static void ReadThread()
        {
            Console.WriteLine("connecting");

            while (!ss.ReadLine().Contains("$")) {
                ss.WriteLine("\n");
                Thread.Sleep(500);
            }
            Console.WriteLine("connected, starting printer stream");
            ss.WriteLine("cat /tmp/printer");
            while (true)
            {
                string line = ss.ReadLine();
                waitingresponse--;
                Debug.WriteLine(line);
                OnMessage(line);
                
            }
        }

        public static void WriteLine(string text, int responselength)
        {
            messageQueue.Enqueue(new Tuple<string, int>(text, responselength));
            if(! queueactive)
                ProcessQueue();                       
        }
        public static volatile bool queueactive = false;
        private static void ProcessQueue()
        {
            queueactive = true;

            if (waitingresponse > 0)
            {
                Task.Delay(100).ContinueWith(t => { ProcessQueue(); });
                return;
            }

            Tuple<string, int> nextmessage = messageQueue.Dequeue();
            waitingresponse = nextmessage.Item2;
            Debug.WriteLine("Sending " + nextmessage.Item1);
            client.RunCommand(String.Format("echo {0} > /tmp/printer", nextmessage.Item1));

            if (messageQueue.Count > 0)
                Task.Delay(100).ContinueWith(t => { ProcessQueue(); });
            else
                queueactive = false;
        }
    }
}
