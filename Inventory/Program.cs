using System.Collections;
using Inventory;
using System.Net.NetworkInformation;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Console = System.Console;
using File = System.IO.File;
using Terminal.Gui;

public class Program
{
    
    private static string COLOR_RED = "\x1b[91m";
    private static string COLOR_GREEN = "\x1b[92m";
    private static string COLOR_NORMAL = "\x1b[39m";
    private static System.Media.SoundPlayer successSound = new System.Media.SoundPlayer(Environment.CurrentDirectory + @"\success.wav");

    private static bool IsConnectedToInternet()
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
    
    private static string CheckUser()
    {
        
        using (StreamReader configFile = new StreamReader(Environment.CurrentDirectory +@"/config.json"))
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
            File.WriteAllText(Environment.CurrentDirectory +@"/config.json", json.ToString());
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
            List<string> devicesBeingUpdated = UserInput.GetServiceTags().ToList();
            List<string> devicesNotInSystem = new List<string>();
            Dictionary<string, string> devicesInSystemAndId = new Dictionary<string, string>();
            
            foreach (string serviceTag in devicesBeingUpdated)
            {
                string deviceId = Freshservice.GetDeviceId(serviceTag);
                if (string.IsNullOrEmpty(deviceId)  || deviceId.StartsWith("Error"))
                {
                    devicesNotInSystem.Add(serviceTag);
                    continue;
                }
                
                devicesInSystemAndId.Add(serviceTag, deviceId);
            }

            if (devicesNotInSystem.Count > 0)
            {
                //CURRENTLY WORKING ON THIS 12/18 - giving the option to add devices that aren't found ##############################################################################
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The following devices could not be found in FS: " + string.Join(", ", devicesNotInSystem));
                Console.ForegroundColor = ConsoleColor.White;
                List<string> question = ["Yes", "No"];
                string answer = UserInput.AskQuestion("Would you like to add these devices to Freshservice? NOTE: They have to be Dell devices to work.", question, 1);
                if (answer == "No")
                {
                    goto ExitAddingToFresh;
                }

                foreach (string device in devicesNotInSystem)
                {
                    string model = DeviceInfo.GetDeviceModel(device);
                    Console.WriteLine($"{device} is a {model}");
                }
                Console.ReadLine();
            }
            
            ExitAddingToFresh:

            if (devicesInSystemAndId.Count == 0)
            {
                Console.WriteLine("No service tags detected. Hit any key to return to menu.");
                Console.ReadLine();
                continue;
            }
                
            Console.Clear();
            
            if (menuSelection == "Add to Inventory")
            {
                foreach (string device in devicesInSystemAndId.Keys)
                {
                    bool newDevice = true;
                    List<string> answers = ["New", "Used"];
                    string answer = UserInput.AskQuestion($"Is {device} a new or used device?", answers);
                    if (answer == "Used")
                    {
                        newDevice = false;
                    }
                    bool deviceUpdated = Freshservice.InventoryDevice(currentUser, devicesInSystemAndId[device], newDevice);
                    if (!deviceUpdated)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{device} could not be added to the inventory.");
                        Console.ForegroundColor = ConsoleColor.White;
                        continue;
                    }

                    successSound.Play();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{device} has been added to {currentUser}'s {answer} Inventory.");
                    Console.ForegroundColor = ConsoleColor.White;
                    
                }
                Console.WriteLine("\nInventory has been updated. Hit any key to return to menu.");
                Console.ReadLine();
                
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
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: Could not find {assigneeEmail}@abskids.com. Hit esc to return to menu. Hit any other key to try again.");
                    Console.ForegroundColor = ConsoleColor.White;
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape)
                    {
                        goto outer;
                    }
                }
                Console.Clear();
                
                foreach (string device in devicesInSystemAndId.Keys)
                {
                    bool deviceUpdated = Freshservice.AssignDevice(devicesInSystemAndId[device], employeeId, "Admin");

                    if (deviceUpdated == false)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{device} failed to update");
                        Console.ForegroundColor = ConsoleColor.White;
                        continue;
                    }
                    
                    successSound.Play();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{device} assigned to {assigneeEmail}@abskids.com");
                    System.Media.SoundPlayer sounds = new System.Media.SoundPlayer(Environment.CurrentDirectory + @"/success.wav");
                    sounds.Play();
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.WriteLine("\nPress any key to return to menu");
                Console.ReadKey(true);
            }
        }
    }
    
    public static void Main()
    {
        /*if (!IsConnectedToInternet())
        {

            Console.WriteLine("Not connected to the internet. Exiting...");
            Environment.Exit(1);
        }*/
        
        //AppFlow();

        //Freshservice.GetDeviceInfo("HJSX564");
        
        /*Freshservice.AddNewDevice("TEST123", "Admin", "In Stock", "OptiPlex 3070 Micro");
        DateTime now = DateTime.Now;
        Console.WriteLine(now.ToString(""));*/
        
        Freshservice.GetProducts();
        
        


    }
}