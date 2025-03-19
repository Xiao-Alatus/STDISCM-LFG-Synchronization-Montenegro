﻿using System;
using System.IO;
using System.Collections.Generic;
using STDISCM_LFG_Synchronization_Montenegro;

class Program
{
    public static object queueLock = new object();
    //Default Values
    public static uint t = 10, h = 10, d = 32, n = 5, t1 = 10, t2 = 60, maxNumberOfParties;

    static void Main()
    {
        string path = "../../../Input.txt";

        var variableMap = new Dictionary<string, Action<uint>>
        {
            { "n", value => n = value },
            { "t", value => t = value },
            { "h", value => h = value },
            { "d", value => d = value },
            { "t1", value => t1 = value },
            { "t2", value => t2 = value }
        };

        try
        {
            foreach (string line in File.ReadLines(path))
            {
                string cleanLine = line.Split("//")[0].Trim();

                if (string.IsNullOrWhiteSpace(cleanLine))
                    continue;

                string[] parts = cleanLine.Split('=');

                if (parts.Length != 2)
                {
                    Console.WriteLine("Invalid input format. Each line must be in 'key=value' format.");
                    return;
                }

                string key = parts[0].Trim();
                string valueStr = parts[1].Trim();

                if (!variableMap.ContainsKey(key))
                {
                    Console.WriteLine($"Invalid key '{key}'. Allowed keys are: n, t, h, d, t1, t2.");
                    return;
                }

                if (!uint.TryParse(valueStr, out uint value))
                {
                    Console.WriteLine($"Invalid value for '{key}'. It must be a non-negative integer.");
                    return;
                }

                variableMap[key](value);
            }

            // Validation after parsing
            if (n == 0)
            {
                Console.WriteLine("Invalid input: 'n' (maximum concurrent instances) must be greater than 0.");
                return;
            }
            if (t == 0)
            {
                Console.WriteLine("Invalid input: 't' (tank players) is missing or zero.");
                return;
            }
            if (h == 0)
            {
                Console.WriteLine("Invalid input: 'h' (healer players) is missing or zero.");
                return;
            }
            if (d == 0)
            {
                Console.WriteLine("Invalid input: 'd' (DPS players) is missing or zero.");
                return;
            }
            if (t1 == 0)
            {
                Console.WriteLine("Invalid input: 't1' (minimum instance time) must be greater than 0.");
                return;
            }
            if (t2 == 0)
            {
                Console.WriteLine("Invalid input: 't2' (maximum instance time) must be greater than 0.");
                return;
            }
            if (t1 > t2)
            {
                Console.WriteLine("Invalid input: 't1' (minimum instance time) cannot be greater than 't2' (maximum instance time).");
                return;
            }

        }
        catch (Exception)
        {
            Console.WriteLine("Error reading the file. Please try again.");
            return;
        }

        Console.WriteLine($"{n} = Number of Concurrent Dungeon Instances\n" +
                          $"{t} = Number of Tank Players\n" +
                          $"{h} = Number of Healer Players\n" +
                          $"{d} = Number of DPS Players\n" +
                          $"{t1} = Minimum Time\n" +
                          $"{t2} = Maximum Time");


        maxNumberOfParties = Math.Min(t, Math.Min(h, d / 3));
        Console.WriteLine($"Max number of parties that can be formed: {maxNumberOfParties}");

        t = t - maxNumberOfParties;
        h = h - maxNumberOfParties;
        d = d - 3 * maxNumberOfParties;


        List<DungeonInstance> instances = new List<DungeonInstance>();
        List<Thread> instanceThreads = new List<Thread>();
        Queue<DungeonInstance> instanceQueue = new Queue<DungeonInstance>();
        string[] instanceStatus = new string[n];

     
        for (int i = 0; i < n; i++)
        {
            instanceStatus[i] = "empty";
            DungeonInstance instance = new DungeonInstance(i, instanceStatus);
            instances.Add(instance);
            instanceQueue.Enqueue(instance); 
        }

      
        while (true)
        {
            lock (queueLock)
            {
                if (maxNumberOfParties == 0)
                    break; 

         
                DungeonInstance instance = instanceQueue.Dequeue();
                instanceQueue.Enqueue(instance);

                
                Thread instanceThread = new Thread(() => instance.Run((int)t1, (int)t2, ref maxNumberOfParties));
                instanceThreads.Add(instanceThread);
                instanceThread.Start();
            }
        }

        
        foreach (var thread in instanceThreads)
        {
            thread.Join();
        }

     
        Console.WriteLine("\n=== Summary ===\n");
        foreach (var instance in instances)
        {
            instance.DisplaySummary();
        }

        Console.WriteLine($"\nTotal parties served: {instances.Sum(inst => inst.PartiesServed)}");
        Console.WriteLine($"\nTotal time served: {instances.Sum(inst => inst.TotalTimeServed)} seconds");
        Console.WriteLine($"\nRemaining players who didn't join a dungeon:");
        Console.WriteLine($"  Tanks: {t}");
        Console.WriteLine($"  Healers: {h}");
        Console.WriteLine($"  DPS: {d}");


    }
}





