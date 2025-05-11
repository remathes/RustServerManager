using RustServerManager.Utils;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RustServerManager.Services
{
    public class RconClient
    {
        private readonly RustMySqlLogger _logger;
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private int _lastRequestId = 1;

        private string _ip;
        private ushort _port;
        private string _password;

        public bool IsConnected => _tcpClient?.Connected == true;

        public RconClient(string serverIdentity)
        {
            _logger = new RustMySqlLogger(serverIdentity);
        }

        public async Task DisconnectAsync()
        {
            try
            {
                if (_stream != null && _tcpClient?.Connected == true)
                {
                    int requestId = _lastRequestId++;
                    byte[] payload = Encoding.UTF8.GetBytes("\0");
                    byte[] packet = CreateRconPacket(requestId, 2, payload);
                    await _stream.WriteAsync(packet, 0, packet.Length);
                    await _stream.FlushAsync();
                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                await _logger.LogAsync("RCON", "Disconnect", "Warning", "RconClient", "Error during disconnect", ex.ToString());
            }

            try
            {
                _stream?.Close();
                _stream?.Dispose();
                _tcpClient?.Client?.Shutdown(SocketShutdown.Both);
                _tcpClient?.Close();
            }
            catch (Exception ex)
            {
                await _logger.LogAsync("RCON", "Disconnect", "Warning", "RconClient", "Error cleaning up stream", ex.ToString());
            }

            _stream = null;
            _tcpClient = null;
        }

        public async Task<bool> EnsureConnectedAsync(string ip, ushort port, string password)
        {
            if (IsConnected) return true;
            _ip = ip;
            _port = port;
            _password = password;
            return await ConnectAsync(_ip, _port, _password);
        }

        public async Task<bool> ConnectAsync(string ip, ushort port, string password)
        {
            try
            {
                await _logger.LogAsync("RCON", "Connect", "Info", "RconClient", $"Attempting connection to {ip}:{port}...");

                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(ip, port);
                await _logger.LogAsync("RCON", "Connect", "Info", "RconClient", "Socket connected.");

                _stream = _tcpClient.GetStream();

                int requestId = _lastRequestId++;
                byte[] payload = Encoding.UTF8.GetBytes(password + "\0");
                byte[] packet = CreateRconPacket(requestId, 3, payload); // 3 = SERVERDATA_AUTH
                await _stream.WriteAsync(packet, 0, packet.Length);
                await _logger.LogAsync("RCON", "Connect", "Debug", "RconClient", $"Auth packet sent with requestId={requestId}");

                bool receivedAuthResponse = false;

                for (int i = 0; i < 2; i++)
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead < 12)
                    {
                        await _logger.LogAsync("RCON", "Connect", "Warning", "RconClient", "Received packet too small to be valid.");
                        continue;
                    }

                    int respId = BitConverter.ToInt32(buffer, 4);
                    int type = BitConverter.ToInt32(buffer, 8);

                    await _logger.LogAsync("RCON", "Connect", "Debug", "RconClient", $"Received response: type={type}, respId={respId}, bytes={bytesRead}");

                    if (type == 2 && respId == requestId)
                    {
                        receivedAuthResponse = true;
                        break;
                    }
                    else if (type == 2 && respId == -1)
                    {
                        await _logger.LogAsync("RCON", "Connect", "Error", "RconClient", "Authentication failed: Incorrect password or IP not whitelisted.");
                        break;
                    }
                }

                if (receivedAuthResponse)
                {
                    await _logger.LogAsync("RCON", "Connect", "Info", "RconClient", $"Connected and authenticated to RCON at {ip}:{port}");
                }
                else
                {
                    await _logger.LogAsync("RCON", "Connect", "Warning", "RconClient", $"Authentication response not received or invalid for {ip}:{port}");
                }

                return receivedAuthResponse;
            }
            catch (Exception ex)
            {
                await _logger.LogAsync("RCON", "Connect", "Error", "RconClient", "Connection exception", ex.ToString());
                await Reset();
                return false;
            }
        }

        public async Task<string> SendCommandAsync(string command)
        {
            await _logger.LogAsync("RCON", "SendCommand", "Info", "RconClient", $"Sending command: {command}");
            await _logger.LogAsync("RCON", "SendCommand", "Warning", "RconClient", $"Stream or client disconnected. _stream null? {_stream == null}, connected? {_tcpClient?.Connected}");
            if (_stream == null || !_tcpClient?.Connected == true)
                return "[RCON] Not connected";

            try
            {
                int requestId = _lastRequestId++;
                byte[] payload = Encoding.UTF8.GetBytes(command + "\0");
                byte[] packet = CreateRconPacket(requestId, 2, payload);
                await _stream.WriteAsync(packet, 0, packet.Length);

                byte[] terminator = CreateRconPacket(requestId, 2, Encoding.UTF8.GetBytes("\0"));
                await _stream.WriteAsync(terminator, 0, terminator.Length);

                StringBuilder responseBuilder = new();
                byte[] buffer = new byte[4096];

                while (true)
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead <= 0) break;

                    int respId = BitConverter.ToInt32(buffer, 4);
                    int type = BitConverter.ToInt32(buffer, 8);
                    await _logger.LogAsync("RCON", "SendCommand", "Debug", "RconClient", $"Received response: type={type}, id={respId}, length={bytesRead}");
                    if (respId != requestId || type != 0) continue;

                    string chunk = Encoding.UTF8.GetString(buffer, 12, bytesRead - 12);
                    responseBuilder.Append(chunk);
                    if (bytesRead < buffer.Length) break;
                }

                var responseText = CleanResponse(responseBuilder.ToString());
                await _logger.LogAsync("RCON", "SendCommand", "Debug", "RconClient", $"Final response text: {responseText}");
                return responseText;
            }
            catch (Exception ex)
            {
                await _logger.LogAsync("RCON", "SendCommand", "Error", "RconClient", $"Failed sending command: {command}", ex.ToString());
                await Reset();
                return "[RCON] send failed";
            }
        }

        public void Disconnect()
        {
            _stream?.Dispose();
            _tcpClient?.Close();
            _stream = null;
            _tcpClient = null;
        }

        public async Task Reset()
        {
            await _logger.LogAsync("RCON", "Reset", "Info", "RconClient", "Resetting connection state.");
            Disconnect();
            _ip = null;
            _password = null;
            _port = 0;
            _lastRequestId = 1;
        }

        private byte[] CreateRconPacket(int id, int type, byte[] payload)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            int length = 4 + 4 + payload.Length;
            bw.Write(length);
            bw.Write(id);
            bw.Write(type);
            bw.Write(payload);
            return ms.ToArray();
        }

        private string CleanResponse(string raw)
        {
            return raw?.Replace("\0", "").Replace("\uFFFD", "").Trim();
        }
    }
}
