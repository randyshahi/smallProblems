/*

Create a task scheduler

*/

// See https://aka.ms/new-console-template for more information
using System.Data.Common;
using System.Reflection;
using System.Reflection.Metadata;

Console.WriteLine("Hello, World!");

public class TaskScheduler
{
    /// <summary>
    /// Longest time in Ms that a core can spend on a job.
    /// </summary>
    private const int TimeoutMs = 5000;

    /// <summary>
    /// Error code to denote that no core could be allocated
    /// </summary>
    private const int ErrorNoFreeCores = -1;

    /// <summary>
    /// Total number of cores available to assign to jobs
    /// </summary>
    private readonly int totalCores;

    /// <summary>
    /// Priority queue where the priority value is equal to the total time the core spend on preforming jobs.
    /// The next core to be assigned will always be the core which has the least amount of execution time in
    /// milliseconds
    /// </summary>
    private PriorityQueue<int, long> availableCores;

    /// <summary>
    /// Keeps track of which cores are currently running jobs paired with the timestamp of when the job started.
    /// </summary>
    private Dictionary<int, DateTimeOffset> usedCoresDict;

    private PriorityQueue<int, long> jobsWithExpiryQueue = new PriorityQueue<int, long>();

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="numberOfCores"></param>
    TaskScheduler(int numberOfCores)
    {
        this.totalCores = numberOfCores;
        this.usedCoresDict = new Dictionary<int, DateTimeOffset>();
        this.availableCores = new PriorityQueue<int, long>();

        for(int i = 0; i < numberOfCores; i++)
        {
            availableCores.Enqueue(i, 0);
        }
    }


    /// <summary>
    /// Will check for jobs that are running other their timeout period and cancel them. These cores will
    /// then be returned to the pool
    /// </summary>
    void Process()
    {
        while(true)
        {
            long currentTimeEpoch = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            if(jobsWithExpiryQueue.Count > 0 && jobsWithExpiryQueue.Peek() > currentTimeEpoch)
            {
                // we have jobs that potentially need to be timed-out
                int core = jobsWithExpiryQueue.Dequeue();
                if(usedCoresDict.ContainsKey(core))
                {
                    // remove from dict and add back into pool. Need to also keep track of execution time
                    DateTimeOffset startTime = usedCoresDict[core];
                    availableCores.Enqueue(core, startTime.ToUnixTimeMilliseconds() + TimeoutMs);
                    usedCoresDict.Remove(core);
                }

            }
            else
            {
                // sleep until the next timeout
                jobsWithExpiryQueue.TryPeek(out _, out long nextTimeoutEpoch);
                long timeToSleepMs = nextTimeoutEpoch - currentTimeEpoch;
                Thread.Sleep((int)timeToSleepMs);
            }
        }
    }

    /// <summary>
    /// Assigns a job to a free core.
    /// </summary>
    /// <returns>-1 if no free core.</returns>
    int AssignJob()
    {
        if(availableCores.Count > 0)
        {
            int core = availableCores.Dequeue();
            DateTimeOffset currentTime = new DateTimeOffset(DateTime.UtcNow);
            usedCoresDict.Add(core, currentTime);
            return core;
        }
        return ErrorNoFreeCores;
    }

    void ReturnCore(int coreNumber)
    {
        // prevents adding the same core twice
        if(usedCoresDict.ContainsKey(coreNumber))
        {
            DateTimeOffset currentTime = new DateTimeOffset(DateTime.UtcNow);
            long currentEpochMs = currentTime.ToUnixTimeMilliseconds();
            DateTimeOffset startTime = usedCoresDict[coreNumber];
            long startTimeEpochMs = startTime.ToUnixTimeMilliseconds();
            long elapsedTimeMs = startTimeEpochMs - currentEpochMs;

            
            availableCores.Enqueue(coreNumber, elapsedTimeMs);
            usedCoresDict.Remove(coreNumber);
        }
    }
}
