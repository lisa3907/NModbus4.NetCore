﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using Modbus.Device;
using Xunit;

namespace Modbus.IntegrationTests;

public class NModbusTcpSlaveFixture
{
    /// <summary>
    /// Tests the scenario when a slave is closed unexpectedly, causing a ConnectionResetByPeer SocketException
    /// We want to handle this gracefully - remove the master from the dictionary
    /// </summary>
    [Fact(Skip = "TestDriver.exe")]
    public void ModbusTcpSlave_ConnectionResetByPeer()
    {
        TcpListener slaveListener = new(ModbusMasterFixture.TcpHost, ModbusMasterFixture.Port);
        using ModbusTcpSlave slave = ModbusTcpSlave.CreateTcp(ModbusMasterFixture.SlaveAddress, slaveListener);
        Thread slaveThread = new(async () => await slave.ListenAsync());
        slaveThread.IsBackground = true;
        slaveThread.Start();

        Thread.Sleep(500);

        using (Process masterProcess = Process.Start(Path.Combine(
            Path.GetDirectoryName(Assembly.GetAssembly(typeof(NModbusTcpSlaveFixture)).Location),
            @"..\..\..\..\tools\nmodbus\TestDriver.exe")))
        {
            Thread.Sleep(2000);
            masterProcess.Kill();
        }

        Thread.Sleep(2000);
        Assert.Empty(slave.Masters);
    }

    /// <summary>
    /// Tests possible exception when master closes gracefully immediately after transaction
    /// The goal is the test the exception in WriteCompleted when the slave attempts to read another request from an already closed master
    /// </summary>
    [Fact]
    public void ModbusTcpSlave_ConnectionClosesGracefully()
    {
        TcpListener slaveListener = new(ModbusMasterFixture.TcpHost, ModbusMasterFixture.Port);
        using ModbusTcpSlave slave = ModbusTcpSlave.CreateTcp(ModbusMasterFixture.SlaveAddress, slaveListener);
        Thread slaveThread = new(async () => await slave.ListenAsync());
        slaveThread.IsBackground = true;
        slaveThread.Start();

        TcpClient masterClient = new(ModbusMasterFixture.TcpHost.ToString(), ModbusMasterFixture.Port);
        using (ModbusIpMaster master = ModbusIpMaster.CreateIp(masterClient))
        {
            master.Transport.Retries = 0;

            bool[] coils = master.ReadCoils(1, 1);

            Assert.Single(coils);
            Assert.Single(slave.Masters);
        }

        // give the slave some time to remove the master
        Thread.Sleep(50);

        Assert.Empty(slave.Masters);
    }

    /// <summary>
    /// Tests possible exception when master closes gracefully and the ReadHeaderCompleted EndRead call returns 0 bytes;
    /// </summary>
    [Fact]
    public void ModbusTcpSlave_ConnectionSlowlyClosesGracefully()
    {
        TcpListener slaveListener = new(ModbusMasterFixture.TcpHost, ModbusMasterFixture.Port);
        using ModbusTcpSlave slave = ModbusTcpSlave.CreateTcp(ModbusMasterFixture.SlaveAddress, slaveListener);
        Thread slaveThread = new(async () => await slave.ListenAsync());
        slaveThread.IsBackground = true;
        slaveThread.Start();

        TcpClient masterClient = new(ModbusMasterFixture.TcpHost.ToString(), ModbusMasterFixture.Port);
        using (ModbusIpMaster master = ModbusIpMaster.CreateIp(masterClient))
        {
            master.Transport.Retries = 0;

            bool[] coils = master.ReadCoils(1, 1);
            Assert.Single(coils);

            Assert.Single(slave.Masters);

            // wait a bit to let slave move on to read header
            Thread.Sleep(50);
        }

        // give the slave some time to remove the master
        Thread.Sleep(50);
        Assert.Empty(slave.Masters);
    }

    [Fact]
    public void ModbusTcpSlave_MultiThreaded()
    {
        TcpListener slaveListener = new(ModbusMasterFixture.TcpHost, ModbusMasterFixture.Port);
        using ModbusTcpSlave slave = ModbusTcpSlave.CreateTcp(ModbusMasterFixture.SlaveAddress, slaveListener);
        Thread slaveThread = new(async () => await slave.ListenAsync());
        slaveThread.IsBackground = true;
        slaveThread.Start();

        Thread workerThread1 = new(Read);
        Thread workerThread2 = new(Read);
        workerThread1.Start();
        workerThread2.Start();

        workerThread1.Join();
        workerThread2.Join();
    }

    private static void Read(object state)
    {
        TcpClient masterClient = new(ModbusMasterFixture.TcpHost.ToString(), ModbusMasterFixture.Port);
        using ModbusIpMaster master = ModbusIpMaster.CreateIp(masterClient);
        master.Transport.Retries = 0;

        Random random = new();
        for (int i = 0; i < 5; i++)
        {
            bool[] coils = master.ReadCoils(1, 1);
            Assert.Single(coils);
            Debug.WriteLine($"{Environment.CurrentManagedThreadId}: Reading coil value");
            Thread.Sleep(random.Next(100));
        }
    }
}