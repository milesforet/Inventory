using System.Collections;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Inventory;

public class Freshservice
{

    private const string FRESH_KEY = "";
    
    public static void WriteOutput(string content)
    {
        File.WriteAllText(Environment.CurrentDirectory + @"/output.json", content);
    }
    
    //ONLY USING THIS METHOD TO GET DEVICE TYPE FIELDS. V1 OF API MAKES IT 1 CALL VS 2 WITH API V2
    public static void GetDeviceInfo(string serviceTag)
    {

        string url = $"https://abskids.freshservice.com/cmdb/items/list.json?field=serial_number&q={serviceTag}";
        
        using (HttpClient httpClient = new HttpClient())
        {
            string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(FRESH_KEY));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = httpClient.GetAsync(url).Result;
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);

        }
        
    }



    public static void GetProducts()
    {

        try
        {
            using (HttpClient httpClient = new HttpClient())
            {
                string url = "https://abskids.freshservice.com/api/v2/products?per_page=100";
                string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(FRESH_KEY));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = httpClient.GetAsync(url).Result;
                WriteOutput(response.Content.ReadAsStringAsync().Result);
            }
        }
        catch
        {
            Console.WriteLine("Error in GetProducts");
        }
        
    }
    
    
    
    
    //returns the display id of the device or an empty string if one isn't found
    public static string GetDeviceId(string serviceTag)
    {
        string id = string.Empty;
        string url = $"https://abskids.freshservice.com/api/v2/assets?search=\"serial_number%3A%27{serviceTag}%27\"";

        try
        {
            using (HttpClient httpClient = new HttpClient())
            {
                string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(FRESH_KEY));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = httpClient.GetAsync(url).Result;
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                
                //FYI: api return 200 status when no device is found. assets array is just empty.
                if (!(response.StatusCode == HttpStatusCode.OK))
                {
                    return $"Error getting device {serviceTag} - {response.ReasonPhrase}";
                }

                JObject jsonResponse = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                //Console.WriteLine(jsonResponse);
                if (jsonResponse["assets"].Count() == 0)
                {
                    return string.Empty;
                }

                id = jsonResponse["assets"][0]["display_id"].ToString();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        

        return id;
    }

    public static long GetEmployeeId(string email)
    {

        try
        {
            using (HttpClient httpClient = new HttpClient())
            {
                string url = $"https://abskids.freshservice.com/api/v2/requesters?query=\"primary_email:%27{email}%40abskids.com%27\"";

                string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(FRESH_KEY));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = httpClient.GetAsync(url).Result;
                if (!(response.StatusCode == HttpStatusCode.OK))
                {
                    return 0;
                }

                JObject jsonResponse = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                if (jsonResponse["requesters"].Count() == 0)
                {
                    return 0;
                }

                return jsonResponse["requesters"][0]["id"].ToObject<long>();
            }
        }
        catch (Exception ex)
        {
            Console.Clear();
            Console.WriteLine($"Unhandled Exception - {ex} Exiting...");
            Environment.Exit(1);
        }

        return 0;
    }


    public static bool InventoryDevice(string whosInventory, string deviceId, bool newDevice)
    {
        JObject requestBody = new JObject();
        requestBody.Add("user_id", null);
        JObject typeFields = new JObject();
        typeFields.Add("asset_state_21002167462", "In Stock");
        requestBody.Add("type_fields", typeFields);
        requestBody.Add("group_id", 21000516398);


        string url = $"https://abskids.freshservice.com/api/v2/assets/{deviceId}";

        //if device is new
        if (newDevice)
        {
            switch (whosInventory)
            {
                case "Dalton":
                    requestBody.Add("location_id", 21000555700);
                    break;
                case "Miles":
                    requestBody.Add("location_id", 21000555701);
                    break;
                case "Warren":
                    requestBody.Add("location_id", 21000555699);

                    break;
            }
        }
        //if device is used
        else
        {
            switch (whosInventory)
            {
                case "Dalton":
                    requestBody.Add("location_id", 21000555863);
                    break;
                case "Miles":
                    requestBody.Add("location_id", 21000555861);
                    break;
                case "Warren":
                    requestBody.Add("location_id", 21000555862);
                    break;
            }
        }

        try
        {
            using (HttpClient httpClient = new HttpClient())
            {
                string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(FRESH_KEY));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpContent content = new StringContent(requestBody.ToString(), Encoding.UTF8, "application/json");
                HttpResponseMessage response = httpClient.PutAsync(url, content).Result;
                if (!(response.StatusCode == HttpStatusCode.OK))
                {
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }

        return true;
    }
    
    
    public static bool AssignDevice(string deviceId, long userId, string assetPurpose)
    {
        if (assetPurpose != "Admin" || assetPurpose == "Reception (CSS)")
        {
            return false;
        }
        
        DateTime date = DateTime.Now;
        Dictionary<string, string> body = new Dictionary<string, string>();
        
        JObject requestBody = new JObject();
        JObject typeFields = new JObject();
        typeFields.Add("asset_state_21002167462", "In Use");
        typeFields.Add("asset_purpose_21002167467", assetPurpose);
        
        requestBody.Add("type_fields", typeFields);
        requestBody.Add("user_id", userId);
        requestBody.Add("group_id", 21000516398);
        requestBody.Add("location_id", null);
        string url = $"https://abskids.freshservice.com/api/v2/assets/{deviceId}";

        try
        {
            using (HttpClient httpClient = new HttpClient())
            {
                string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(FRESH_KEY));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpContent content = new StringContent(requestBody.ToString(), Encoding.UTF8, "application/json");
                HttpResponseMessage response = httpClient.PutAsync(url, content).Result;
                WriteOutput(response.Content.ReadAsStringAsync().Result);

                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }
                
            }
        }
        catch (Exception ex)
        {
            return false;
        }

        return true;
    }

    public static bool AddNewDevice(string serviceTag, string assetPurpose, string assetState, string deviceModel, string description = "", string location = null, long userId = 0)
    {
        if (assetPurpose != "Admin" || assetPurpose == "Reception (CSS)")
        {
            return false;
        }
        
        string deviceType = "Computer";
        
        long productId = 21000257174;
        DateTime date = DateTime.Now;
        
        JObject requestBody = new JObject();
        JObject typeFields = new JObject();
        
        //desktop = 21002167498
        //product_21002167462 - 21002167467 (Computer)
        
        typeFields.Add("asset_state_21002167462", assetState);
        typeFields.Add("asset_purpose_21002167467", assetPurpose);
        typeFields.Add("serial_number_21002167462", serviceTag);
        typeFields.Add("manufacturer_21002167462", "Dell Inc.");
        typeFields.Add("product_21002167462", productId);
        
        requestBody.Add("name", serviceTag);
        requestBody.Add("type_fields", typeFields);
        requestBody.Add("group_id", 21000516398);
        requestBody.Add("description", description);
        requestBody.Add("location_id", location);
        //requestBody.Add("updated_at", "2018-08-03T07:48:11Z");
        //requestBody.Add("ci_type_name", deviceType);
        requestBody.Add("asset_type_id", 21002167498);

        if (userId == 0){
            requestBody.Add("user_id", null);
        }
        else
        {
            requestBody.Add("user_id", userId);
        }

        try
        {
            using (HttpClient httpClient = new HttpClient())
            {
                string url = $"https://abskids.freshservice.com/api/v2/assets";
                string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(FRESH_KEY));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpContent content = new StringContent(requestBody.ToString(), Encoding.UTF8, "application/json");
                HttpResponseMessage response = httpClient.PostAsync(url, content).Result;
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }
            }
        }
        catch
        {
            return false;
        }
        
        return true;
    }
    
}