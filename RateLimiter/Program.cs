// Challenge: Implemment a rate limiter
// For this problem, I need to talk about all the options before starting
//
// Option 1
// Fixed window
//  - maintain a count for each user
//  - resets every minute/ time period
//
// This works but it is bursty at boundaries.
//
// Option 2
// Sliding window
//  - store timestamps for each request
//  - requires more memory
//      - if there is a surge in traffic for many users -> we could end up storing way to many timestamps
//      - One way to mitigate this is that we cap how many timestamps we store for each user.
//          - ex. Once we have 100 timestamps -> don't store any more until the oldest have aged out
//
// This is better as now we are ratelimiting on a sliding window basis versus a fixed window
//  - however, we are still subjugated to change of bursty traffic.
//      - ex. 
//          at t0 100 requests come from User 1
//              - the queue holding these timestamps is now full and will only reset once the window has elapsed
//
// Option 3
// Token bucket
//  - Each user is given a Token bucket (this will be represented as an integer where the value indicates the number of tokens)
//  - when a request comes in -> a token is removed from the bucket
//      - if no tokens remain in the bucket -> request is ratelimited
//  - as time passes -> tokens are slowly added back to the bucket
//      - ex.
//          at T0 (0): bucket has 100 tokens -> 10 user requests come in -> we now have 90 tokens remaining
//              - the token fill rate is 10 tokens every 6 seconds
//          at T1 (6): 10 tokens are added to the bucket -> the bucket now again has the max number of tokens
//
//  - This also solves the problem of bursty traffic
//      - ex. (refresh rate is 10 tokens eveny 6 seconds)
//          at T0 (0): bucket has 100 tokens -> 100 user request come in within a couple of seconds -> 0 tokens remaining
//          at T1 (3): bucket has 0 tokens -> 10 user requests come in -> all requests are ratelimited
//          at T2 (6): bucket has 0 tokens -> 10 tokens are added to the bucket -> bucket has 10 tokens
//          at T3 (7): bucket has 10 tokens -> 10 user requests come in -> all requests are served
//          - this pattern can repeat which allows for traffic to still trickle at the rate limit has been reached
//          - this can save our service WHILE also providing a usable experience for our client

namespace RateLimiterProgram
{
    class Program
    {
        static void Main(string[] args) // TODO: also do testing using NUnit
        {
            // Initialize the RateLimiter
            int limit = 100;
            TimeSpan window = new TimeSpan(0, 0, 0, 1);
            RateLimiter rateLimiter = new RateLimiter(limit, window);
            DateTimeOffset dto = new DateTimeOffset(DateTime.UtcNow);
            DateTimeOffset dto2 = new DateTimeOffset(DateTime.UtcNow);
            dto2 = dto2.AddSeconds(2);

            long epochMs = dto.ToUnixTimeMilliseconds();
            long epochMs2 = dto2.ToUnixTimeMilliseconds();


            int success = 0;
            int failure = 0;

            // Send 105 requests to rate limiter for "usera" -> 100 should pass -> 5 should fail
            for(int i = 0; i < 105; i++)
            {
                if(rateLimiter.Allow("usera", epochMs))
                {
                    success++;
                }
                else
                {
                    failure++;
                }
            }

            Console.WriteLine($"success:{success}");
            Console.WriteLine($"failure:{failure}");

            Thread.Sleep(1005);

            // Send 105 requests to rate limiter for "usera" -> 100 should pass -> 5 should fail
            success = 0;
            failure = 0;
            for(int i = 0; i < 105; i++)
            {
                if(rateLimiter.Allow("usera", epochMs2))
                {
                    success++;
                }
                else
                {
                    failure++;
                }
            }

            Console.WriteLine($"success:{success}");
            Console.WriteLine($"failure:{failure}");

        }
    }
}

/// <summary>
/// RateLimter which implements Token bucket algorithm
/// </summary>
sealed class RateLimiter
{
    private readonly int limit;
    private readonly int numberOfBuckets;
    private readonly int refillRate;
    private readonly long refillIntervalMs;
    private Dictionary<string, TokenBucket> clientIdToTokenBucket;

    public RateLimiter(int limit, TimeSpan window)
    {
        this.limit = limit;
        this.numberOfBuckets = 10; // this can be configurable if required
        this.refillRate = limit/numberOfBuckets;
        this.refillIntervalMs = (long)window.TotalMilliseconds/numberOfBuckets;
        this.clientIdToTokenBucket = new Dictionary<string, TokenBucket>();
    }

    public bool Allow(string clientId, long currentTimeEpochMs)
    {
        DateTimeOffset dto = new DateTimeOffset(DateTime.UtcNow);
        long epochMs = dto.ToUnixTimeMilliseconds();

        if(!clientIdToTokenBucket.ContainsKey(clientId))
        {
            clientIdToTokenBucket[clientId] = new TokenBucket(epochMs , this.limit);
        }

        TokenBucket tokenBucket = clientIdToTokenBucket[clientId];

        tokenBucket.AddTokensToBucket(currentTimeEpochMs,
            this.refillIntervalMs,
            this.refillRate,
            this.limit,
            this.numberOfBuckets);
        
        bool isAllowed = tokenBucket.RemoveTokenFromBucket();
        return isAllowed;
    }
}
class TokenBucket
{
    /// <summary>
    /// the last time that a request came in for this TokenBucket
    /// </summary>
    public long lastRequestTimeEpochMs;

    /// <summary>
    /// the number of tokens in the bucket
    /// </summary>
    public int numOfTokens;

    public TokenBucket(
        long lastRequestTimeEpochMs,
        int numOfTokens)
    {
        this.lastRequestTimeEpochMs = lastRequestTimeEpochMs;
        this.numOfTokens = numOfTokens;
    }

    /// <summary>
    /// Helper method to add tokens to the bucket
    /// </summary>
    /// <param name="currentTimeEpochMs"></param>
    /// <param name="refillIntervalMs"></param>
    /// <param name="refillRate"></param>
    /// <param name="limit"></param>
    /// <param name="numberOfBuckets"></param>
    public void AddTokensToBucket(
        long currentTimeEpochMs,
        long refillIntervalMs,
        int refillRate,
        int limit,
        int numberOfBuckets)
    {
        if(this.lastRequestTimeEpochMs < currentTimeEpochMs - refillIntervalMs)
        {
            long numberOfRefills = (currentTimeEpochMs - this.lastRequestTimeEpochMs)/refillIntervalMs;

            // this gives our number of refills an upper bound.
            // ex. if hours go by -> the number of refills will supercede the size of the bucket
            numberOfRefills = Math.Min(numberOfRefills, limit/numberOfBuckets);

            this.numOfTokens = Math.Min(
                limit,
                this.numOfTokens + (int)(refillRate * numberOfRefills));

            this.lastRequestTimeEpochMs = currentTimeEpochMs;
        }
    }

    public bool RemoveTokenFromBucket()
    {
        if(this.numOfTokens > 0)
        {
            this.numOfTokens--;
            return true;
        }
        return false;
    }
}