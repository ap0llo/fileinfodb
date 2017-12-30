using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileInfoDb.Config
{
    class ConfigurationWriter
    {
        readonly ILogger m_Logger;
        readonly string m_Path;

        public ConfigurationWriter(ILogger logger, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Value must not be null or empty", nameof(path));
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_Path = path;
        }



        public void SetValue(string key, bool value) => SetValue(key, (JToken) value);

        public void SetValue(string key, string value) => SetValue(key, (JToken)value);


        void SetValue(string key, JToken value)
        {
            // load configuration (or create new configuration)
            JObject root;
            if(File.Exists(m_Path))
            {
                m_Logger.LogInformation($"Loading existing configuration file from '{m_Path}'");
                root = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(m_Path));
            }
            else
            {
                root = new JObject();
                Directory.CreateDirectory(Path.GetDirectoryName(m_Path));
            }


            // find node which's property to set
            var keyParts = key.Split(':');           
            var currentParent = root;
            foreach(var name in keyParts.Take(keyParts.Length - 1))
            {
                var property = currentParent.Property(name);
                if(property == null)
                {
                    var newNode = new JObject();
                    currentParent.Add(new JProperty(name, newNode));
                    currentParent = newNode;
                }
                else
                {
                    currentParent = (JObject) property.Value;
                }
            }

            // set propety
            var proeprtyName = keyParts.Last();
            var propertyToSet = currentParent.Property(proeprtyName);
            if(propertyToSet == null)
            {
                currentParent.Add(new JProperty(proeprtyName, value));
            }
            else
            {
                propertyToSet.Value = value;
            }
            var json = JsonConvert.SerializeObject(root, Formatting.Indented);

            m_Logger.LogInformation($"Saving configuration to '{m_Path}'");
            File.WriteAllText(m_Path, json);
        }
    }
}
