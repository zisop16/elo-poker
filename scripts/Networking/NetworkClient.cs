using Godot;
using Collections = Godot.Collections;
using System;

public partial class NetworkClient : Control {
    WebSocketMultiplayerPeer Peer;
    void StartClient() {
        X509Certificate cert = ResourceLoader.Load<X509Certificate>(NetworkServer.SERVER_CERT_PATH);
        TlsOptions opt = TlsOptions.ClientUnsafe(cert);
        Peer.CreateClient(NetworkServer.SERVER_URL, opt);
    }

    public PhysicalLobby Lobby { get; private set; }
    PhysicalQueue Queue;

    public override void _Ready() {
        Peer = new();
        StartClient();
        GD.Print("Client Started");
        Lobby = GetNode<PhysicalLobby>("PhysicalLobby");
        Queue = GetNode<PhysicalQueue>("PhysicalQueue");
        SetActive(Lobby, false);
        Global.Client = this;
    }

    public void HandleAction(Poker.Action action, int amount = 0) {
        ClientActionPacket pack = new(action, amount, Lobby.ActionIndex);
        byte[] bytes = Packet.ToBytes(pack);
        Peer.PutPacket(bytes);
        GD.Print("Sent ", action);
    }

    public void JoinQueue() {
        JoinQueuePacket pack = new();
        Peer.PutPacket(Packet.ToBytes(pack));
    }

    void HandleLobbyStartPacket(LobbyStartPacket pack) {
        Lobby.HandleLobbyStart(pack);
        SetActive(Queue, false);
        SetActive(Lobby, true);
    }

    void HandleActionPacket(ServerActionPacket pack) {
        Poker.Action action = pack.Action;
        int amount = pack.Amount;
        PokerCard[] newlyDealtCards = pack.DealtCards;
        Lobby.ReceiveAction(action, amount, pack.ActionIndex, newlyDealtCards);
    }

    void HandleNextPacket() {
        byte[] msg = Peer.GetPacket();
        PacketType type = (PacketType)msg[0];
        switch (type) {
            case PacketType.LOBBY_START:
                LobbyStartPacket lobbyPack = Packet.FromBytes<LobbyStartPacket>(msg);
                HandleLobbyStartPacket(lobbyPack);
                break;
            case PacketType.SERVER_ACTION:
                ServerActionPacket actionPack = Packet.FromBytes<ServerActionPacket>(msg);
                HandleActionPacket(actionPack);
                break;
        }
    }

    static void SetActive(Control parent, bool setting) {
        parent.Visible = setting;
        foreach (Control child in parent.GetChildren()) {
            // SetActiveRecursive(child, setting);
        }
    }

    static void SetActiveRecursive(Control element, bool setting) {
        if (setting) {
            element.MouseFilter = MouseFilterEnum.Stop;
        } else {
            element.MouseFilter = MouseFilterEnum.Ignore;
        }
        foreach (Control child in element.GetChildren()) {
            SetActiveRecursive(child, setting);
        }
    }


    public override void _Process(double delta) {
        Peer.Poll();
        while (Peer.GetAvailablePacketCount() > 0) {
            HandleNextPacket();
        }
    }

    public override void _UnhandledInput(InputEvent ev) {
        // GD.Print(ev);
    }
}
