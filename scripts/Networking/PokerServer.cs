using Godot;
using System;
using System.Text;

public partial class PokerServer : Node {
    WebSocketMultiplayerPeer Peer;
    public const int PORT = 42069;
    public const string IP_ADDRESS = "localhost";
    public static string SERVER_URL = "wss://" + IP_ADDRESS + ":" + PORT;
    public const string SERVER_CERT_PATH = "res://resources/server_CAS.crt";
    public const string SERVER_KEY_PATH = "res://resources/server_key.key";

    

    void StartServer() {
        bool certExists = ResourceLoader.Exists(SERVER_CERT_PATH);
        bool keyExists = ResourceLoader.Exists(SERVER_KEY_PATH);
        if (!certExists || !keyExists) {
            GenerateCertificate();
        }
        X509Certificate cert = ResourceLoader.Load<X509Certificate>(SERVER_CERT_PATH);
        CryptoKey key = ResourceLoader.Load<CryptoKey>(SERVER_KEY_PATH);
        TlsOptions opt = TlsOptions.Server(key, cert);
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

    public override void _Ready() {
        Peer = new();
        StartServer();
        GD.Print("Server Started");
    }


    public override void _Process(double delta) {
        Peer.Poll();
        if (Peer.GetAvailablePacketCount() == 0) return;
        byte[] packet = Peer.GetPacket();
        string msg = Encoding.UTF8.GetString(packet);
        GD.Print("Server received: ", msg);
    }

}
