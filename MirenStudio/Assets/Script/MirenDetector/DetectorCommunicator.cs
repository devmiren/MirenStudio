using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

using System;
using System.IO.Pipes;

namespace MirenDetector
{
    public class DetectorCommunicator
    {
        public ConcurrentQueue<DetectionResult> result;
        private Thread ResultUpdateThread;

        public DetectorCommunicator()
        {
            result = new ConcurrentQueue<DetectionResult>();
            ResultUpdateThread = new Thread(UpdateResults);
            ResultUpdateThread.Start();
        }

        public void Stop()
        {
            if(ResultUpdateThread != null)
            {
                ResultUpdateThread.Abort();
                ResultUpdateThread = null;
            }
        }

        public void Start()
        {
            if (ResultUpdateThread != null) return;
            ResultUpdateThread = new Thread(UpdateResults);
            ResultUpdateThread.Start();
        }

        private void UpdateResults()
        {
            byte[] arr = new byte[1000];
            while (true)
            {
                NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "MirenPipe", PipeDirection.InOut);
                Debug.Log("Connect start");
                try
                {
                    pipeClient.Connect();
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                    Task.Delay(1000).Wait();
                    continue;
                }
                Debug.Log("Connect Success");

                while (true)
                {
                    int length = pipeClient.Read(arr, 0, 1000);
                    if (length == 0) break;
                    if(result.Count < 3) result.Enqueue(new DetectionResult(arr));
                }
                pipeClient.Close();
                pipeClient.Dispose();
            }
        }
        ~DetectorCommunicator()
        {
            Stop();
        }
    }
}