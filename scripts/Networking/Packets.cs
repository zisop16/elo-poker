using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Godot;
using Poker;

public enum PacketType : byte { CLIENT_ACTION, SERVER_ACTION, JOIN_QUEUE, EXIT_QUEUE, LOBBY_START, HAND_START, CLIENT_LOBBY_JOIN_ACK, SERVER_QUEUE_ACK };

public readonly struct ClientActionPacket(Poker.Action action, int amount, int actionIndex) {
    public readonly PacketType Type = PacketType.CLIENT_ACTION;
    public readonly Poker.Action Action = action;
    public readonly int Amount = amount;
    public readonly int ActionIndex = actionIndex;
}

public struct ServerActionPacket {
    public readonly PacketType Type = PacketType.SERVER_ACTION;
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
    public bool HandEnd = false;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = TableSettings.MAX_PLAYERS)]
    Hand[] _tableHands;
    public Hand[] TableHands {
        get => _tableHands;
        set {
            _tableHands = new Hand[TableSettings.MAX_PLAYERS];
            if (value == null) {
                return;
            }
            for (int i = 0; i < value.Length; i++) {
                _tableHands[i] = value[i];
            }
        }
    }
    public ServerActionPacket(Poker.Action action, int amount, int actionIndex, PokerCard[] dealtBoardCards = null, Hand[] tableHands = null) {
        Action = action;
        Amount = amount;
        ActionIndex = actionIndex;
        DealtCards = dealtBoardCards;
        TableHands = tableHands;
        if (tableHands != null) {
            HandEnd = true;
        }
    }
}

public readonly struct HandStartPacket {
    public readonly PacketType Type = PacketType.HAND_START;
    public readonly Hand LocalHand;
    public readonly bool HandEnd = false;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = TableSettings.MAX_PLAYERS)]
    readonly Hand[] _tableHands;
    public Hand[] TableHands {
        get => _tableHands;
        private init {
            _tableHands = new Hand[TableSettings.MAX_PLAYERS];
            if (value == null) {
                return;
            }
            for (int i = 0; i < value.Length; i++) {
                _tableHands[i] = value[i];
            }
        }
    }
    public HandStartPacket(Hand localHand, Hand[] tableHands = null) {
        LocalHand = localHand;
        if (tableHands != null) {
            HandEnd = true;
        }
        TableHands = tableHands;
    }
}

public readonly struct JoinQueuePacket() {
    public readonly PacketType Type = PacketType.JOIN_QUEUE;
}

public readonly struct ExitQueuePacket() {
    public readonly PacketType Type = PacketType.EXIT_QUEUE;
}

public readonly struct ServerQueueAck(bool joined) {
    public readonly PacketType Type = PacketType.SERVER_QUEUE_ACK;
    public readonly bool Joined = joined;
    public bool Exitted { get => !Joined; }
}

public readonly struct ClientLobbyJoinAck() {
    public readonly PacketType Type = PacketType.CLIENT_LOBBY_JOIN_ACK;
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