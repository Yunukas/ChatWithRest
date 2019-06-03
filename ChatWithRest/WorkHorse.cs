using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace ChatWithRest
{
    // this is a static class for performing chat tasks such as
    // get chat messages, block users, search etc.
    public static class WorkHorse
    {
        // chat board URL
        private const string CHAT_URL = "https://chat-ucla.herokuapp.com/chats";
        // direct message URL
        private const string DM_URL = "https://chat-ucla.herokuapp.com/direct_message";

        // list of all chat at the chat board
        public static List<Chat> ChatList { get; private set; }  = new List<Chat>();

        // list of DMs between active user and the selected receiver
        public static List<DirectMessage> DMChatList { get; private set; } = new List<DirectMessage>();

        // dictionary of blocked users
        public static Dictionary<string, List<string>> BlockedUsersDict = new Dictionary<string, List<string>>();

        public static string DMReceiver { get; set; }

        // get all of the chat at the chat board
        public static async Task<List<Chat>> GetAllChatAsync()
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

        // get the DMs of active user
        public static async Task<List<DirectMessage>> GetDMsAsync(string sender, string receiver)
        {
            string json = "";
            // used by Visual Studio to create socket connections
            HttpClient client = new HttpClient();
            // Makes request to cat facts
            HttpResponseMessage response = await client.GetAsync(DM_URL + "?sender=" + sender + "&receiver=" + receiver);
            // Not required, but checks if status code is successful. Other status codes are errors or page not found.
            if (response.IsSuccessStatusCode)
            {
                json = await response.Content.ReadAsStringAsync();

                DeserializeDMs(json);
                // print JSON response
                //Console.WriteLine(await response.Content.ReadAsStringAsync());
            }

            return DMChatList;
        }

        // this method will deserialize chat json string
        public static void DeserializeDMs(string json)
        {
            DMChatList.Clear();

            JArray jarr = JArray.Parse(json);

            // get the results fragment of the json as a list of JTokens
            List<JToken> results = jarr.Children().ToList();

            // populate the list
            foreach (JToken result in results)
            {
                DirectMessage dm = result.ToObject<DirectMessage>();
                DMChatList.Add(dm);
            }
        }

        public static void ClearDMs()
        {
            DMChatList.Clear();
        }

        // post a chat to the chat board
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

        // post a chat to the chat board
        public static async Task<List<DirectMessage>> PostDMAsync(string sender, string receiver, string message)
        {

            // used by Visual Studio to create socket connections
            HttpClient client = new HttpClient();

            // parameters for POSTing data to url
            var @params = new FormUrlEncodedContent(new[]
            {
                   new KeyValuePair<string, string>("sender", sender),
                   new KeyValuePair<string, string>("receiver", receiver),
                   new KeyValuePair<string, string>("message", message)
            });

            // POST to URL
            HttpResponseMessage response = await client.PostAsync(DM_URL, @params);

            // Read response
            string json = await response.Content.ReadAsStringAsync();

            DeserializeDMs(json);

            return DMChatList;
        }

        // read the blocked.txt file and get all information
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

        // get the list of users who are blocked by the active user
        public static List<string> GetBlockedUsers(string mainUser)
        {
            foreach (KeyValuePair<string, List<string>> kv in BlockedUsersDict)
            {
                if (kv.Key.Equals(mainUser))
                {
                    return kv.Value;
                }
            }

            return null;
        }

        // check if a user is blocked by the active user
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

            UpdateBlockedTextFile();
        }

        // unblocking a user
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

            UpdateBlockedTextFile();
        }
        // this method will write blocked user list to the blocked.txt file
        public static void UpdateBlockedTextFile()
        {
            // serialize the dictonary
            string serializedJson = JsonConvert.SerializeObject(BlockedUsersDict);

            // write back to the file
            using (StreamWriter sr = new StreamWriter(Directory.GetCurrentDirectory().ToString() + "\\blocked.txt"))
            {
                sr.Write(serializedJson);
            }
        }

        // this method will search through the List of Chat objects
        // and return the results containing the search key
        // we are searching in both messages and sender fields
        public static List<Chat> SearchChat(string searchKey)
        {
            var result = ChatList.Where(x => x.Fields.Message.Contains(searchKey) ||
            x.Fields.Username.Contains(searchKey)).Select(y => y).ToList();

            return result;
        }
    }
}
