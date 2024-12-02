// See https://aka.ms/new-console-template for more information

using System.Collections;
using Inventory;
using System.Net.NetworkInformation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Console = System.Console;
using File = System.IO.File;

public class Program
{
    private static string COLOR_RED = "\x1b[91m";
    private static string COLOR_GREEN = "\x1b[92m";
    private static string COLOR_NORMAL = "\x1b[39m";

    public static bool IsConnectedToInternet()
    {
        try { 
            Ping myPing = new Ping();
            String host = "google.com";
            byte[] buffer = new byte[32];
            int timeout = 1000;
            PingOptions pingOptions = new PingOptions();
            PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
            return (reply.Status == IPStatus.Success);
        }
        catch (Exception) {
            return false;
        }
    }

    public static void PrintList(IEnumerable list)
    {
        foreach (var item in list)
        {
            Console.WriteLine(item);
        }
    }
    
    public static string CheckUser()
    {
        string path = @"../../../config.json";
        
        using (StreamReader configFile = new StreamReader(path))
        using (JsonTextReader reader = new JsonTextReader(configFile))
        {
            JObject json = (JObject)JToken.ReadFrom(reader);
            string isValidUser = (string)json.SelectToken("user");
            
            if (!isValidUser.Equals(""))
            {
                //user already set in config.json
                return (string)json["user"];
            }
            
            List<string> users = json["users"].ToObject<List<string>>();
            string whichUser = "Uh oh! We don't know you! Who are you?";
            
            string user = UserInput.AskQuestion(whichUser, users);
            json["user"] = user;
            
            configFile.Close();
            File.WriteAllText(path, json.ToString());
            return user;
        }
    }

    public static void AppFlow()
    {
        string currentUser = CheckUser();
        bool keepAppRunning = true;
       outer: while (keepAppRunning)
        {
            Console.Clear();
            string menuSelection = UserInput.ShowMainMenu();
            if (menuSelection == "Exit")
            {
                Environment.Exit(1);
            }
            else if (menuSelection == "View Inventory")
            {
                Console.WriteLine("VIEWING INVENTORY");
                Environment.Exit(1);
            }
            
            Console.Clear();
            List<string> devicesBeingUpdated = UserInput.GetServiceTags().Cast<string>().ToList();
            List<string> devicesToRemove = new List<string>();
            Dictionary<string, string> devicesServiceAndId = new Dictionary<string, string>();
            
            foreach (string serviceTag in devicesBeingUpdated)
            {
                string deviceId = Freshservice.GetDeviceId(serviceTag);
                if (string.IsNullOrEmpty(deviceId)  || deviceId.StartsWith("Error"))
                {
                    devicesToRemove.Add(serviceTag);
                    continue;
                }
                
                devicesServiceAndId.Add(serviceTag, deviceId);
            }

            if (devicesToRemove.Count > 0)
            {
                Console.WriteLine("The following devices could not be found in FS. They are being removed: " + string.Join(", ", devicesToRemove));
                Console.WriteLine("Press any key to continue...");
                Console.ReadLine();
            }

            if (devicesServiceAndId.Count == 0)
            {
                Console.WriteLine("No service tags detected. Hit any key to return to menu.");
                Console.ReadLine();
                continue;
            }
                
            Console.Clear();
            
            if (menuSelection == "Add to Inventory")
            {
                Console.WriteLine("ADD TO INVENTORY");
            }
            else if (menuSelection == "Assign Inventory")
            {
                long employeeId = 0;
                string assigneeEmail = string.Empty;
                bool needEmployeeId = true;
                while (needEmployeeId)
                { 
                    
                    Console.Clear();
                    Console.WriteLine("Please provide the email or email username of user (e.g. mforet or mforet@abskids.com)");
                    Console.CursorVisible = true;
                    assigneeEmail = Console.ReadLine();
                    if (assigneeEmail.Contains("@abskids.com"))
                    {
                        assigneeEmail = assigneeEmail.Replace("@abskids.com", "");
                    }
                    
                    Console.CursorVisible = false;
                    employeeId = Freshservice.GetEmployeeId(assigneeEmail);
                    
                    if (!(employeeId == 0))
                    {
                        break;
                    }          
                    
                    Console.Clear();
                    Console.WriteLine($"Error: Could not find {assigneeEmail}@abskids.com. Hit esc to return to menu. Hit any other key to try again.");
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape)
                    {
                        goto outer;
                    }
                }
                Console.Clear();
                
                foreach (string device in devicesServiceAndId.Keys)
                {
                    bool deviceUpdated = Freshservice.AssignDevice(devicesServiceAndId[device], employeeId);

                    if (deviceUpdated == false)
                    {
                        Console.WriteLine($"{COLOR_RED}{device} failed to update{COLOR_NORMAL}");
                        continue;
                    }
                    Console.WriteLine($"{COLOR_GREEN}{device} assigned to {assigneeEmail}@abskids.com{COLOR_NORMAL}\n");
                    Console.WriteLine("Press any key to return to menu");
                    Console.ReadLine();
                }
            }
        }
    }
    
    public static void Main()
    {

        if (!IsConnectedToInternet())
        {
            Console.WriteLine("Not connected to the internet. Exiting...");
            Environment.Exit(1);
        }
        
        AppFlow();
        
        //Console.WriteLine(Webscraping.getDeviceModel("1688C54"));
        
        Environment.Exit(1);
    }
}