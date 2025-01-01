using System.Text.Json.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Inventory;
using HtmlAgilityPack;


public class DeviceInfo
{

    public static string GetDeviceModel(string deviceServiceTag)
    {
        try
        {
            string url = $"https://www.dell.com/support/product-details/en-us/servicetag/{deviceServiceTag}";
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            HtmlNodeCollection deviceModel = doc.DocumentNode.SelectNodes("//h1[@class='dds__d-none dds__d-sm-none dds__d-md-block dds__h2']");
            return deviceModel[0].InnerHtml.Trim();
        }
        catch
        {
            return string.Empty;
        }
    }

    public static List<string> GetDevicesWithoutIds()
    {

        return new List<string>();
    }
        
    public static void NewDeviceModel(string deviceModel)
    {
        try
        {
            using (StreamReader configFile = new StreamReader(Environment.CurrentDirectory + @"/config.json"))
            using (JsonTextReader reader = new JsonTextReader(configFile))
            {
                JObject json = (JObject)JToken.ReadFrom(reader);
                JObject deviceList = json.SelectToken("devices") as JObject;
                deviceList.Add(deviceModel, "");
                json["devices"].Replace(deviceList);
                configFile.Close();
                File.WriteAllText(Environment.CurrentDirectory + @"/config.json", json.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
    public static string GetDeviceTypeId(string deviceModel)
    {
        string deviceTypeId = string.Empty;
        try
        {
            using (StreamReader configFile = new StreamReader(Environment.CurrentDirectory + @"/config.json"))
            using (JsonTextReader reader = new JsonTextReader(configFile))
            {
                JObject json = (JObject)JToken.ReadFrom(reader);
                deviceTypeId = json["devices"][deviceModel].Value<string>();
            }
        }
        catch (Exception ex)
        {
            return string.Empty;
        }
        return deviceTypeId.Trim();
    }
}