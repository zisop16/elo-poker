using Godot;
using System;
using System.Text;
using Poker;
using System.Collections.Generic;

public partial class NetworkServer : Node {
    WebSocketMultiplayerPeer Peer;
    public const int PORT = 42069;
    public const string IP_ADDRESS = "localhost";
    public static readonly string SERVER_URL = "wss://" + IP_ADDRESS + ":" + PORT;
    public const string SERVER_CERT_PATH = "res://resources/encryption/server_CAS.crt";
    public const string SERVER_KEY_PATH = "res://resources/encryption/server_key.key";

    public override void _Ready() {
        StartServer();
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
                case PacketType.CLIENT_ACTION:
                    ClientActionPacket actPack = Packet.FromBytes<ClientActionPacket>(rawBytes);
                    ActionError result = HandleAction(actPack, source);
                    break;
                case PacketType.JOIN:
                    JoinQueuePacket joinPack = Packet.FromBytes<JoinQueuePacket>(rawBytes);
                    HandleJoin(joinPack, source);
                    break;
            }
        }
    }

    enum ActionError { NotInGame, NotCurrentTurn, InvalidAction, Outdated, Success };
    ActionError HandleAction(ClientActionPacket pack, int peerID) {
        if (!PlayerIDLobby.TryGetValue(peerID, out NetworkLobby lobby)) return ActionError.NotInGame;
        if (pack.ActionIndex != lobby.NextActionIndex) return ActionError.Outdated;
        if (!lobby.PlayerIsActing(peerID)) return ActionError.NotCurrentTurn;
        Poker.Action action = pack.Action;
        int amount = pack.Amount;
        ActionResult result = lobby.Act(action, amount);
        if (result.Success) {
            ServerActionPacket response = new(pack.Action, pack.Amount, pack.ActionIndex);
            if (result.StreetChange) {
                response.DealtCards = result.NewCards;
            }
            foreach (int p in lobby.ConnectedPlayers) {
                if (!result.StreetChange && p == peerID) continue;
                Peer.SetTargetPeer(p);
                Peer.PutPacket(Packet.ToBytes(response));
            }
            return ActionError.Success;
        }
        return ActionError.InvalidAction;
    }

    readonly struct QueuedPlayer(int peerID) {
        public readonly int PeerID = peerID;
        readonly double queueStart = Global.Time;
        readonly double TimeQueued { get => Global.Time - queueStart; }
    }

    readonly List<QueuedPlayer> PlayerQueue = [];
    enum JoinError : byte {
        Success, LobbyFull, AlreadyJoined
    }

    JoinError HandleJoin(JoinQueuePacket pack, int peerID) {
        if (PlayerQueue.Exists(x => x.PeerID == peerID)) return JoinError.AlreadyJoined;
        PlayerQueue.Add(new QueuedPlayer(peerID));
        return JoinError.Success;
    }

    [Export]
    TableSettings HeadsUp;
    TablePreset NextGamePreset = TablePreset.NONE;
    TableSettings NextGameSettings { get => TableSettings.GetPreset(NextGamePreset); }
    readonly List<NetworkLobby> RunningLobbies = [];
    readonly Dictionary<int, NetworkLobby> PlayerIDLobby = [];
    void SelectNextGame() {
        NextGamePreset = TablePreset.HEADS_UP;
    }

    /// <summary>
    /// Grabs the next set of viable players based on matchmaking, and removes them from the playerqueue
    /// </summary>
    /// <returns>Array of player IDs if success, else null</returns>
    int[] NextPlayerSet() {
        int requiredCount = NextGameSettings.NumPlayers;
        int currentCount = PlayerQueue.Count;
        if (currentCount < requiredCount) return null;
        int[] players = new int[requiredCount];
        for (int i = 0; i < requiredCount; i++) {
            int QueueInd = PlayerQueue.Count - 1;
            QueuedPlayer curr = PlayerQueue[QueueInd];
            PlayerQueue.RemoveAt(QueueInd);
            players[i] = curr.PeerID;
        }
        return players;
    }

    void HandlePlayerQueue() {
        if (NextGamePreset == TablePreset.NONE) SelectNextGame();
        int[] players = NextPlayerSet();
        if (players == null) return;
        StartLobby(players);
        HandlePlayerQueue();
    }

    void StartLobby(int[] players) {
        NetworkLobby lobby = new(NextGameSettings, players);
        RunningLobbies.Add(lobby);
        lobby.Deal();
        for(int i = 0; i < NextGameSettings.NumPlayers; i++) {
            int p = players[i];
            LobbyStartPacket pack = new(TablePreset.HEADS_UP, i, lobby.Game);
            Peer.SetTargetPeer(p);
            Peer.PutPacket(Packet.ToBytes(pack));
            PlayerIDLobby.Add(p, lobby);
        }
        NextGamePreset = TablePreset.NONE;
    }

    public override void _Process(double delta) {
        Peer.Poll();
        HandleIncomingPackets();
        HandlePlayerQueue();
    }
}
