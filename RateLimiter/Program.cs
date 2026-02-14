// so now that we have buckets, how do we determine which bucket should be used?
//  also, when do refill the bucket with tokens??
//  Example:
//      TimeSpan = 14 minutes
//      numberOfBuckets = 14
//      
//      Each bucket corresponds to a minute
//      - seems like we need to keep track of a start time or start epoch
//          - lets say we do this. Now we know which bucket each request should go to
//      - no we have the issue where we need to determine when the tokens are put back into the bucket
//          - how do we do that?? Does something take care of this in the background??
//              - this background worker can create tasks to reset the value??

using System.Runtime.InteropServices;

sealed class RateLimiter
{
    private const int LastTimeStampIndex = 0;
    private const int TokenBucketIndex = 1;
    private readonly int limit;
    private readonly TimeSpan window;
    private readonly int numberOfBuckets;
    private readonly long refillRate;
    private readonly long refillIntervalMs;
    private Dictionary<string, long[]> clientIdToTokenBucket;

    RateLimiter(int limit, TimeSpan window)
    {
        this.limit = limit;
        this.window = window;
        this.numberOfBuckets = 10;
        this.refillRate = limit/numberOfBuckets;
        this.refillIntervalMs = (long)window.TotalMilliseconds/numberOfBuckets;
        this.clientIdToTokenBucket = new Dictionary<string, long[]>();
    }

    public bool Allow(string clientId, long currentTimeEpochMs)
    {
        DateTimeOffset dto = new DateTimeOffset(DateTime.UtcNow);
        long epochMs = dto.ToUnixTimeMilliseconds();

        if(!clientIdToTokenBucket.ContainsKey(clientId))
        {
            clientIdToTokenBucket[clientId] = new long[2] {epochMs , this.limit};
        }

        // check if tokens needs to be added to bucket
        if(clientIdToTokenBucket[clientId][LastTimeStampIndex] < currentTimeEpochMs - this.refillIntervalMs)
        {
            long numberOfRefills = (clientIdToTokenBucket[clientId][LastTimeStampIndex] - currentTimeEpochMs)/refillIntervalMs;
            numberOfRefills = Math.Min(numberOfRefills, limit/numberOfBuckets);

            clientIdToTokenBucket[clientId][TokenBucketIndex] = Math.Min(
                limit,
                clientIdToTokenBucket[clientId][TokenBucketIndex] + (this.refillRate * numberOfRefills));
        }

        // remove tokens from bucket
        if(clientIdToTokenBucket[clientId][TokenBucketIndex] > 0)
        {
            clientIdToTokenBucket[clientId][TokenBucketIndex]--;
            return true;
        }
        return false;
    }
}
