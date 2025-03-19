using System;
using System.Collections.Generic;
using System.Linq;
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

        public int InstanceID => instanceID;
        public int PartiesServed => partiesServed;
        public int TotalTimeServed => totalTimeServed;

        public DungeonInstance(int id, string[] instanceStatus)
        {
            instanceID = id;
            this.instanceStatus = instanceStatus;
        }

        public void Run(int t1, int t2, ref uint remainingParties)
        {
            while (true)
            {
                lock (Program.queueLock)
                {
                    if (remainingParties == 0)
                        break; 

                    remainingParties--; 
                    instanceStatus[instanceID] = "active"; 
                    PrintInstanceStatus();
                }

                int instanceTime = random.Next(t1, t2 + 1);
                partiesServed++;
                totalTimeServed += instanceTime;

                Thread.Sleep(instanceTime * 1000); 

                lock (Program.queueLock)
                {
                    instanceStatus[instanceID] = "empty"; 
                    Console.WriteLine($"\nInstance {instanceID + 1} completed.");
                    PrintInstanceStatus();
                }

                // **Ensure instance waits before taking another party (Round-Robin fairness)**
                Thread.Sleep(500);
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
