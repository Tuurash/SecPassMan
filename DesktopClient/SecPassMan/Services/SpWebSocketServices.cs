using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace SecPassMan.Services;

public class SpWebSocketServices
{
}


public class SPWebSocketServerHost : WebSocketBehavior
{
    protected override void OnMessage(MessageEventArgs e)
    {
        var message = e.Data;
        Console.WriteLine("Received message: " + message);
    }
}

public class SpWebSocketServerClient
{
    private WebSocketServer _server;
    private Dictionary<string, SPWebSocketServerHost> _clients = new Dictionary<string, SPWebSocketServerHost>();
    public SpWebSocketServerClient(string url)
    {
        _server = new WebSocketServer(url);
        _server.AddWebSocketService<SPWebSocketServerHost>("/");
        _server.Start();
    }

    public void Stop() => _server.Stop();
    public void Start() => _server.Start();

    //Send Message 
    public void SendMessage(string message)
    {
        //Send Message to all clients
        try
        {
            foreach (var session in _server.WebSocketServices.Hosts.ToList()[0].Sessions.Sessions)
                session.Context.WebSocket.Send(message);
        }catch(Exception exc)
        {
            Console.WriteLine(exc.Message);
        }         
    }

}