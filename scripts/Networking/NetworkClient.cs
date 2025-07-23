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

    PhysicalLobby Lobby;
    PhysicalQueue Queue;

    public override void _Ready() {
        Peer = new();
        StartClient();
        GD.Print("Client Started");
        Lobby = GetNode<PhysicalLobby>("PhysicalLobby");
        Queue = GetNode<PhysicalQueue>("PhysicalQueue");
        SetActive(Lobby, false);
    }

    public void HandleAction(Poker.Action action, int amount = 0) {
        ActionPacket pack = new(action, amount);
        byte[] bytes = Packet.ToBytes(pack);
        Peer.PutPacket(bytes);
        GD.Print("Sent ", action);
    }

    public void JoinQueue() {
        JoinPacket pack = new();
        Peer.PutPacket(Packet.ToBytes(pack));
    }

    void HandleLobbyStartPacket(LobbyStartPacket pack) {
        Lobby.HandleLobbyStart(pack);
        SetActive(Queue, false);
        SetActive(Lobby, true);
    }

    void HandleNextPacket() {
        byte[] msg = Peer.GetPacket();
        PacketType type = (PacketType)msg[0];
        switch (type) {
            case PacketType.LOBBY_START:
                LobbyStartPacket pack = Packet.FromBytes<LobbyStartPacket>(msg);
                HandleLobbyStartPacket(pack);
                break;
        }
    }

    static void SetActive(Control parent, bool setting) {
        parent.Visible = setting;
        foreach (Control child in parent.GetChildren()) {
            SetActiveRecursive(child, setting);
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
