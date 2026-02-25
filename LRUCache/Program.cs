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
namespace LRUCache
{
    public class LRUCache
    {
        /// <summary>
        /// key - cache slot number
        /// data - cached data
        /// </summary>
        private Dictionary<int, string> cacheSlots;

        /// <summary>
        /// Linked list that keeps track of the LRU cache slots. Ordered by oldest to newest
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
                this.LRUSlots.Remove(key);
            }
            else // may need to remove LRU entry
            {
                if(this.cacheSlots.Count == this.numberOfSlots)
                {
                    this.LRUSlots.RemoveFirst();
                }
            }
            this.LRUSlots.Append(key);
            this.cacheSlots[key] = value;
        }

        public string Get(int key)
        {
            if(this.cacheSlots.ContainsKey(key))
            {
                return this.cacheSlots[key];
            }
            return String.Empty;
        }
    }
}
