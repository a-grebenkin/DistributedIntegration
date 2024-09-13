using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DistributedIntegration.Common
{
    public interface ITask
    {
        object Execute(Dictionary<string, object> parameters);
    }

    [Serializable]
    public class Job
    {
        public string TaskName { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public byte[] AssemblyBytes { get; set; }
    }

    [Serializable]
    public class JobResult
    {
        public object Result { get; set; }
        public string ErrorMessage { get; set; }
    }

    public static class SerializationHelper
    {
        public static string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
