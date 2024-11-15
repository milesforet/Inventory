namespace Inventory;
using Microsoft.SharePoint.Client;
using SP = Microsoft.SharePoint.Client;
using Microsoft.Identity.Client;

public class Sharepoint
{
    public static void connectToSharepoint()
    {
        string clientId = "";
        string tenantId = "";
        string authority = $"https://login.microsoftonline.com/{tenantId}/";
        string redidirectUri = "http://localhost";
        
        
    }

    public void getSharepointData()
    {
        string sharepointUrl = "https://abs79.sharepoint.com/sites/ITLogsandAudits";
        
        ClientContext clientContext = new ClientContext(sharepointUrl);
        SP.List list = clientContext.Web.Lists.GetByTitle("EquipmentPrep");
        
        CamlQuery camlQuery = new CamlQuery();
        camlQuery.ViewXml = "<View><Query><Where><Eq><FieldRef Name='Title'/>" +
                            "<Value Type='Text'>Michael Miller</Value></Eq></Where></Query></View>";
        ListItemCollection listItems = list.GetItems(camlQuery);
        
        clientContext.Load(listItems);
        clientContext.ExecuteQuery();

        foreach (ListItem item in listItems)
        {
            Console.WriteLine("Name: {0}\nSup: {1}", item["Title"], item["Supervisor"]);
        }
    }
}