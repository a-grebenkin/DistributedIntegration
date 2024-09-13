using System.Net.Sockets;
using System.Net;
using DistributedIntegration.Common;
using System.Diagnostics;

namespace DistributedIntegration.Master
{
    public partial class MasterForm : Form
    {
        private string taskLibPath;
        private List<SlaveConnection> slaveConnections = new List<SlaveConnection>();
        private TcpListener listener;
        private bool isListening = false;

        public MasterForm()
        {
            InitializeComponent();
            listener = new TcpListener(IPAddress.Any, 12345);
            StartListening();
        }

        private async void StartListening()
        {
            try
            {
                listener.Start();
                isListening = true;
                //lblStatus.Text = "Listening for slaves...";

                while (isListening)
                {
                    var client = await listener.AcceptTcpClientAsync();
                    var slaveConnection = new SlaveConnection(client);
                    slaveConnections.Add(slaveConnection);
                    UpdateClientList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while listening: {ex.Message}");
            }
        }

        private void UpdateClientList()
        {
            Invoke((MethodInvoker)delegate
            {
                lstClients.Items.Clear();
                foreach (var slaveConnection in slaveConnections)
                {
                    lstClients.Items.Add(slaveConnection.EndPoint.ToString());
                }
                //lblConnectedSlaves.Text = $"Connected Slaves: {slaveConnections.Count}";
            });
        }

        private void btnSelectDll_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "DLL files (*.dll)|*.dll";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    taskLibPath = openFileDialog.FileName;
                    txtDllPath.Text = taskLibPath;
                    btnExecute.Enabled = true;
                }
            }
        }

        private async void btnExecute_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(taskLibPath))
            {
                MessageBox.Show("Please select a DLL file first.");
                return;
            }

            if (slaveConnections.Count == 0)
            {
                MessageBox.Show("No slaves connected. Please wait for slaves to connect.");
                return;
            }

            btnExecute.Enabled = false;
            //lblStatus.Text = "Executing task...";

            var job = new Job
            {
                TaskName = "IntegrateTask",
                Parameters = new Dictionary<string, object>
                {
                    { "a", 0.0 },
                    { "b", Math.PI },
                    { "n", 1000 },
                    { "function", "sin" }
                },
                AssemblyBytes = File.ReadAllBytes(taskLibPath)
            };

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var tasks = slaveConnections.Select(connection => ExecuteOnSlaveAsync(connection, job)).ToList();
            await Task.WhenAll(tasks);

            stopwatch.Stop();
            //lblStatus.Text = $"Task completed in {stopwatch.ElapsedMilliseconds} ms";
            btnExecute.Enabled = true;
        }

        private async Task ExecuteOnSlaveAsync(SlaveConnection slaveConnection, Job job)
        {
            try
            {
                await slaveConnection.SendJobAsync(job);
                var result = await slaveConnection.ReceiveResultAsync();
                UpdateResult(result, slaveConnection.EndPoint.ToString());
            }
            catch (Exception ex)
            {
                Invoke((MethodInvoker)delegate
                {
                    txtResult.AppendText($"Error from {slaveConnection.EndPoint}: {ex.Message}\r\n");
                });
                slaveConnections.Remove(slaveConnection);
                UpdateClientList();
            }
        }

        private void UpdateResult(JobResult result, string slaveEndpoint)
        {
            Invoke((MethodInvoker)delegate
            {
                if (string.IsNullOrEmpty(result.ErrorMessage))
                {
                    txtResult.AppendText($"Result from {slaveEndpoint}: {result.Result}\r\n");
                }
                else
                {
                    txtResult.AppendText($"Error from {slaveEndpoint}: {result.ErrorMessage}\r\n");
                }
            });
        }

        private void MasterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            isListening = false;
            listener.Stop();
            foreach (var slaveConnection in slaveConnections)
            {
                slaveConnection.Close();
            }
        }
    }
}
