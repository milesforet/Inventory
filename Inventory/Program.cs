﻿// See https://aka.ms/new-console-template for more information

using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Security.AccessControl;
using Inventory;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Text.Json;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using File = System.IO.File;

public class Program
{
    
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
            
            ArrayList users = json["users"].ToObject<ArrayList>();
            string whichUser = "Uh oh! We don't know you! Who are you?";
            
            string user = UserInput.AskQuestion(whichUser, users);
            json["user"] = user;
            
            configFile.Close();
            File.WriteAllText(path, json.ToString());
            return user;
        }
    }
    
    public static void Main()
    {
        /*Console.CursorVisible = false;
        string user = Program.CheckUser();
        Console.Clear();
        Console.WriteLine($"Welcome {user}! Press any key to continue...");
        Console.ReadKey();
        Console.Clear();

        bool usingApp = true;

        while (usingApp)
        {
            string menuSelection = UserInput.ShowMainMenu();

            if (menuSelection == "Exit")
            {
                Console.Clear();
                
                Environment.Exit(1);
            }
            
        }*/

        
        Freshservice.AssignDevice("123456789", "test");
        
        Environment.Exit(1);
        
        
    }
}