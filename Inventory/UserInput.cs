using System.Collections;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.SharePoint.Client;

namespace Inventory;

public class UserInput
{
    public static string ShowMainMenu()
    {
        ArrayList options = ["Add to Inventory", "Assign Inventory", "View Inventory"];
        string answer = AskQuestion("What are you trying to do?", options);
        return answer;
    }
    
    /* 
     * Method that takes in a string question and array of answer choices. Returns a string of the answer
     */
    public static string AskQuestion(string question, ArrayList answers)
    {
        Console.CursorVisible = false;

        Console.WriteLine(question);
        ConsoleKeyInfo key;
        int option = 0;
        bool isSelected = false;
        string reverseText = "\x1b[7m";

        (int left, int top) = Console.GetCursorPosition();

        while (!isSelected)
        {
            Console.SetCursorPosition(left, top);

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
        string serviceTag = "temp value";
        Console.WriteLine("Enter service tags. Scan them or hit enter after each one (hit enter to exit):");
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

        Console.WriteLine("These are the service tags:");
        foreach (string tag in listOfServiceTags)
        {
            Console.WriteLine(tag);
        }
        Console.WriteLine("\n");
        ArrayList yesNo = ["Yes", "No"];
        string answer = UserInput.AskQuestion("Are these correct?", yesNo);
        if (answer == "")
        {
            
        }

        return listOfServiceTags;
    }
    
}