// See https://aka.ms/new-console-template for more information

using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Security.AccessControl;
using Inventory;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.Json;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Console = System.Console;
using File = System.IO.File;

public class Program
{

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
        while (keepAppRunning)
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
            else
            {
                Console.Clear();
                List<string> devicesBeingUpdated = UserInput.GetServiceTags().Cast<string>().ToList();
                List<string> devicesToRemove = new List<string>();
                
                foreach (string serviceTag in devicesBeingUpdated)
                {
                    string deviceId = Freshservice.GetDeviceId(serviceTag);
                    if (deviceId == "")
                    {
                        devicesToRemove.Add(serviceTag);
                    }
                }
                Console.WriteLine("The following devices could not be found in FS. They cannot be updated" 
                                   + String.Join(", ", devicesToRemove));
                
                devicesBeingUpdated = devicesBeingUpdated.Except(devicesToRemove).ToList();

                if (devicesBeingUpdated.Count == 0)
                {
                    Console.WriteLine("No service tags detected. Hit any key to return to menu.");
                    Console.ReadLine();
                    continue;
                }
                

                if (menuSelection == "Add to Inventory")
                {
                    Console.WriteLine("ADD TO INVENTORY");
                }else if (menuSelection == "Assign Inventory")
                {
                    Console.WriteLine("Assign INVENTORY");
                }
                
            }

            keepAppRunning = false;
        }
    }
    
    public static void Main()
    {

        AppFlow();
        
        Environment.Exit(1);
    }
}