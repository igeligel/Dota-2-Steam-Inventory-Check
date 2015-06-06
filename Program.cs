using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SteamBotV2.Dota2LoungeAPI;

namespace SteamBotV2
{
    class Program
    {
        static void Main(string[] args)
        {
			string steamid = "";
			//Insert a steamid here
            steamid = "78171214148";
            Inventory i = new Inventory(steamid);
            Console.WriteLine(i.ToString());
            Console.ReadKey();
        }
    }
}
