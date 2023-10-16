﻿using MySqlConnector;
using Newtonsoft.Json;

namespace SAMonitor.Utils
{
    public static class MySql
    {
        public static string? ConnectionString { get; private set; }
        public static bool MySqlSetup()
        {
            MySqlConnectionStringBuilder builder = new();

            try
            {
                dynamic? data = JsonConvert.DeserializeObject(File.ReadAllText($"/home/markski/sam/mysql.txt"));

                if (data is null)
                    return false;

                builder.Server = data.Server;
                builder.UserID = data.UserID;
                builder.Password = data.Password;
                builder.Database = data.Database;

                ConnectionString = builder.ToString();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}