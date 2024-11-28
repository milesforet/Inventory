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
        catch (Exception ex)
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
                WriteOutput(json.ToString());

            }
        }
        catch (Exception ex)
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
        WriteOutput(jsonResponse.ToString());
        deviceInfo.Add("user", jsonResponse["config_items"][0]["user_id"]);
        deviceInfo.Add("name", jsonResponse["config_items"][0]["name"]);
        deviceInfo.Add("lastLogin", jsonResponse["config_items"][0]["levelfield_values"]["last_login_by_21002167467"]);

        return deviceInfo;
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

    public static string GetEmployeeId(string email)
    {
        if (email.Contains("@abskids.com"))
        {
            email = email.Replace("@abskids.com", "");
        }

        try
        {
            using (HttpClient httpClient = new HttpClient())
            {
                string url =
                    $"https://abskids.freshservice.com/api/v2/requesters?query=\"primary_email:%27{email}%40abskids.com%27\"";

                string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(FRESH_KEY));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = httpClient.GetAsync(url).Result;
                if (!(response.StatusCode == HttpStatusCode.OK))
                {
                    return "";
                }

                JObject jsonResponse = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                if (jsonResponse["requesters"].Count() == 0)
                {
                    return "";
                }

                return jsonResponse["requesters"][0]["id"].ToString();
            }
        }
        catch (Exception ex)
        {
            Console.Clear();
            Console.WriteLine($"Unhandled Exception - {ex} Exiting...");
            Environment.Exit(1);
        }

        return "";
    }


    public static void InventoryDevice(string whosInventory, string deviceId, bool newDevice)
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
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
}