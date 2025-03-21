using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace STDISCM_LFG_Synchronization_Montenegro
{
    class DungeonInstance
    {
        private static Random random = new Random();
        private int instanceID;
        private int partiesServed = 0;
        private int totalTimeServed = 0;
        private string[] instanceStatus;
        private AutoResetEvent signal;
        private Thread instanceThread;
        private int t1, t2;
        private bool hasWork = false;



        //short hand for getters
        public AutoResetEvent Signal => signal;
        public int InstanceID => instanceID;
        public int PartiesServed => partiesServed;
        public int TotalTimeServed => totalTimeServed;

        public DungeonInstance(int id, string[] instanceStatus, int t1, int t2)
        {
            instanceID = id;
            this.instanceStatus = instanceStatus;
            this.t1 = t1;
            this.t2 = t2;
            signal = new AutoResetEvent(false);
            instanceThread = new Thread(Run);
        }


        public void AssignWork()
        {
            hasWork = true;
            signal.Set();
        }

        public void Start()
        {
            instanceThread.Start();
        }

        public void Join()
        {
            instanceThread.Join();
        }



        public void Run()
        {
            while (true)
            {
                signal.WaitOne();

                if (Program.shutdown)
                    break;

                if (!hasWork)
                    continue;

                hasWork = false;

                instanceStatus[instanceID] = "active";
                PrintInstanceStatus();

                int instanceTime = random.Next(t1, t2 + 1);
                partiesServed++;
                totalTimeServed += instanceTime;

                Thread.Sleep(instanceTime * 1000);

                instanceStatus[instanceID] = "empty";
                Console.WriteLine($"\nInstance {instanceID + 1} completed.");
                PrintInstanceStatus();

                // Let dispatcher know this instance is free again
                Program.availableInstances.Enqueue(instanceID);
            }
        }


        public void PrintInstanceStatus()
        {
            Console.WriteLine("\n=== Instance Status ===");
            for (int i = 0; i < instanceStatus.Length; i++)
            {
                Console.WriteLine($"Instance {i + 1}: {instanceStatus[i]}");
            }
        }

        public void DisplaySummary()
        {
            Console.WriteLine($"Instance {instanceID + 1}: Served {partiesServed} parties, Total Time: {totalTimeServed} seconds");
        }
    }



}
