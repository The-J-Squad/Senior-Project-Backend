﻿namespace RecipeBackend.Models
{
    public class Settings
    {
        public string ConnectionString { get; set; }
        public string Database { get; set; }
        public string Collection { get; set; }
        public string MongoPath { get; set; } = "";
        public string ApplicationName { get; set; }
        public string JwtSecret { get; set; }
    }
}
