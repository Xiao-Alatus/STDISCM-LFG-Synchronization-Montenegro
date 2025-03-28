using System;
using System.IO;
using System.Collections.Generic;
using STDISCM_LFG_Synchronization_Montenegro;
using System.Collections.Concurrent;

class Program
{
    public static bool shutdown = false;
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
                if (!uint.TryParse(valueStr, out uint value) || value > int.MaxValue)
                {
                    Console.WriteLine($"Invalid value for '{key}'. It must be between 0 and {int.MaxValue}.");
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
            if (n > 500)
            {
                Console.WriteLine("Invalid input: 'n' (maximum concurrent instances) must be less than 500.");
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
        t -= maxNumberOfParties;
        h -= maxNumberOfParties;
        d -= maxNumberOfParties * 3;

        List<DungeonInstance> instances = new List<DungeonInstance>();
        string[] instanceStatus = new string[n];

        for (uint i = 0; i < n; i++)
        {
            instanceStatus[i] = "empty";
            var instance = new DungeonInstance((int)i, instanceStatus, (int)t1, (int)t2);
            instances.Add(instance);
            instance.Start();
        }

        //Round Robin Party Dispatcher
        uint currentIndex = 0;
        while (maxNumberOfParties > 0)
        {
            if (instanceStatus[currentIndex] == "empty")
            {
                instances[(int)currentIndex].AssignWork();
                maxNumberOfParties--;
            }

            currentIndex = (currentIndex + 1) % (uint)instances.Count;
            Thread.Sleep(50); 
        }

        //Gives all the instances time to finish
        Thread.Sleep((int)(t2 + 1) * 1000); 

        Program.shutdown = true;
        foreach (var instance in instances)
        {
            instance.Signal.Set(); 
        }

        foreach (var instance in instances)
        {
            instance.Join();
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





