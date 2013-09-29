using System.Collections.Generic;
using Newtonsoft.Json;

namespace cslib
{
    public class JsonSettings : ISettings
    {
        private Dictionary<string, dynamic> m_Settings;
        
        public JsonSettings(string json)
        {
            this.m_Settings = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json);
        }

        public dynamic Get()
        {
            return this.m_Settings;
        }
    }
}

