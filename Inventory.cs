using Newtonsoft.Json;
using Dota2SteamInvCheck.SerializationClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;

namespace Dota2SteamInvCheck
{
    /// <summary>
    ///     Inventory Class to save a inventory
    /// </summary>
    public class Inventory
    {
        /// <summary>
        /// List of the inventory.
        /// </summary>
        public List<InventoryEntry> list;

        /// <summary>
        /// The URL of the inventory
        /// </summary>
        private String inventoryURL = "";

        /// <summary>
        ///     Public constructor in which the inventory will be analysed.
        /// </summary>
        /// <param name="inventoryURL">
        ///     The URL of the inventory.
        /// </param>
        public Inventory(string _steam64Id)
        {
            string _inventoryURL = "http://steamcommunity.com/profiles/" + _steam64Id + "/inventory/json/570/2/";
            this.inventoryURL = _inventoryURL;
            this.updateInventory();
        }

        /// <summary>
        ///     Update the local list of the inventory.
        /// </summary>
        public void updateInventory()
        {
            list = new List<InventoryEntry>();
            String result = this.getStringOutOfWebResponse(this.getRequest(this.inventoryURL, "GET", 7000));
            var root = JsonConvert.DeserializeObject<RootObject>(result);
            if (root.success)
            {
                Console.WriteLine("Executing Pricecheck:");
                int counter = 0;

                int maxItemsToCheck = 0;
                foreach (var pair in root.rgInventory)
                {
                    DescriptionsEntry temp = root.rgDescriptions[pair.Value.classid + "_" + pair.Value.instanceid];
                    if (temp.marketable && temp.marketable) 
                    {
                        maxItemsToCheck += 1;
                    }
                }
                
                foreach (var pair in root.rgInventory)
                {
                    DescriptionsEntry temp = root.rgDescriptions[pair.Value.classid + "_" + pair.Value.instanceid];
                    if (temp.tradable && temp.marketable)
                    {
                        for (int i = 0; i < counter.ToString().Length + 6; i++)
                        {
                            Console.Write("\b");
                        }
                        
                        Console.Write(counter + " / " + maxItemsToCheck);
                        double price = -1;
                        foreach (var data in list)
                        {
                            if (data.market_hash_name == temp.market_hash_name)
                            {
                                price = data.price;
                            }
                        }
                        if (price == -1)
                        {
                            price = getPrice((HttpUtility.UrlEncode(temp.market_hash_name)).Replace("+", "%20"));
                        }

                        InventoryEntry tempInventoryObject = this.getInventoryEntryObject(temp, price);
                        list.Add(tempInventoryObject);
                        counter += 1;
                    }
                }
            }
        }

        /// <summary>
        ///     Will return the price of an specific item.
        /// </summary>
        /// <param name="marketHashName">
        ///     The item which price will be calculated.
        /// </param>
        /// <returns>
        ///     The price of an item as a double.
        /// </returns>
        private double getPrice(String marketHashName)
        {
            double doubleResult = 0.00;
            try
            {
                String url = "http://steamcommunity.com/market/priceoverview/?currency=3&appid=570&market_hash_name=" + marketHashName;
                String result = this.formatResult(this.getStringOutOfWebResponse(this.getRequest(url, "GET", 3000)));
                var root = JsonConvert.DeserializeObject<RootPriceRequest>(result);
                if (root.lowest_price != null)
                {
                    doubleResult = Double.Parse(root.lowest_price);
                }
            }
            catch (WebException ex)
            {
                DateTime time = new DateTime();
                time = DateTime.Now.ToLocalTime();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(time + ": " + ex.Status);
                Console.WriteLine(ex.Message);
                Console.WriteLine("------------");
                Console.ResetColor();
            }
            return doubleResult;
        }

        /// <summary>
        ///     This method will format the price String result of using this URL:
        ///     "http://steamcommunity.com/market/priceoverview/?currency=" + currency + "&appid=" + appid + "&market_hash_name=" + marketHashName.
        ///     Some Decoding will be done here.
        /// </summary>
        /// <param name="input">
        ///     The String which should be parsed.
        /// </param>
        /// <returns>
        ///     The formatted String
        /// </returns>
        private String formatResult(String input)
        {
            input = HttpUtility.HtmlDecode(input);
            input = input.Replace("€ ", "");
            return input.Replace(",--", "");
        }

