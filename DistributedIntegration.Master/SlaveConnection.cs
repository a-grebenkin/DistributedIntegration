using DistributedIntegration.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DistributedIntegration.Master
{
    public class SlaveConnection
    {
        private TcpClient client;
        private StreamWriter writer;
        private StreamReader reader;

        public IPEndPoint EndPoint => (IPEndPoint)client.Client.RemoteEndPoint;

        public SlaveConnection(TcpClient client)
        {
            this.client = client;
            var stream = client.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);
        }

        public async Task SendJobAsync(Job job)
        {
            var serializedJob = SerializationHelper.Serialize(job);
            await writer.WriteLineAsync(serializedJob);
            await writer.FlushAsync();
        }

        public async Task<JobResult> ReceiveResultAsync()
        {
            var serializedResult = await reader.ReadLineAsync();
            return SerializationHelper.Deserialize<JobResult>(serializedResult);
        }

        public void Close()
        {
            writer.Close();
            reader.Close();
            client.Close();
        }
    }
}
