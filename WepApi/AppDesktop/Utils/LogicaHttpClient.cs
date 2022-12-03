using Newtonsoft.Json;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace AppDesktop.Utils
{
    public static class LogicaHttpClient
    {
        public static string PostSync(string url, dynamic pData)
        {
            string result;
            try
            {
                var client = new RestClient(Rutas.urlBase);
                var request = new RestRequest(url, Method.Post);
                string json = JsonConvert.SerializeObject(pData);
                request.AddBody(json);
                var resulta = client.Execute(request);
                result=resulta.Content;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        public static async Task<string> PostAsync(string url, dynamic pData)
        {
            string result;
            try
            {
                var client = new RestClient(Rutas.urlBase);
                var request = new RestRequest(url, Method.Post);
                string json = JsonConvert.SerializeObject(pData);
                request.AddBody(json);
                var data = await client.ExecuteAsync(request);
                result=data.Content;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }
    }
}