        /// <summary>
        ///     This method will do a HttpWebRequest.
        /// </summary>
        /// <param name="_uRL">
        ///     The url which will the WebRequest connect to.
        /// </param>
        /// <param name="_method">
        ///     GET or POST.
        /// </param>
        /// <param name="_timeout">
        ///     Time after the Request will timeout.
        /// </param>
        /// <returns>
        ///     The webrequest will response. So it will return a HttpWebResponse Object.
        /// </returns>
        private HttpWebResponse getRequest(String _uRL, String _method, int _timeout)
        {
            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(_uRL);
            WebReq.Method = _method;
            WebReq.Timeout = _timeout;
            HttpWebResponse result = null;
            try
            {
                result = (HttpWebResponse)WebReq.GetResponse();
            }
            catch (WebException ex)
            {
                DateTime time = new DateTime();
                time = DateTime.Now.ToLocalTime();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(time + ": " + ex.Status);
                Console.WriteLine(ex.Message);
                Console.WriteLine("------------");
                Console.ResetColor();
            }
            return result;
        }

        /// <summary>
        ///     This method will parse a HttpWebResponse to a String.
        /// </summary>
        /// <param name="_webResponse">
        ///     The WebResponse which should be parsed.
        /// </param>
        /// <returns>
        ///     The webrequest will response a string. This is the String of the Repsonse
        /// </returns>
        private String getStringOutOfWebResponse(HttpWebResponse _webResponse)
        {
            Stream Answer = _webResponse.GetResponseStream();
            StreamReader _Answer = new StreamReader(Answer);
            return _Answer.ReadToEnd();
        }

        /// <summary>
        ///    Will create a InventoryEntries Object because in the DescriptionEntry Object the price is missing. 
        /// </summary>
        /// <param name="input">
        ///     The copy of the Descriptionsobject. I create another one because in it the price is missing and not calculated.
        /// </param>
        /// <param name="price">
        ///     The price that should be added to the DesciptionEntry object.
        /// </param>
        /// <returns>
        ///     A InventoryEntries object.
        /// </returns>
        private InventoryEntry getInventoryEntryObject(DescriptionsEntry input, double price)
        {
            InventoryEntry a = new InventoryEntry();
            a.appid = input.appid;
            a.classid = input.classid;
            a.commodity = input.commodity;
            a.instanceid = input.instanceid;
            a.market_hash_name = input.market_hash_name;
            a.market_name = input.market_name;
            a.marketable = input.marketable;
            a.name = input.name;
            a.tradable = input.tradable;
            a.type = input.type;
            String tempStr = input.market_name;
            a.price = price;
            return a;
        }

        /// <summary>
        ///     Will return the price of all marketable and tradable items combined.
        /// </summary>
        /// <returns>
        ///     The value of the inventory
        /// </returns>
        public double getValueOfInventory()
        {
            double resultPrice = 0.00;
            foreach (var d in this.list)
            {
                resultPrice += d.price;
            }
            return Math.Round(resultPrice, 2);
        }

        /// <summary>
        ///     Returns the price of an specific items in this inventory.
        /// </summary>
        /// <param name="_name">
        ///     The name of the item which should be checked.
        /// </param>
        /// <returns> 
        ///     The price of the item.
        /// </returns>
        public double getPriceOf(String _name)
        {
            double resultPrice = 0.00;
            foreach (var d in this.list)
            {
                if (d.name == _name)
                {
                    resultPrice = d.price;
                }
            }
            return Math.Round(resultPrice, 2);
        }

        /// <summary>
        ///     Returns the inventory as String.
        /// </summary>
        /// <returns>
        ///     Inventory as String.
        /// </returns>
        public override string ToString()
        {
            string result = "";
            if (list.Count != 0) 
            {
                result += "List of the inventory, AppId: " + this.list[0].appid;
                foreach (var data in this.list)
                {
                    result += "\n";
                    result += data.name + ": " + data.price + " (" + data.type + ")";
                }
            }
            else 
            {
                result = "Inventory is empty.";
            }
            Console.WriteLine(result);
            return result;
            
        }
    }

    /// <summary>
    ///     Class: copy of DescriptionsEntry, because the price of an item is not included in that class.
    /// </summary>
    public class InventoryEntry : DescriptionsEntry
    {
        public double price;
    }
}
