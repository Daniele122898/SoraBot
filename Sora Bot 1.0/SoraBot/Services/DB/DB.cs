using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Sora_Bot_1.SoraBot.Services.ConfigService;


namespace Sora_Bot_1.SoraBot.Services.DB
{
    public class DB
    {
        private static DB instance = null;
        private static readonly object padlock = new object();
        private string ip;
        private string port;

        public DB()
        {
            
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