using Godot;
using System;

public partial class PokerClient : Control {
    WebSocketMultiplayerPeer Peer;
    void StartClient() {
        X509Certificate cert = ResourceLoader.Load<X509Certificate>(PokerServer.SERVER_CERT_PATH);
        TlsOptions opt = TlsOptions.ClientUnsafe(cert);
        Peer.CreateClient(PokerServer.SERVER_URL, opt);
    }

    PokerUI UserInterface;

    [Export]
    TableSettings Settings;
    PokerGame Game;

    

    public override void _Ready() {
        Peer = new();
        StartClient();
        GD.Print("Client Started");
        UserInterface = GetNode<PokerUI>("PokerUI");
        UserInterface.PokerAction += SendAction;
        Game = new PokerGame(Settings);
    }

    void SendAction(Poker.Action action, uint amount) {
        ActionPacket pack = new(action, amount);
        byte[] bytes = Packet.ToBytes(pack);
        Peer.PutPacket(bytes);
    }


    public override void _Process(double delta) {
        Peer.Poll();
    }
}
