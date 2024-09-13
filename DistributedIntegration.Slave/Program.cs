using DistributedIntegration.Slave;

string masterIp = "192.168.31.46";
int masterPort = 12345;

var slave = new Slave(masterIp, masterPort);
await slave.Start();