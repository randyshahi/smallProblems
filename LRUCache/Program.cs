// LRU Cache
// Core requirement:
//  - get(key) in O(1)
//  - put(key, value) in O(1)
//  - Evict least recently used item when capacity exceeded
//
// Breakdown:
//  - get() and put() are both O(1)
//      - a hash map satisfies both these criteria
//  - Evict LRU items when capacity exceeded
//      - capacity -> our LRU has a fixed size
//      - Evict LRU items -> need a way to keep track of LRU Items
//          - Idea 1 -> doubley Linked List
//              - O(1) to figure out which item should be evicted -> not a problem
//              - O(n) + O(1) to check if cache slot exists and to move it to the front -> can be a problem for big n
//                  - If we are assuming an L1 cache -> num of cache slots will be in the 100's
//                  - If we are assuming L2 cache is in the thousands so we will start to feel the O(n)
//                  - If we are assuming L3 cache then we need a further abstraction like (sets and ways)
//                      - meaning -> all slots are mapped to groups -> aka we first hash which group of slots the data
//                          should be cached at (not a true hash but rather take the middle bits of a memory address) 
//                              -> then we apply our algorithm
//                      - analogy of a hotel -> floors == groups AND rooms == cache slots AND middle 3 digits of passport to
//                          determine floor

public class LRUCache
{
    /// <summary>
    /// mapping from cacheslot number to cached data
    /// </summary>
    private Dictionary<int, string> cacheSlots;

    /// <summary>
    /// Linked list that keeps track of the LRU cache slots
    /// </summary>
    private LinkedList<int> LRUSlots;

    /// <summary>
    /// number of slots that our cache has
    /// </summary>
    private int numberOfSlots;

    public LRUCache(int numberOfSlots)
    {
        this.numberOfSlots = numberOfSlots;
        this.cacheSlots = new Dictionary<int, string>();
        this.LRUSlots = new LinkedList<int>();
    }

    public void Put(int key, string value)
    {
        if(cacheSlots.ContainsKey(key))
        {
            LRUSlots.Remove(key); // update LRU List

            cacheSlots[key] = value; // update value     
            LRUSlots.AddFirst(key);
        }
        // new key
        else
        {
            if(IsCacheFull())
            {
                RemoveLRUEntry();
            }

            // Add new entry
            cacheSlots[key] = value;
            LRUSlots.AddFirst(key);

        }
    }

    public string Get(int key)
    {
        if(cacheSlots.ContainsKey(key))
        {
            // update LRU list

            // return value
            return cacheSlots[key];
        }
    }

    /// <summary>
    /// Removes LRU entry
    /// </summary>
    private void RemoveLRUEntry()
    {
        int oldKey = LRUSlots.Last();
        LRUSlots.Remove(oldKey);
        cacheSlots.Remove(oldKey);
    }

    private bool IsCacheFull()
    {
        return cacheSlots.Count == this.numberOfSlots;
    }

    private void AddEntry(int key, string value)
    {
        cacheSlots[key] = value;
        LRUSlots.AddFirst(key);
    }
}
