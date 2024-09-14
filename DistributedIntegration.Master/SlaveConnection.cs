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
        private NetworkStream stream;
        private StreamWriter writer;
        private StreamReader reader;

        public IPEndPoint EndPoint => (IPEndPoint)client.Client.RemoteEndPoint;

        public SlaveConnection(TcpClient client)
        {
            this.client = client;
            stream = client.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);
        }

        public async Task<bool> IsConnectedAsync()
        {
            if (!client.Connected)
                return false;

            try
            {
                await Task.Run(() =>
                {
                    client.Client.Send(new byte[1], 0, 0);
                });
                return true;
            }
            catch
            {
                return false;
            }
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
            stream.Close();
            client.Close();
        }
    }
}
