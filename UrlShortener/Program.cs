/*
Problem: Create a URL shortner where which converts a long url to a short url.

Example:

Input:
https://search.brave.com/search?q=c%23+no+projects+found+inthe+worspace&source=desktop&summary=1&conversation=08b33d55b154d870a80fe1ad444532ab793f

Output:
https://www.tinyurl.com/xxxxxxxx

Constraints:
- The hash must be a minimum size of 8 characters
- If there is a hash collision, it needs to be handled
- a URL must always map to the same hash

*/

using System.Security.Cryptography;
using System.Text;

 internal class Program
 {
     static void Main(string[] args)
     {
        UrlShortner urlShortner = new UrlShortner();

        while(true)
        {
            Console.WriteLine("0 to generate a tiny url. 1 to use a hash to retrieve the full url");
            int choice = Convert.ToInt16(Console.ReadLine());
            if(choice == 0)
            {
                Console.WriteLine("Enter the url to be converted");
                string url = Convert.ToString(Console.ReadLine());
                string hash = urlShortner.HashLongUrlAndStore(url);
                Console.WriteLine(hash);
            }
            else if(choice == 1)
            {
                Console.WriteLine("Enter the hash to retrieve the full url");
                string hash = Convert.ToString(Console.ReadLine());
                string url = urlShortner.GetLongUrlFromHash(hash);
                Console.WriteLine(url);
            }
            else
            {
                Console.WriteLine("Invalid Choice selected");
            }
        }
     }
 }

public class UrlShortner
{
    const string tinyUrlBase = "https://www.tinyurl.com/";
    const string tinyUrlErrorPage = tinyUrlBase + "errorpage";
    const int minimumHashSize = 8;
    Dictionary<string, string> HashToLongUrl;

    public UrlShortner()
    {
        HashToLongUrl = new Dictionary<string, string>();
    }

    public string HashLongUrlAndStore(string url)
    {
        // validate
        if(String.IsNullOrEmpty(url))
        {
            // invalid input
            return tinyUrlErrorPage;
        }

        // compute hash
        string hash = ComputeHashFromUrl(url);

        // check and return result
        if(String.IsNullOrEmpty(hash))
        {
            return tinyUrlErrorPage;
        }
        return tinyUrlBase + hash;
    }

    public string GetLongUrlFromHash(string hash)
    {
        // validate
        if(String.IsNullOrEmpty(hash) || hash.Length < minimumHashSize)
        {
            return tinyUrlErrorPage;
        }

        // check if dictionary contains this hash
        if(HashToLongUrl.ContainsKey(hash))
        {
            return HashToLongUrl[hash];
        }
        return tinyUrlErrorPage;
    }

    private string ComputeHashFromUrl(string url)
    {
        string hash = String.Empty;

        using(MD5 md5Hash = MD5.Create())
        {
            // get hash in form of bytes
            byte[] bytes = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(url));

            // convert hash to hex
            StringBuilder sb = new StringBuilder();
            foreach(byte b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }

            // Take first 8 characters and check if it exists in our HashMap
            string fullHash = sb.ToString();
            hash = fullHash.Substring(minimumHashSize);

            for(int i = minimumHashSize; i < fullHash.Length; i++)
            {
                hash = fullHash.Substring(i);
                if(HashToLongUrl.ContainsKey(hash) && HashToLongUrl[hash] == url)
                {
                    //no-op as this entry has already been added in a
                    //previous iteration. Just return hash0
                    Console.WriteLine("url has been shortened before.");
                    return hash;
                }
                else if(!HashToLongUrl.ContainsKey(hash))
                {
                    Console.WriteLine("new url to shorten.");
                    HashToLongUrl[hash] = url;
                    return hash;
                }
            }

            //we have the exact same MD5 has as another URL. Highlighly unlikely we get to this point
            // we could add some logging in to detect this and fire alerts so that it can be handled in 1 or 2 manners
            //1. move to SHA256 (all existing links will be come invalid. mixed mode situation)
            //2. append a second hash to the hash compute by MD5. (existing links still work and no mixed mode)
        }
        return hash;
    }
}
