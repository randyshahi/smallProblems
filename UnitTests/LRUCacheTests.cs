namespace LRUCacheTests
{
    using System.Runtime;
    using LRUCache;
    public class LRUCacheTests
    {
        private LRUCache lruCache;

        [Test]
        public void TestAllValuesCanBeAccessed()
        {
            this.lruCache = new LRUCache(100);

            for(int i = 0; i < 100; i++)
            {
                this.lruCache.Put(i, i.ToString());
            }

            for(int i = 0; i < 100; i++)
            {
                Assert.That(this.lruCache.Get(i) == i.ToString(), Is.True);
            }
        }

        [Test]
        public void TestThatCorrectEntriesAreRemoved()
        {
            this.lruCache = new LRUCache(100);

            for(int i = 0; i < 105; i++)
            {
                this.lruCache.Put(i, i.ToString());
            }

            for(int i = 5; i < 100; i++)
            {
                Assert.That(this.lruCache.Get(i) == i.ToString(), Is.True);
            }

            for(int i = 0; i < 5; i++)
            {
                Assert.That(this.lruCache.Get(i) == String.Empty, Is.False);
            }
        }
    }
}

