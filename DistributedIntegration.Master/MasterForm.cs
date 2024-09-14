using System.Net.Sockets;
using System.Net;
using DistributedIntegration.Common;
using System.Diagnostics;

namespace DistributedIntegration.Master
{
    public partial class MasterForm : Form
    {
        private string taskLibPath;
        private string dataFilePath;
        private List<SlaveConnection> slaveConnections = new List<SlaveConnection>();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
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

                    // Start monitoring this slave connection
                    _ = MonitorSlaveConnectionAsync(slaveConnection, cancellationTokenSource.Token);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while listening: {ex.Message}");
            }
        }

        private async Task MonitorSlaveConnectionAsync(SlaveConnection slaveConnection, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (!await slaveConnection.IsConnectedAsync())
                    {
                        RemoveSlaveConnection(slaveConnection);
                        break;
                    }
                    await Task.Delay(5000, cancellationToken); // Check every 5 seconds
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation, do nothing
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error monitoring slave connection: {ex.Message}");
                RemoveSlaveConnection(slaveConnection);
            }
        }

        private void RemoveSlaveConnection(SlaveConnection slaveConnection)
        {
            slaveConnection.Close();
            slaveConnections.Remove(slaveConnection);
            UpdateClientList();
        }

        private void UpdateClientList()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateClientList));
                return;
            }

            lstClients.Items.Clear();
            foreach (var slaveConnection in slaveConnections)
            {
                lstClients.Items.Add(slaveConnection.EndPoint.ToString());
            }
            //lblConnectedSlaves.Text = $"Connected Slaves: {slaveConnections.Count}";
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

        private void btnSelectDataFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Text files (*.txt)|*.txt";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    dataFilePath = openFileDialog.FileName;
                    txtDataFilePath.Text = dataFilePath;
                }
            }
        }

        private async void btnExecute_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(taskLibPath) || string.IsNullOrEmpty(dataFilePath) || slaveConnections.Count == 0)
            {
                MessageBox.Show("Please select a DLL file, a data file, and ensure slaves are connected.");
                return;
            }

            btnExecute.Enabled = false;
            txtResult.Clear();

            var jobs = ReadJobsFromFile(dataFilePath);
            foreach (var job in jobs)
            {
                await ExecuteJobAndCompare(job);
            }

            btnExecute.Enabled = true;
        }

        private List<Job> ReadJobsFromFile(string filePath)
        {
            var jobs = new List<Job>();
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length != 3) continue;

                jobs.Add(new Job
                {
                    TaskName = "IntegrateTask",
                    Parameters = new Dictionary<string, object>
                    {
                        { "a", double.Parse(parts[0]) },
                        { "b", double.Parse(parts[1]) },
                        { "n", int.Parse(parts[2]) },
                    },
                    AssemblyBytes = File.ReadAllBytes(taskLibPath)
                });
            }
            return jobs;
        }

        private async Task ExecuteJobAndCompare(Job job)
        {
            // Execute on all slaves
            var stopwatchAll = Stopwatch.StartNew();
            var distributedResult = await ExecuteDistributed(job);
            stopwatchAll.Stop();

            // Execute on single slave
            var stopwatchSingle = Stopwatch.StartNew();
            var singleResult = await ExecuteSingle(job);
            stopwatchSingle.Stop();

            // Calculate time difference
            double timeDifference = ((double)stopwatchAll.ElapsedMilliseconds / stopwatchSingle.ElapsedMilliseconds - 1) * -100;

            // Update result text box
            UpdateResultTextBox($"Job: a={job.Parameters["a"]}, b={job.Parameters["b"]}, n={job.Parameters["n"]}\r\n" +
                                $"Distributed Result: {distributedResult}, Time: {stopwatchAll.ElapsedMilliseconds}ms\r\n" +
                                $"Single Result: {singleResult}, Time: {stopwatchSingle.ElapsedMilliseconds}ms\r\n" +
                                $"Time Difference: {timeDifference:F2}%\r\n\r\n");
        }
        private async Task<double> ExecuteDistributed(Job job)
        {
            var subJobs = DistributeJob(job, slaveConnections.Count);
            var tasks = slaveConnections.Zip(subJobs, (conn, subJob) => ExecuteOnSlaveAsync(conn, subJob)).ToList();
            var results = await Task.WhenAll(tasks);
            return ConsolidateResults(results);
        }

        private async Task<double> ExecuteSingle(Job job)
        {
            var selectedSlave = slaveConnections[new Random().Next(slaveConnections.Count)];
            return await ExecuteOnSlaveAsync(selectedSlave, job);
        }

        private List<Job> DistributeJob(Job originalJob, int slaveCount)
        {
            var subJobs = new List<Job>();
            double start = (double)originalJob.Parameters["a"];
            double end = (double)originalJob.Parameters["b"];
            int totalIterations = (int)originalJob.Parameters["n"];

            double range = end - start;
            int iterationsPerSlave = totalIterations / slaveCount;

            for (int i = 0; i < slaveCount; i++)
            {
                double subStart = start + (range * i / slaveCount);
                double subEnd = start + (range * (i + 1) / slaveCount);
                int subIterations = (i == slaveCount - 1) ? totalIterations - (iterationsPerSlave * i) : iterationsPerSlave;

                var subJob = new Job
                {
                    TaskName = originalJob.TaskName,
                    Parameters = new Dictionary<string, object>(originalJob.Parameters)
                    {
                        ["a"] = subStart,
                        ["b"] = subEnd,
                        ["n"] = subIterations
                    },
                    AssemblyBytes = originalJob.AssemblyBytes
                };

                subJobs.Add(subJob);
            }

            return subJobs;
        }

        private double ConsolidateResults(double[] results)
        {
            // For integration, we can simply sum up all the partial results
            return results.Sum();
        }

        private async Task<double> ExecuteOnSlaveAsync(SlaveConnection slaveConnection, Job job)
        {
            try
            {
                await slaveConnection.SendJobAsync(job);
                var result = await slaveConnection.ReceiveResultAsync();
                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    throw new Exception(result.ErrorMessage);
                }
                return (double)result.Result;
            }
            catch (Exception ex)
            {
                UpdateResultTextBox($"Error from {slaveConnection.EndPoint}: {ex.Message}\r\n");
                RemoveSlaveConnection(slaveConnection);
                return 0; // or handle this error case as appropriate for your application
            }
        }

        private void UpdateResultTextBox(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateResultTextBox), message);
                return;
            }

            txtResult.AppendText(message);
        }

        private void MasterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            isListening = false;
            listener.Stop();
            cancellationTokenSource.Cancel();
            foreach (var slaveConnection in slaveConnections)
            {
                slaveConnection.Close();
            }
        }
    }
}
