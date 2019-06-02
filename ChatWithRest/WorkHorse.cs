using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace chat
{
    // this is a static class for performing chat tasks such as
    // get chat messages, block users, search etc.
    public static class WorkHorse
    {
        private const string CHAT_URL = "https://chat-ucla.herokuapp.com/chats";
        private const string DM_URL = "https://chat-ucla.herokuapp.com/direct_message";

        public static List<Chat> ChatList = new List<Chat>();


        public static Dictionary<string, List<string>> BlockedUsersDict = new Dictionary<string, List<string>>();

        public static async Task<List<Chat>> GetChat()
        {
            string json = "";
            // used by Visual Studio to create socket connections
            HttpClient client = new HttpClient();
            // Makes request to cat facts
            HttpResponseMessage response = await client.GetAsync(CHAT_URL);
            // Not required, but checks if status code is successful. Other status codes are errors or page not found.
            if (response.IsSuccessStatusCode)
            {
                json = await response.Content.ReadAsStringAsync();

                DeserializeChat(json);
                // print JSON response
                //Console.WriteLine(await response.Content.ReadAsStringAsync());
            }

            return ChatList;
        }

        // this method will deserialize chat json string
        public static void DeserializeChat(string json)
        {
            // first clear the List of Chats
            ChatList.Clear();

            JArray jarr = JArray.Parse(json);

            // get the results fragment of the json as a list of JTokens
            List<JToken> results = jarr.Children().ToList();

            // populate the list
            foreach (JToken result in results)
            {
                Chat chat = result.ToObject<Chat>();
                ChatList.Add(chat);
            }
        }

        public static async Task<List<Chat>> PostChatAsync(string userName, string message)
        {

            // used by Visual Studio to create socket connections
            HttpClient client = new HttpClient();

            // parameters for POSTing data to url
            var @params = new FormUrlEncodedContent(new[]
            {
                   new KeyValuePair<string, string>("username", userName),
                   new KeyValuePair<string, string>("message", message)
            });

            // POST to URL
            HttpResponseMessage response = await client.PostAsync(CHAT_URL, @params);

            // Read response
            string json = await response.Content.ReadAsStringAsync();

            DeserializeChat(json);

            return ChatList;
        }

        public static void GetAllBlockedUsers()
        {
            // firstly, clear the dictionary
            BlockedUsersDict.Clear();

            if (File.Exists(Directory.GetCurrentDirectory() + "\\blocked.txt"))
            {

                string json = "";

                using (StreamReader sr = new StreamReader(Directory.GetCurrentDirectory() + "\\blocked.txt"))
                {
                    json = sr.ReadToEnd();
                }

                var blocked = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);

                foreach (KeyValuePair<string, List<string>> kv in blocked)
                {
                    BlockedUsersDict.Add(kv.Key, kv.Value);
                }
            }

            return;
        }

        public static List<string> GetBlockedUsers(string mainUser)
        {
            //GetAllBlockedUsers();

            foreach (KeyValuePair<string, List<string>> kv in BlockedUsersDict)
            {
                if (kv.Key.Equals(mainUser))
                {
                    return kv.Value;
                }
            }

            return null;
        }

        public static bool IsBlocked(string mainUser, string user)
        {
            foreach (KeyValuePair<string, List<string>> kv in BlockedUsersDict)
            {
                if (kv.Key.Equals(mainUser))
                {
                    if (kv.Value.Contains(user))
                        return true;
                }
            }

            return false;
        }
        // this method will add a user to the list of blocked users for the active user
        // it will also write to the blocked.txt file
        public static void BlockUser(string mainUser, string user)
        {
            if (IsBlocked(mainUser, user))
                return;


            if (BlockedUsersDict.ContainsKey(mainUser))
                BlockedUsersDict[mainUser].Add(user);
            else
                BlockedUsersDict.Add(mainUser, new List<string> { user });

            WriteToBlockedTextFile();
        }

        public static void UnblockUser(string mainUser, string user)
        {
            foreach (KeyValuePair<string, List<string>> kv in BlockedUsersDict)
            {
                if (kv.Key.Equals(mainUser))
                {
                    if (kv.Value.Contains(user))
                        kv.Value.Remove(user);
                }
            }

            WriteToBlockedTextFile();
        }

        public static void WriteToBlockedTextFile()
        {
            // serialize the dictonary
            string serializedJson = JsonConvert.SerializeObject(BlockedUsersDict);

            // write back to the file
            using (StreamWriter sr = new StreamWriter(Directory.GetCurrentDirectory().ToString() + "\\blocked.txt"))
            {
                sr.Write(serializedJson);
            }
        }
    }
}
