using Godot;
using System;
using System.Text;
using Poker;
using System.Collections.Generic;

public partial class PokerServer : Node {
    WebSocketMultiplayerPeer Peer;
    Node Lobbies;
    public const int PORT = 42069;
    public const string IP_ADDRESS = "localhost";
    public static readonly string SERVER_URL = "wss://" + IP_ADDRESS + ":" + PORT;
    public const string SERVER_CERT_PATH = "res://resources/encryption/server_CAS.crt";
    public const string SERVER_KEY_PATH = "res://resources/encryption/server_key.key";

    public override void _Ready() {
        StartServer();
        Lobbies = GetNode("Lobbies");
        GD.Print("Server Started");
    }

    void StartServer() {
        bool certExists = ResourceLoader.Exists(SERVER_CERT_PATH);
        bool keyExists = ResourceLoader.Exists(SERVER_KEY_PATH);
        if (!certExists || !keyExists) {
            GenerateCertificate();
        }
        X509Certificate cert = ResourceLoader.Load<X509Certificate>(SERVER_CERT_PATH);
        CryptoKey key = ResourceLoader.Load<CryptoKey>(SERVER_KEY_PATH);
        TlsOptions opt = TlsOptions.Server(key, cert);
        Peer = new();
        Peer.CreateServer(PORT, "*", opt);
    }

    void GenerateCertificate() {
        Crypto crypto = new();
        CryptoKey key;
        X509Certificate cert;

        key = crypto.GenerateRsa(4096);
        cert = crypto.GenerateSelfSignedCertificate(key, "CN=dinkis.xyz, O=DinkisCorp, C=US");

        key.Save("res://resources/server_key.key");
        cert.Save("res://resources/server_CAS.crt");
    }

    void HandleIncomingPackets() {
        while (Peer.GetAvailablePacketCount() > 0) {
            int source = Peer.GetPacketPeer();
            byte[] rawBytes = Peer.GetPacket();
            PacketType type = (PacketType)rawBytes[0];
            switch (type) {
                case PacketType.ACTION:
                    ActionPacket actPack = Packet.FromBytes<ActionPacket>(rawBytes);
                    HandleAction(actPack, source);
                    break;
                case PacketType.JOIN:
                    JoinPacket joinPack = Packet.FromBytes<JoinPacket>(rawBytes);
                    HandleJoin(joinPack, source);
                    break;
            }
        }
    }

    void HandleAction(ActionPacket pack, int peerID) {
        Poker.Action action = pack.Action;
        switch (action) {
            case Poker.Action.CHECK:
                GD.Print("Check");
                break;
            case Poker.Action.BET:
                uint amount = pack.Amount;
                GD.Print("Bet ", amount);
                break;
            case Poker.Action.CALL:
                GD.Print("Call");
                break;
            case Poker.Action.FOLD:
                GD.Print("Fold");
                break;
        }
    }

    readonly struct QueuedPlayer(int peerID) {
        public readonly int PeerID = peerID;
        readonly double queueStart = Global.Time;
        readonly double TimeQueued { get => Global.Time - queueStart; }
    }

    List<QueuedPlayer> PlayerQueue = [];
    enum JoinError : byte {
        Success, LobbyFull, AlreadyJoined
    }


    JoinError HandleJoin(JoinPacket pack, int peerID) {
        if (PlayerQueue.Exists(x => x.PeerID == peerID)) return JoinError.AlreadyJoined;
        PlayerQueue.Add(new QueuedPlayer(peerID));
        return JoinError.Success;
    }

    [Export]
    TableSettings HeadsUp;
    TableSettings NextGameSettings = null;
    readonly List<PokerLobby> RunningLobbies = [];
    void SelectNextGame() {
        NextGameSettings = HeadsUp;
    }

    /// <summary>
    /// Grabs the next set of viable players based on matchmaking, and removes them from the playerqueue
    /// </summary>
    /// <returns>Array of player IDs if success, else null</returns>
    int[] NextPlayerSet() {
        int requiredCount = NextGameSettings.NumPlayers;
        int currentCount = PlayerQueue.Count;
        if (requiredCount < currentCount) return null;
        int[] players = new int[requiredCount];
        for (int i = 0; i < requiredCount; i++) {
            int QueueInd = PlayerQueue.Count - 1;
            QueuedPlayer curr = PlayerQueue[QueueInd];
            PlayerQueue.RemoveAt(i);
            players[i] = curr.PeerID;
        }
        return players;
    }

    void HandlePlayerQueue() {
        if (NextGameSettings == null) SelectNextGame();
        int[] players = NextPlayerSet();
        if (players == null) return;
        PokerLobby lobby = new(NextGameSettings, players);
        RunningLobbies.Add(lobby);
        NextGameSettings = null;
        HandlePlayerQueue();
    }

    public override void _Process(double delta) {
        Peer.Poll();
        HandleIncomingPackets();
        HandlePlayerQueue();
    }
}
