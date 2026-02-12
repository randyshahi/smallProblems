/*
This project is serve as a guide so that I can refresh my mind before doing an interview in C#
*/

using System.Security.Cryptography;
using System.Text;

// Stack
Stack<string> nameStack = new Stack<string>();
nameStack.Push("randeep");
nameStack.Push("shahi");
nameStack.Push("zyhra");

nameStack.Peek(); //zyhra
nameStack.Pop(); //zyhra
nameStack.Pop(); //shahi

// Queue
Queue<int> queue = new Queue<int>();
queue.Enqueue(1);
queue.Enqueue(2);
queue.Enqueue(3);

queue.Dequeue(); // 1
queue.Dequeue(); // 2
queue.Dequeue(); // 3

int size = queue.Count; // the queue is empty so 0;

// PriorityQueue (PriorityQueue in C# is a min-heap by default)
PriorityQueue<string, int> pq = new PriorityQueue<string, int>();
pq.Enqueue("task1", 50);
pq.Enqueue("task2", 10);
pq.Enqueue("task3", 40);

pq.Dequeue(); // "task2"

// max-heap (honestly, super ugly)
PriorityQueue<string, int> pqMaxHeap = new PriorityQueue<string, int>(Comparer<int>.Create((x, y) => y.CompareTo(x)));
pqMaxHeap.Enqueue("task1", 1);
pqMaxHeap.Enqueue("task2", 100);
pqMaxHeap.Enqueue("task3", 50);

pqMaxHeap.Dequeue(); // "task2"
pqMaxHeap.Dequeue(); // "task3"
pqMaxHeap.Dequeue(); // "task1"

// array
int[] values = new int[10]; // creates an array of length 10
values[5] = 10;

int[] valuesFilledIn = new int[]{ 1, 2, 3, 4, 5};
int[] valuesFilledIn2 = new int[3] {1, 2 ,3};

int[] concat = (int [])valuesFilledIn.Concat(valuesFilledIn2);

double average = valuesFilledIn.Average();

int[,] matrix = new int[4,5]; // creates a matrix that is 4 x 5 (rows x columns)
matrix[1,1] = 100;

// string
string name = "randeepshahi";
string firstName = name.Substring(0, 7); //randeep
string lastName = name.Substring(7); //shahi
int length = name.Length; // the number of characters in the string.

// iterates through each character in a string
for(int i = 0; i < name.Length; i++)
{
    char c = name[i];
}

// list
List<int> listOfInts = new List<int>(); // no size specified, we can add any amount of elements to this list
for(int i = 0; i < listOfInts.Count; i++)
{
    int value = listOfInts[i];
}

// string builder
StringBuilder sb = new StringBuilder();
sb.Append('c'); // can append characters
sb.Append("string"); // can also apend strings
sb.ToString(); // cstring

StringBuilder sb2 = new StringBuilder();
sb2.AppendJoin(",", new string[] {"word1", "word2"});
sb2.ToString(); // "word1,word2"

// dictionary (used to create hashmaps)
Dictionary<string, string> dict = new Dictionary<string, string>();
dict["randeep"] = "shahi";

if(dict.ContainsKey("randeep")) // O(1)
{
    // checks if the dictionary contains key = "randeep"
}
if(dict.ContainsValue("shahi")) // O(n)
{
    // checks if the dictionary contains value = "shahi
}

// set (called a HashSet in C# for no real reason...)
HashSet<int> numbers = new HashSet<int>();
numbers.Add(1);
numbers.Add(2);
numbers.Add(3);
numbers.Add(4);
numbers.Add(5);

if(numbers.Contains(1))
{
    // do something
}

if(!numbers.Contains(6))
{
    // do something
}

// MD5 hashing example
using(MD5 md5 = MD5.Create()) // creates a MD5 object so that we can compute hashs.
{
    string stringToHash = "randeepshahi";
    byte[] stringToHashInBytes = Encoding.UTF8.GetBytes(stringToHash);
    byte[] bytes = md5.ComputeHash(stringToHashInBytes); // we now have our hash in bytes

    // to convert it to hex
    StringBuilder sbForHash = new StringBuilder();
    foreach(byte b in bytes)
    {
        // "x2" - x means hex, and 2 means the precision (aka always 2 hex for each bytes)
        sbForHash.Append(b.ToString("x2")); 
    }
    
    string hashInHex = sbForHash.ToString();
}