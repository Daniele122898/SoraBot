using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using static RethinkDb.Driver.RethinkDB;
using Sora_Bot_1.SoraBot.Services.ConfigService;
using RethinkDb.Driver;
using RethinkDb.Driver.Ast;

namespace Sora_Bot_1.SoraBot.Services.DB
{
    public class DB
    {
        private static DB instance = null;
        private static readonly object padlock = new object();
        private string ip;
        private string port;
        public RethinkDb.Driver.Net.Connection conn { get; private set; }

        public DB()
        {
            ConcurrentDictionary<string, string> config = ConfigService.ConfigService.getConfig();
            config.TryGetValue("RethinkDBIP", out ip);
            config.TryGetValue("RethinkDBPort", out port);
            conn = R.Connection()
                .Hostname(ip)
                .Port(Int32.Parse(port))
                .Timeout(60)
                .Connect();
        }

        //Singleton dings bums
        public static DB Instance
        {
            get
            {
                lock (padlock)//1. thread locked macht scheisse, 2 lock wartet bis 1. fertig ist und geht dann durch
                {
                    if (instance == null)//Erstes mal wird der Konstruktor gerufen, danach die instanz weiter gegeben <3
                    {
                        instance = new DB();
                    }
                    return instance;
                }
            }
        }
    }
}