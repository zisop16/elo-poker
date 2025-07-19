using Godot;
using System;

public partial class PokerClient : Node {
    WebSocketMultiplayerPeer Peer;
    void StartClient() {
        X509Certificate cert = ResourceLoader.Load<X509Certificate>(PokerServer.SERVER_CERT_PATH);
        TlsOptions opt = TlsOptions.ClientUnsafe(cert);
        Peer.CreateClient(PokerServer.SERVER_URL, opt);
    }

    public override void _Ready() {
        Peer = new();
        StartClient();
        GD.Print("Client Started");
    }


    public override void _Process(double delta) {
        Peer.Poll();
    }

}
