using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Poker;

public enum PacketType : byte { ACTION, JOIN, LOBBY_START };

public readonly struct ActionPacket(Poker.Action action, int amount = 0) {
    public readonly PacketType Type = PacketType.ACTION;
    public readonly Poker.Action Action = action;
    public readonly int Amount = amount;
}

public readonly struct JoinPacket() {
    public readonly PacketType Type = PacketType.JOIN;
}

public readonly struct LobbyStartPacket(TablePreset preset, int seatNumber, PokerGame game) {
    public readonly PacketType Type = PacketType.LOBBY_START;
    readonly TablePreset Preset = preset;
    public TableSettings Settings { get => TableSettings.GetPreset(Preset); }
    public readonly int SeatNumber = seatNumber;
    public readonly int BigBlindPosition = game.BigBlindPosition.Val;
    public readonly Hand Hand = game.Players[seatNumber].Hand;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = TableSettings.MAX_PLAYERS)]
    public readonly int[] PlayerIDs = game.PlayerIDs;
}

public static class Packet {
    /// <summary>
    /// Reads in a block from a file and converts it to the struct
    /// type specified by the template parameter
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="reader"></param>
    /// <returns></returns>
    public static T FromBytes<T>(byte[] arr) {
        T pack;

        int size = Marshal.SizeOf(typeof(T));
        IntPtr ptr = IntPtr.Zero;
        try {
            ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            pack = (T)Marshal.PtrToStructure(ptr, typeof(T));
        } finally {
            Marshal.FreeHGlobal(ptr);
        }
        return pack;
    }
    public static byte[] ToBytes<T>(T pack) {
        int size = Marshal.SizeOf(pack);
        byte[] arr = new byte[size];

        IntPtr ptr = IntPtr.Zero;
        try {
            ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(pack, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
        } finally {
            Marshal.FreeHGlobal(ptr);
        }
        return arr;
    }
}