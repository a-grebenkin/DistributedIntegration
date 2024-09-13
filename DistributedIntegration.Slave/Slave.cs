using DistributedIntegration.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DistributedIntegration.Slave
{
    public class Slave
    {
        private readonly string masterIp;
        private readonly int masterPort;
        private TcpClient client;
        private StreamWriter writer;
        private StreamReader reader;

        public Slave(string masterIp, int masterPort)
        {
            this.masterIp = masterIp;
            this.masterPort = masterPort;
        }

        public async Task Start()
        {
            while (true)
            {
                try
                {
                    await ConnectToMaster();
                    await ProcessJobs();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine("Attempting to reconnect in 5 seconds...");
                    await Task.Delay(5000);
                }
                finally
                {
                    CloseConnection();
                }
            }
        }

        private async Task ConnectToMaster()
        {
            client = new TcpClient();
            await client.ConnectAsync(masterIp, masterPort);
            Console.WriteLine($"Connected to master at {masterIp}:{masterPort}");

            var stream = client.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);
        }

        private async Task ProcessJobs()
        {
            while (true)
            {
                var job = await ReceiveJobAsync();
                var result = ExecuteJob(job);
                await SendResultAsync(result);
            }
        }

        private async Task<Job> ReceiveJobAsync()
        {
            var serializedJob = await reader.ReadLineAsync();
            if (serializedJob == null)
            {
                throw new Exception("Connection closed by master");
            }
            return SerializationHelper.Deserialize<Job>(serializedJob);
        }

        private JobResult ExecuteJob(Job job)
        {
            try
            {
                var assembly = Assembly.Load(job.AssemblyBytes);
                var type = assembly.GetTypes().FirstOrDefault(t => typeof(ITask).IsAssignableFrom(t) && t.Name == job.TaskName);

                if (type == null)
                {
                    throw new Exception($"Invalid assembly: {job.TaskName} not found or doesn't implement ITask");
                }

                var task = (ITask)Activator.CreateInstance(type);
                var result = task.Execute(job.Parameters);
                return new JobResult { Result = result };
            }
            catch (Exception ex)
            {
                return new JobResult { ErrorMessage = ex.Message };
            }
        }

        private async Task SendResultAsync(JobResult result)
        {
            var serializedResult = SerializationHelper.Serialize(result);
            await writer.WriteLineAsync(serializedResult);
            await writer.FlushAsync();
        }

        private void CloseConnection()
        {
            writer?.Close();
            reader?.Close();
            client?.Close();
        }
    }
}
