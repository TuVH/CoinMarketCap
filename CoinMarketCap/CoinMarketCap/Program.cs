using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CoinMarketCap
{
    class Program
    {
        //aa
        public class CoinReponse
        {
            public string Name { get; set; }
            public List<CoinReponseLists> coinReponseLists { get; set; }
        }
        public class CoinReponseLists
        {
            public DateTime TimeOpen { get; set; }
            public DateTime TimeClose { get; set; }
            public DateTime TimeHigh { get; set; }
            public DateTime TimeLow { get; set; }
            public CoinReponseDetail CoinReponseDetails { get; set; }
        }
        public class CoinReponseDetail
        {
            public double Open { get; set; }
            public double High { get; set; }
            public double Low { get; set; }
            public double Close { get; set; }
            public double Volume { get; set; }
            public double MarketCap { get; set; }
            public DateTime Timestamp { get; set; }
        }
        static void Main(string[] args)
        {
            try
            {
                bool flag = false;
                int[] coinNeed = { 9288, 2608, 14806, 1975, 2566, 14463, 8206, 20314, 4172, 6535, 20456, 328, 2700, 12971 };
                for (int i = 0; i < coinNeed.Length; i++)
                {
                    List<CoinReponse> listCoins = new List<CoinReponse>();
                    Int32 unixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                    for (int j = 1638921600; j < unixTimestamp; j += 7000000)
                    {
                        var temp = j + 7000000;
                        var resultCoin = ConvertCoin(Post(String.Format("https://api.coinmarketcap.com/data-api/v3/cryptocurrency/historical?id={0}&convertId=2823&timeStart={1}&timeEnd={2}", coinNeed[i], j, temp)));
                        listCoins.Add(resultCoin);
                    }

                    CoinReponseLists dateLowestValue = new CoinReponseLists
                    {
                        CoinReponseDetails = new CoinReponseDetail()
                    };
                    for (int k = 0; k < listCoins.Count; k++)
                    {
                        if (listCoins[k].coinReponseLists.Count == 0)
                        {
                            continue;
                        }
                        else if (listCoins[k].coinReponseLists.Count > 0)
                        {
                            dateLowestValue.CoinReponseDetails.Low = listCoins[k].coinReponseLists[0].CoinReponseDetails.Low;
                            break;
                        }
                    }
                    foreach (var item in listCoins)
                    {
                        var min = item.coinReponseLists.Count != 0 ? item.coinReponseLists?.Select(x => x.CoinReponseDetails.Low).Min() : 0;
                        var lowestValue = item.coinReponseLists.SingleOrDefault(x => x.CoinReponseDetails.Low == min);
                        if (lowestValue == null)
                        {
                            continue;
                        }
                        if (lowestValue.CoinReponseDetails.Low < dateLowestValue.CoinReponseDetails.Low)
                        {
                            dateLowestValue = lowestValue;
                        }
                    }
                    string format = "{0,-15} {1,-30} {2,-27} {3,-5} {4,-7}";
                    string warrning = "";
                    if ((dateLowestValue.CoinReponseDetails.Low * 1.05) >= listCoins[^1].coinReponseLists[^1].CoinReponseDetails.Open)
                    {
                        warrning = "Warning";
                    }
                    string[] row = new string[] { "Name: " + listCoins[0].Name, " Time: " + dateLowestValue.TimeHigh, "Lowest: " + dateLowestValue.CoinReponseDetails.Low + "đ" , "Near:" + listCoins[3].coinReponseLists[67].CoinReponseDetails.Open, warrning };
                    Console.WriteLine(String.Format(format, row));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
        static CoinReponse ConvertCoin(string obj)
        {
            var json = JsonConvert.DeserializeObject<dynamic>(obj);

            List<CoinReponseLists> coinReponseLists = new List<CoinReponseLists>();
            foreach (var item in json["data"]["quotes"])
            {
                var coin = new CoinReponseDetail
                {
                    Low = item["quote"]["open"],
                    High = item["quote"]["high"],
                    MarketCap = item["quote"]["marketCap"],
                    Timestamp = item["quote"]["timestamp"],
                    Open = item["quote"]["open"],
                    Volume = item["quote"]["volume"]
                };
                var temp = new CoinReponseLists
                {
                    TimeClose = item["timeOpen"],
                    TimeHigh = item["timeHigh"],
                    TimeLow = item["timeLow"],
                    TimeOpen = item["timeOpen"],
                    CoinReponseDetails = coin
                };
                coinReponseLists.Add(temp);
            }

            CoinReponse result = new CoinReponse
            {
                Name = json["data"]["symbol"].ToString(),
                coinReponseLists = coinReponseLists,
            };
            return result;
        }
        public static string Post(string url)
        {
            HttpResponseMessage result = null;
            var _client = new HttpClient();

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, url))
            {
                //requestMessage.Content = new StringContent(Encoding.UTF8, "application/json");
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                requestMessage.Headers.Add("User-Agent","Other");
                //requestMessage.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);

                var task = Task.Run(() => _client.SendAsync(requestMessage));
                task.Wait();
                result = task.Result;
            }

            if (result != null && result.IsSuccessStatusCode)
            {
                var responseString = Task.Run(() => result.Content.ReadAsStringAsync());
                responseString.Wait();

                var respData = responseString.Result;
                return respData;
            }

            throw new Exception($"Get request to '${url}' failed");
        }
    }
}
