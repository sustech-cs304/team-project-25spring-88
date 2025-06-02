using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[AddComponentMenu("Network/Authenticators/Token Authenticator")]
public class TokenAuthenticator : NetworkAuthenticator
{
    [Header("Server Configuration")]
    [Tooltip("Server expects this token from clients")]
    public string serverToken;

    [Header("Client Configuration")]
    [Tooltip("Token sent to server during authentication")]
    public string clientToken;

    readonly HashSet<NetworkConnectionToClient> connectionsPendingDisconnect = new HashSet<NetworkConnectionToClient>();

    #region Messages
    public struct AuthRequestMessage : NetworkMessage
    {
        public string authToken; // 客户端发送的Token
    }

    public struct AuthResponseMessage : NetworkMessage
    {
        public byte code;
        public string message;
    }
    #endregion

    #region Server
    public override void OnStartServer()
    {
        NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, false);
    }

    public override void OnStopServer()
    {
        NetworkServer.UnregisterHandler<AuthRequestMessage>();
    }

    public override void OnServerAuthenticate(NetworkConnectionToClient conn)
    {
        // 等待客户端发送AuthRequestMessage
    }

    private void OnAuthRequestMessage(NetworkConnectionToClient conn, AuthRequestMessage msg)
    {
        if (connectionsPendingDisconnect.Contains(conn)) return;

        if (msg.authToken == serverToken)
        {
            // 认证成功
            conn.Send(new AuthResponseMessage
            {
                code = 100,
                message = "Authentication Success"
            });

            ServerAccept(conn);
        }
        else
        {
            connectionsPendingDisconnect.Add(conn);

            conn.Send(new AuthResponseMessage
            {
                code = 200,
                message = "Invalid Token"
            });

            conn.isAuthenticated = false;
            StartCoroutine(DelayedDisconnect(conn, 1f));
        }
    }

    IEnumerator DelayedDisconnect(NetworkConnectionToClient conn, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        ServerReject(conn);
        connectionsPendingDisconnect.Remove(conn);
    }
    #endregion

    #region Client
    public override void OnStartClient()
    {
        NetworkClient.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage, false);
    }

    public override void OnStopClient()
    {
        NetworkClient.UnregisterHandler<AuthResponseMessage>();
    }

    public override void OnClientAuthenticate()
    {
        // 发送Token到服务器
        NetworkClient.Send(new AuthRequestMessage
        {
            authToken = clientToken
        });
    }

    private void OnAuthResponseMessage(AuthResponseMessage msg)
    {
        if (msg.code == 100)
        {
            ClientAccept();
        }
        else
        {
            Debug.LogError($"Authentication Failed: {msg.message}");
            ClientReject();
        }
    }
    #endregion
}