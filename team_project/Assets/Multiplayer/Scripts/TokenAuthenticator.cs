using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// A Unity script that implements token-based authentication for a multiplayer racing game using Mirror.
/// <para>
/// This script authenticates clients by comparing their token with the server's token, handling authentication
/// messages and managing connection acceptance or rejection.
/// </para>
/// </summary>
[AddComponentMenu("Network/Authenticators/Token Authenticator")]
public class TokenAuthenticator : NetworkAuthenticator
{
    /// <summary>
    /// The token expected from clients for server authentication.
    /// </summary>
    [Header("Server Configuration")]
    [Tooltip("Server expects this token from clients")]
    public string serverToken;

    /// <summary>
    /// The token sent by the client during authentication.
    /// </summary>
    [Header("Client Configuration")]
    [Tooltip("Token sent to server during authentication")]
    public string clientToken;

    /// <summary>
    /// A set of client connections pending disconnection due to failed authentication.
    /// </summary>
    readonly HashSet<NetworkConnectionToClient> connectionsPendingDisconnect = new HashSet<NetworkConnectionToClient>();

    #region Messages
    /// <summary>
    /// A network message struct sent by clients to request authentication.
    /// </summary>
    public struct AuthRequestMessage : NetworkMessage
    {
        /// <summary>
        /// The authentication token sent by the client.
        /// </summary>
        public string authToken; // 客户端发送的Token
    }

    /// <summary>
    /// A network message struct sent by the server in response to an authentication request.
    /// </summary>
    public struct AuthResponseMessage : NetworkMessage
    {
        /// <summary>
        /// The response code (100 for success, 200 for failure).
        /// </summary>
        public byte code;

        /// <summary>
        /// The response message describing the authentication result.
        /// </summary>
        public string message;
    }
    #endregion

    #region Server
    /// <summary>
    /// Registers the authentication request handler when the server starts.
    /// </summary>
    public override void OnStartServer()
    {
        NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, false);
    }

    /// <summary>
    /// Unregisters the authentication request handler when the server stops.
    /// </summary>
    public override void OnStopServer()
    {
        NetworkServer.UnregisterHandler<AuthRequestMessage>();
    }

    /// <summary>
    /// Initiates server-side authentication, waiting for client authentication messages.
    /// </summary>
    /// <param name="conn">The client connection to authenticate.</param>
    public override void OnServerAuthenticate(NetworkConnectionToClient conn)
    {
        // 等待客户端发送AuthRequestMessage
    }

    /// <summary>
    /// Processes an authentication request from a client.
    /// </summary>
    /// <param name="conn">The client connection sending the request.</param>
    /// <param name="msg">The authentication request message containing the client's token.</param>
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

    /// <summary>
    /// Coroutine that delays disconnection of a client after failed authentication.
    /// </summary>
    /// <param name="conn">The client connection to disconnect.</param>
    /// <param name="delay">The delay in seconds before disconnection.</param>
    IEnumerator DelayedDisconnect(NetworkConnectionToClient conn, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        ServerReject(conn);
        connectionsPendingDisconnect.Remove(conn);
    }
    #endregion

    #region Client
    /// <summary>
    /// Registers the authentication response handler when the client starts.
    /// </summary>
    public override void OnStartClient()
    {
        NetworkClient.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage, false);
    }

    /// <summary>
    /// Unregisters the authentication response handler when the client stops.
    /// </summary>
    public override void OnStopClient()
    {
        NetworkClient.UnregisterHandler<AuthResponseMessage>();
    }

    /// <summary>
    /// Initiates client-side authentication by sending the client token to the server.
    /// </summary>
    public override void OnClientAuthenticate()
    {
        // 发送Token到服务器
        NetworkClient.Send(new AuthRequestMessage
        {
            authToken = clientToken
        });
    }

    /// <summary>
    /// Processes the server's authentication response.
    /// </summary>
    /// <param name="msg">The authentication response message from the server.</param>
    private void OnAuthResponseMessage(AuthResponseMessage msg)
    {
        if (msg.code == 100)
        {
            ClientAccept();
        }
        else
        {
            Debug.LogError($"[{nameof(TokenAuthenticator)}] Authentication Failed: {msg.message}");
            ClientReject();
        }
    }
    #endregion
}