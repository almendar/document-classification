using System;
using System.Threading;

public class Worker
{
    // This method will be called when the thread is started.
    string a = "abc";
    public void DoWorkLock()
    {
        lock (this)
        {
            while (true)
            {
                a = DateTime.Now.ToLongTimeString();
                Console.WriteLine(Thread.CurrentThread.Name + " " + a);
            }
        }
    }

    public void DoWorkNoLock()
    {
   
            while (true)
            {

                Console.WriteLine(Thread.CurrentThread.Name + " " + a);
            }
    }
}

public delegate void CallbackFunction(String d); 

public class WorkerThreadExample
{
    static void Main()
    {
        // Create the thread object. This does not start the thread.
        Worker workerObj = new Worker();
        Thread workerThread1 = new Thread(workerObj.DoWorkLock);
        workerThread1.Name = "thread1";
        Thread workerThread2 = new Thread(workerObj.DoWorkLock);
        workerThread2.Name = "thread2";
        workerThread1.Start();
        workerThread2.Start();
        workerThread1.Join();
        workerThread2.Join();
        
    }
}