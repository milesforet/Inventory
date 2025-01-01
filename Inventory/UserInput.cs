using System.Collections;
using System.IO.Pipes;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.SharePoint.Client;

namespace Inventory;

public class UserInput
{
    public static string ShowMainMenu()
    {
        List<string> options = ["Add to Inventory", "Assign Inventory", "View Inventory", "Exit"];
        string answer = AskQuestion("What are you trying to do?", options);
        return answer;
    }
    
    /* 
     * Method that takes in a string question and array of answer choices. Returns a string of the answer
     */
    public static string AskQuestion(string question, List<string> answers, int height=0)
    {
        height++;
        Console.CursorVisible = false;

        Console.WriteLine(question);
        ConsoleKeyInfo key;
        int option = 0;
        bool isSelected = false;
        string reverseText = "\x1b[7m";

        

        while (!isSelected)
        {
            Console.SetCursorPosition(0, height);

            for (int i = 0; i < answers.Count; i++)
            {
                if (option == i)
                {
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                Console.WriteLine(answers[i]);
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
            }

            key = Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.DownArrow:
                    option = (option == answers.Count - 1 ? answers.Count - 1 : option + 1);
                    break;
                case ConsoleKey.UpArrow:
                    option = (option == 0 ? 0 : option - 1);
                    break;
                case ConsoleKey.Enter:
                    isSelected = true;
                    break;
            }
        }
        
        return answers[option].ToString();
    }

    public static List<string> GetServiceTags()
    {
        ConsoleKeyInfo notEnter = new ConsoleKeyInfo();
        List<string> listOfServiceTags = new List<string>();

        bool needsToAddItems = true;

        while (needsToAddItems)
        {
            Console.Clear();
            Console.CursorVisible = true;
            Console.WriteLine("Enter service tags. Scan them or hit enter after each one (hit enter to exit):");

            foreach (string tag in listOfServiceTags)
            {
                Console.WriteLine(tag);
            }

            string serviceTag = "temp value";
            while (!string.IsNullOrEmpty(serviceTag))
            {
                
                serviceTag = Console.ReadLine();
                serviceTag = serviceTag.ToUpper();
                if (!string.IsNullOrEmpty(serviceTag) && !listOfServiceTags.Contains(serviceTag))
                {
                    listOfServiceTags.Add(serviceTag);
                }
            }

            bool verifyItems = true;
            while (verifyItems) {
                Console.Clear();
                (int left, int top) = Console.GetCursorPosition();
                Console.SetCursorPosition(left, top);
                Console.CursorVisible = false;
                Console.WriteLine("These are the service tags:");
                foreach (string tag in listOfServiceTags)
                {
                    Console.WriteLine(tag);
                }

                Console.WriteLine("");
                List<string> yesNo = ["Yes", "No"];
                (int x, int y) = Console.GetCursorPosition();
                string questionStart = (listOfServiceTags.Count == 1) ? "Is this " : "Are these ";
                
                string answer = AskQuestion($"{questionStart}{listOfServiceTags.Count} item(s) correct?", yesNo, y);
                Console.Clear();

                if (answer == "Yes")
                {
                    return listOfServiceTags;
                }

                List<string> addOrRemoveList = ["Add", "Remove"];
                string addOrRemove = AskQuestion("Do you need to add or remove items?", addOrRemoveList);

                if (addOrRemove == "Add")
                {
                    Console.Clear();
                    verifyItems = false;
                    continue;
                }
                
                bool removingItems = true;
                List<string> serviceTagsToRemove = new List<string>();
                
                while (removingItems)
                {
                    Console.Clear();
                    List<string> answersToRemove = new List<string>(listOfServiceTags);
                    answersToRemove.Add("--Done--");
                    string answerToRemovalQuestion = AskQuestion("Which devices do you want to remove?", answersToRemove);
                    if (answerToRemovalQuestion == "--Done--")
                    {
                        removingItems = false;
                    }
                    else
                    {
                        serviceTagsToRemove.Add(answerToRemovalQuestion);
                        listOfServiceTags = listOfServiceTags.Except(serviceTagsToRemove).ToList();
                    }
                    if (listOfServiceTags.Count == 0)
                    {
                        removingItems = false;
                        verifyItems = false;
                    }
                }
                
            }
            
            Console.Clear();
            foreach (var VARIABLE in listOfServiceTags)
            {
                Console.WriteLine(VARIABLE);
            }
        }
        
        return listOfServiceTags;
    }
    
}