using System.Collections;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Inventory;

public class Freshservice
{

    private const string FRESH_KEY = "";
    private const string NINJA_KEY = "";


    public static void WriteOutput(string content)
    {
        File.WriteAllText("../../../output.json", content);
    }

    public static JObject GetFreshApi(string endpoint)
    {   
        string url = $"https://abskids.freshservice.com/{endpoint}";
        JObject json = new JObject();

        
        try
        {
            using (HttpClient httpClient = new HttpClient())
            {
                
                string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(FRESH_KEY));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = httpClient.GetAsync(url).Result;
                
                if (!response.IsSuccessStatusCode) 
                { 
                    throw new Exception($"Error: {response.ReasonPhrase}");
                }
                
                json = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return json;
    }
    
    public static JObject PutFreshApi(string endpoint, Dictionary<string, string> body)
    {   
        
        FormUrlEncodedContent content = new FormUrlEncodedContent(body);
        
        string url = $"https://abskids.freshservice.com/{endpoint}";
        JObject json = new JObject();
        
        try
        {
            using (HttpClient httpClient = new HttpClient())
            {
                
                string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(FRESH_KEY));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = httpClient.PutAsync(url, content).Result;
                
                if (!response.IsSuccessStatusCode) 
                { 
                    throw new Exception($"Error: {response.ReasonPhrase}");
                }
                
                json = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return json;
    }
    
    
    public static void AssignDevice(string serviceTag, string user)
    {
        if (!user.Contains("@abskids.com"))
        {
            user = user + "@abskids.com";
        }
        
        //TODO: FRESH API TO GET USER ID OF ASSIGNEE. NEED PERMISSION TO CALL USER ENDPOINT

        Dictionary<string, string> body = new Dictionary<string, string>();
        body.Add("state_name", "In Use");
        body.Add("user_id", "21001534735");
        JObject json = PutFreshApi($"/api/v2/assets/{serviceTag}", body);
        Console.WriteLine(json);

    }

    public static Hashtable GetDeviceInfo(string serviceTag)
    {
        
        string endpoint = $"cmdb/items/list.json?field=serial_number&q={serviceTag}";
        Hashtable deviceInfo = new Hashtable();
        
        JObject jsonResponse = GetFreshApi(endpoint);
        
        deviceInfo.Add("user", jsonResponse["config_items"][0]["user_id"]);
        deviceInfo.Add("name", jsonResponse["config_items"][0]["name"]);
        deviceInfo.Add("lastLogin", jsonResponse["config_items"][0]["levelfield_values"]["last_login_by_21002167467"]);
        
        return deviceInfo;
    }
    
    
}

//NINJA API TEST -- THIS IS WORKING :)
/*httpClient.DefaultRequestHeaders.Add("X-Api-Key", NINJA_KEY);
HttpResponseMessage response = httpClient.GetAsync(url).Result;
JObject jsonResponse = JObject.Parse(await response.Content.ReadAsStringAsync());
Console.WriteLine(jsonResponse.ToString());*/