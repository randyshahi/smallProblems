/*

Create a task scheduler

*/

// See https://aka.ms/new-console-template for more information
using System.Reflection;

Console.WriteLine("Hello, World!");

public class TaskScheduler
{
    private readonly int totalCores;
    private PriorityQueue<int, int> freeCores;
    private HashSet<int> usedCores;
    private int errorNoFreeCores = -1;
    private Dictionary<int, int> coreToExecutionTimeDict;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="numberOfCores"></param>
    TaskScheduler(int numberOfCores)
    {
        this.totalCores = numberOfCores;
        this.usedCores = new HashSet<int>();
        this.freeCores = new PriorityQueue<int, int>();
        this.coreToExecutionTimeDict = new Dictionary<int, int>();

        for(int i = 0; i < numberOfCores; i++)
        {
            freeCores.Enqueue(i, 0);
            coreToExecutionTimeDict[i] = 0;
        }
    }

    /// <summary>
    /// Assigns a job to a free core.
    /// </summary>
    /// <returns>-1 if no free core.</returns>
    int AssignJob()
    {
        if(freeCores.Count > 0)
        {
            int core = freeCores.Dequeue();
            usedCores.Add(core);
            return core;
        }
        return errorNoFreeCores;
    }

    void ReturnCore(int coreNumber)
    {
        // prevents adding the same core twice
        if(usedCores.Contains(coreNumber))
        {
            freeCores.Enqueue(coreNumber);
            usedCores.Remove(coreNumber);
        }
    }
}
