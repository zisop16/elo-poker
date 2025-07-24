using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Godot;
using Poker;

public enum PacketType : byte { ACTION, JOIN, LOBBY_START };

public struct ActionPacket {
    public readonly PacketType Type = PacketType.ACTION;
    public readonly Poker.Action Action;
    public readonly int Amount;
    public readonly int ActionIndex;
    public int NumDealtCards { get; private set; }
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = PokerGame.BOARD_SIZE)]
    PokerCard[] _dealtCards;
    public PokerCard[] DealtCards {
        get {
            if (NumDealtCards == 0) return null;
            PokerCard[] compressed = new PokerCard[NumDealtCards];
            for (int i = 0; i < NumDealtCards; i++) {
                compressed[i] = _dealtCards[i];
            }
            return compressed;
        }
        set {
            PokerCard[] expanded = new PokerCard[PokerGame.BOARD_SIZE];
            if (value == null) {
                _dealtCards = expanded;
                NumDealtCards = 0;
                return;
            }
            for (int i = 0; i < value.Length; i++) {
                expanded[i] = value[i];
            }
            _dealtCards = expanded;
            NumDealtCards = value.Length;
        }
    }
    public ActionPacket(Poker.Action action, int amount, int actionIndex, PokerCard[] dealtBoardCards = null) {
        Action = action;
        Amount = amount;
        ActionIndex = actionIndex;
        DealtCards = dealtBoardCards;
    }
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