using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TestCrypto
{
    class Crytography
    {
        private static Dictionary<char, int> charDictionary = new Dictionary<char, int>()
            {
                {'0', 0}, {'1', 1}, {'2', 2}, {'3', 3}, {'4', 4}, {'5', 5},
                {'6', 6}, {'7', 7}, {'8', 8}, {'9', 9}, {'A', 10}, {'B', 11},
                {'C', 12}, {'D', 13}, {'E', 14}, {'F', 15}, {'G', 16}, {'H', 17},
                {'I', 18}, {'J', 19}, {'K', 20}, {'L', 21}, {'M', 22}, {'N', 23},
                {'O', 24}, {'P', 25}, {'Q', 26}, {'R', 27}, {'S', 28}, {'T', 29},
                {'U', 30}, {'V', 31}, {'W', 32}, {'X', 33}, {'Y', 34}, {'Z', 35},
                {'a', 36}, {'b', 37}, {'c', 38}, {'d', 39}, {'e', 40}, {'f', 41},
                {'g', 42}, {'h', 43}, {'i', 44}, {'j', 45}, {'k', 46}, {'l', 47},
                {'m', 48}, {'n', 49}, {'o', 50}, {'p', 51}, {'q', 52}, {'r', 53},
                {'s', 54}, {'t', 55}, {'u', 56}, {'v', 57}, {'w', 58}, {'x', 59},
                {'y', 60}, {'z', 61}, {'=', 62}, {'+', 63}, {'/', 64}
            };
        private static string PublicKey = null;
        private static Dictionary<string, string> PrivateKey = null;
        private static Random random = new Random();

        public static bool Init(Int32 keyLength = 1024)//Must be 512, 1024, 2048, ‭4096‬
        {
            if (keyLength == 512 || keyLength == 1024 || keyLength == 2048 || keyLength == 4096)
            {
                int div1 = keyLength / 8;
                int div2 = div1 / 8;
                PublicKey = RandomString(keyLength / 8);
                PrivateKey = GetPrivateKey(PublicKey);
                return true;
            }
            return false;
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static IEnumerable<string> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }

        public static Dictionary<string, string> GetPrivateKey(string MyPublicKey)
        {
            int div1 = MyPublicKey.Length;
            int div2 = div1 / 8;
            IEnumerable<string> tab = Split(MyPublicKey, div2);
            string intitules = "ABCDEFGH";
            int partno = 0;
            Dictionary<string, string> pk = new Dictionary<string, string>();
            foreach (string segment in tab)
            {
                pk[intitules[partno].ToString()] = segment;
                partno += 1;
            }
            return pk;
        }

        public static string GetPublicKeyString()
        {
            return PublicKey;
        }


        public static string Encrypt(string toEncrypt, string key = null)
        {
            if (toEncrypt == null) { return null; }
            if (toEncrypt.Length == 0) { return null; }
            Dictionary<string, string> pk = null;
            if (key == null) { pk = PrivateKey; }
            else
            {
                if (key.Length == 0) { return null; }
                pk = GetPrivateKey(key);
            }
            Dictionary<string, string> ret = GetEndKey(pk);
            string end = ret["end"];
            string endpk = ret["endpk"];
            Console.WriteLine("pk = " + JsonConvert.SerializeObject(pk));
            Console.WriteLine("endpk = " + endpk);
            
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);
            string b64 = Convert.ToBase64String(toEncryptArray, 0, toEncryptArray.Length);

            int keymax = charDictionary.Count - 1;
            string nrt = "";
            int ikey = 0;
            int mkey = endpk.Length - 1;
            int reb = 0;
            //if (b64[b64.Length - 1] == '=') { reb = 1; }
            for (int i = 0; i < b64.Length - reb; i++)
            {
                char val = b64[i];
                Console.WriteLine("char["+i+"] = " + val);
                int num = charDictionary[val] + charDictionary[endpk[ikey]];
                if (num > keymax) { num -= keymax; }
                foreach (KeyValuePair<char, int> duo in charDictionary)
                {
                    if (duo.Value == num) { nrt += duo.Key;break; }
                }
                ikey++;
                if (ikey >= mkey) { ikey = 0; }
            }

            return nrt + end;
        }

        public static string Decrypt(string cipherString, string key = null)
        {
            if (cipherString == null) { return null; }
            if (cipherString.Length == 0) { return null; }
            string end = cipherString.Substring(cipherString.Length - 8);
            cipherString = cipherString.Substring(0, cipherString.Length - 8);
            Dictionary<string, string> pk = null;
            if (key == null) { pk = PrivateKey; }
            else
            {
                if (key.Length == 0) { return null; }
                pk = GetPrivateKey(key);
            }
            string endpk = GetEndKey(pk, end)["endpk"];

            int keymax = charDictionary.Count - 1;
            
	        string nrt = "";
	        int ikey = 0;
	        int mkey = endpk.Length - 1;
		    for(int i=0; i < cipherString.Length; i++){
		        char val = cipherString[i];
		        int num = charDictionary[val] - charDictionary[endpk[ikey]];
		        if(num < 0){num += keymax;}
                foreach (KeyValuePair<char, int> duo in charDictionary)
                {
                    if (duo.Value == num) { nrt += duo.Key; }
                }
		        ikey++;
		        if(ikey >= mkey){ikey=0;}
		        }

            byte[] toEncryptArray = Convert.FromBase64String(nrt);
            return UTF8Encoding.UTF8.GetString(toEncryptArray, 0, toEncryptArray.Length);
        }

        private static Dictionary<string, string> GetEndKey(Dictionary<string, string> pk, string end="") {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            string chars = "ABCDEFGH";
            int y = 0;
            while (end.Length != chars.Length)
            {
                y = random.Next(chars.Length);
                if (end.Contains(chars[y]) == false)
                {
                    end += chars[y];
                }
            }
            string endpk = "";
            foreach (char indice in end)
            {
                endpk += pk[indice.ToString()];
            }
            ret["end"] = end;
            ret["endpk"] = endpk;
            return ret;
        }
    }
}
