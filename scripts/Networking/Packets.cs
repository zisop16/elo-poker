using System;
using System.IO;
using System.Runtime.InteropServices;

public enum PacketType : byte { ACTION, JOIN };

public readonly struct ActionPacket(Poker.Action action, uint amount = 0) {
    public readonly PacketType Type = PacketType.ACTION;
    public readonly Poker.Action Action = action;
    public readonly uint Amount = amount;
}

public readonly struct JoinPacket() {
    public readonly PacketType Type = PacketType.JOIN;
}

public static class Packet {
    /// <summary>
    /// Reads in a block from a file and converts it to the struct
    /// type specified by the template parameter
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="reader"></param>
    /// <returns></returns>
    public static T FromBytes<T>(byte[] arr)
    {
        T pack;

        int size = Marshal.SizeOf(typeof(T));
        IntPtr ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            pack = (T)Marshal.PtrToStructure(ptr, typeof(T));
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
        return pack;
    }
    public static byte[] ToBytes<T>(T pack) {
        int size = Marshal.SizeOf(pack);
        byte[] arr = new byte[size];

        IntPtr ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(pack, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
        return arr;
    }
}