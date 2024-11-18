using System.Collections;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.SharePoint.Client;

namespace Inventory;

public class UserInput
{
    public static string ShowMainMenu()
    {
        ArrayList options = ["Add to Inventory", "Assign Inventory", "View Inventory", "Exit"];
        string answer = AskQuestion("What are you trying to do?", options);
        return answer;
    }
    
    /* 
     * Method that takes in a string question and array of answer choices. Returns a string of the answer
     */
    public static string AskQuestion(string question, ArrayList answers, int height=0)
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
                Console.WriteLine($"{(option == i ? reverseText : "")}{answers[i]}\x1b[27m");
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

    public static ArrayList GetServiceTags()
    {
        ConsoleKeyInfo notEnter = new ConsoleKeyInfo();
        ArrayList listOfServiceTags = new ArrayList();

        bool needsToAddItems = true;

        while (needsToAddItems)
        {
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

            ArrayList yesNo = ["Yes", "No"];
            (int x, int y) = Console.GetCursorPosition();
            string answer = AskQuestion($"Are these {listOfServiceTags.Count} items correct?", yesNo, y);
            Console.Clear();

            if (answer == "Yes")
            {
                break;
            }
            
            ArrayList addOrRemoveList = ["Add", "Remove"];
            string addOrRemove = AskQuestion("Do you need to add or remove items?", addOrRemoveList);
            
            if (addOrRemove == "Add")
            {
                Console.Clear();
                continue;
            }
            
            Console.Clear();
        }
        
        return listOfServiceTags;
    }
    
}