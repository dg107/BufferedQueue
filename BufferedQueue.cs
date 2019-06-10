using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Generic
{
    public class BufferedQueue<T>
    {

        System.Collections.Concurrent.ConcurrentStack<T> BufferQueue = new System.Collections.Concurrent.ConcurrentStack<T>();
        System.Threading.Thread tQueueProcessor;
        object ProcessorLock = new object();
        object ClosingLock = new object();
        bool bQueueProcessorActive = false;
        bool bStopQueue = false;

        public event EventHandler Exception;
        public event EventHandler deQueue;
        public event EventHandler QueueStopped;

        public int Delay { get; set; } = 500;


        public void EnQueue(List<T> Item)
        {

            try
            {
                BufferQueue.PushRange(Item.ToArray());
            }
            catch (Exception ex)
            {
                if (ex != null)
                {
                    Exception?.Invoke(ex,  null);
                }
                return;
            }


            try
            {
                StartQueueProcessing();

            }
            catch (Exception ex)
            {
                if (Exception != null)
                {
                    Exception(ex, null);
                }
            }

        }

        private void StartQueueProcessing()
        {
            if (bQueueProcessorActive == false && bStopQueue == false && BufferQueue.Count > 0)
            {
                lock (ProcessorLock)
                {
                    if (bQueueProcessorActive == false && bStopQueue == false)
                    {
                        ParameterizedThreadStart ts = new ParameterizedThreadStart((x) => deQueueThread());
                        Thread workerThread = new Thread(ts);
                        workerThread.Name = "BufferedQueue Reader of:  " + typeof(T).ToString();
                        tQueueProcessor = workerThread;
                        tQueueProcessor.Start();
                        bQueueProcessorActive = true;
                    }
                }
            }

        }


        private void deQueueThread()
        {

            try
            {
 
                while (BufferQueue?.Count > 0 & bStopQueue == false)
                {
                    if (BufferQueue.Count > 0)
                    {
                        T[] POP = new T[BufferQueue.Count - 1];

                        BufferQueue.TryPopRange(POP);

                        var lPOP = POP.Where(x => x != null).ToList();

                        if (lPOP.Count > 0)
                        {
                            if (deQueue != null)
                            {
                                deQueue?.Invoke(lPOP, null);
                            }
                        }

                        System.Threading.Thread.Sleep(Delay);
                    }

                }

                if (bStopQueue == true)
                    if (QueueStopped != null)
                    {
                        QueueStopped.Invoke(null, null);
                    }


            }
            catch (Exception ex)
            {
                if (Exception != null)
                {
                    Exception?.Invoke(ex, null);
                }
            }
            finally
            {
                bQueueProcessorActive = false;
                if (bStopQueue == false)
                    StartQueueProcessing();
            }

        }
    }

}
