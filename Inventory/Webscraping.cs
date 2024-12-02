namespace Inventory;
using HtmlAgilityPack;


public class Webscraping
{

    public static string getDeviceModel(string deviceServiceTag)
    {
        string url = $"https://www.dell.com/support/product-details/en-us/servicetag/{deviceServiceTag}";
        HtmlWeb web = new HtmlWeb();
        HtmlDocument doc = web.Load(url);
        HtmlNodeCollection deviceModel = doc.DocumentNode.SelectNodes("//h1[@class='dds__d-none dds__d-sm-none dds__d-md-block dds__h2']");
        return deviceModel[0].InnerHtml.Trim();
    }
}