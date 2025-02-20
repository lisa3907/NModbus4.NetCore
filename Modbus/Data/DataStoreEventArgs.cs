﻿using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Modbus.Utility;

namespace Modbus.Data;

/// <summary>
///     Event args for read write actions performed on the DataStore.
/// </summary>
public class DataStoreEventArgs : EventArgs
{
    private DataStoreEventArgs(ushort startAddress, ModbusDataType modbusDataType)
    {
        StartAddress = startAddress;
        ModbusDataType = modbusDataType;
    }

    /// <summary>
    ///     Type of Modbus data (e.g. Holding register).
    /// </summary>
    public ModbusDataType ModbusDataType { get; }

    /// <summary>
    ///     Start address of data.
    /// </summary>
    public ushort StartAddress { get; }

    /// <summary>
    ///     Data that was read or written.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public DiscriminatedUnion<ReadOnlyCollection<bool>, ReadOnlyCollection<ushort>> Data { get; private set; }

    internal static DataStoreEventArgs CreateDataStoreEventArgs<T>(ushort startAddress, ModbusDataType modbusDataType, IEnumerable<T> data)
    {
        ArgumentNullException.ThrowIfNull(data);

        DataStoreEventArgs eventArgs;

        if (typeof(T) == typeof(bool))
        {
            ReadOnlyCollection<bool> a = new(data.Cast<bool>().ToArray());

            eventArgs = new DataStoreEventArgs(startAddress, modbusDataType)
            {
                Data = DiscriminatedUnion<ReadOnlyCollection<bool>, ReadOnlyCollection<ushort>>.CreateA(a)
            };
        }
        else if (typeof(T) == typeof(ushort))
        {
            ReadOnlyCollection<ushort> b = new(data.Cast<ushort>().ToArray());

            eventArgs = new DataStoreEventArgs(startAddress, modbusDataType)
            {
                Data = DiscriminatedUnion<ReadOnlyCollection<bool>, ReadOnlyCollection<ushort>>.CreateB(b)
            };
        }
        else
        {
            throw new ArgumentException("Generic type T should be of type bool or ushort");
        }

        return eventArgs;
    }
}