using DistributedIntegration.Slave;

string masterIp = "127.0.0.1";
int masterPort = 12345;

var slave = new Slave(masterIp, masterPort);
await slave.Start();