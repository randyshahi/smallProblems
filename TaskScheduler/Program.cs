/*

Create a task scheduler

*/

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
    private PriorityQueue<int, long> availableWorkers;

    /// <summary>
    /// Keeps track of how much time each core has spend during execution
    /// </summary>
    private Dictionary<int, long> workerToExecutionTimeDict;

    /// <summary>
    /// Keeps track of which cores are currently running jobs paired with the timestamp of when the job started.
    /// </summary>
    private Dictionary<int, DateTimeOffset> workersWithRunningJobsDict;

    private PriorityQueue<int, long> jobsWithExpiryQueue;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="numberOfCores"></param>
    TaskScheduler(int numberOfCores)
    {
        this.totalCores = numberOfCores;
        this.availableWorkers = new PriorityQueue<int, long>();
        this.workerToExecutionTimeDict = new Dictionary<int, long>();
        this.workersWithRunningJobsDict = new Dictionary<int, DateTimeOffset>();
        this.jobsWithExpiryQueue = new PriorityQueue<int, long>();

        for(int i = 0; i < numberOfCores; i++)
        {
            availableWorkers.Enqueue(i, 0);
            workerToExecutionTimeDict[i] = 0;
        }

        this.Initialize();
    }

    void Initialize()
    {
        Thread backgroundThread = new Thread(Process);
        backgroundThread.IsBackground = true;
        backgroundThread.Start();
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
                int worker = jobsWithExpiryQueue.Dequeue();
                if(this.workersWithRunningJobsDict.ContainsKey(worker))
                {
                    // remove from dict and add back into pool. Need to also keep track of execution time
                    DateTimeOffset startTime = this.workersWithRunningJobsDict[worker];
                    availableWorkers.Enqueue(worker, startTime.ToUnixTimeMilliseconds() + TimeoutMs);
                    workersWithRunningJobsDict.Remove(worker);
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
        if(availableWorkers.Count > 0)
        {
            int worker = availableWorkers.Dequeue();
            DateTimeOffset currentTime = new DateTimeOffset(DateTime.UtcNow);
            workersWithRunningJobsDict.Add(worker, currentTime);
            return worker;
        }
        return ErrorNoFreeCores;
    }

    void ReturnCore(int worker)
    {
        // prevents adding the same core twice
        if(workersWithRunningJobsDict.ContainsKey(worker))
        {
            DateTimeOffset currentTime = new DateTimeOffset(DateTime.UtcNow);
            long currentEpochMs = currentTime.ToUnixTimeMilliseconds();

            DateTimeOffset startTime = workersWithRunningJobsDict[worker];
            long startTimeEpochMs = startTime.ToUnixTimeMilliseconds();

            long elapsedTimeMs = startTimeEpochMs - currentEpochMs;

            availableWorkers.Enqueue(worker, elapsedTimeMs);
            workersWithRunningJobsDict.Remove(worker);
        }
        else
        {
            // a worker as attempted to be returned to the pool but has already been returned
        }
    }
}
