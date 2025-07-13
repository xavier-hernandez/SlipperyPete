using System.Net.Sockets;
using System.Text;
using Renci.SshNet;
using Renci.SshNet.Common;

class Program
{
    static void Main(string[] args)
    {
        string userName = "dummyUser";

        if (args.Length != 1)
        {
            Console.WriteLine("Usage: SshBatchChecker <targets.txt>");
            Console.WriteLine("  targets.txt should contain lines like 192.168.1.100:22");
            return;
        }

        var filePath = args[0];
        if (!File.Exists(filePath))
        {
            Console.Error.WriteLine($"File not found: {filePath}");
            return;
        }

        foreach (var raw in File.ReadAllLines(filePath))
        {
            var line = raw.Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            // split into at most 2 parts
            var parts = line.Split(new[] { ':' }, 2);
            string host;
            int port;

            if (parts.Length == 1)
            {
                // no port specified → use default 22
                host = parts[0];
                port = 22;
            }
            else if (!int.TryParse(parts[1], out port))
            {
                Console.WriteLine($"[INVALID] “{line}” – port is not a number");
                continue;
            }
            else
            {
                host = parts[0];
            }

            if (TryGetSshAuthMethods(host, port, userName, out var methods))
            {
                Console.WriteLine($"[{host}:{port}] SSH detected");

                // Check password auth
                bool allowsPassword = methods.Contains("password");
                Console.WriteLine($"Password authentication:\t{(allowsPassword ? "Allowed" : "Not allowed")}");

                // Check publickey auth
                bool allowsPubKey = methods.Contains("publickey");
                Console.WriteLine($"Public-key authentication:\t{(allowsPubKey ? "Allowed" : "Not allowed")}");

                // (You can add more checks below, e.g. keyboard-interactive, gssapi-…)
                bool allowsKeyboard = methods.Contains("keyboard-interactive");
                Console.WriteLine($"Keyboard-interactive auth:\t{(allowsKeyboard ? "Allowed" : "Not allowed")}");

                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Not an SSH port or unable to detect auth methods.");
            }
        }
    }

    public static bool TryGetSshAuthMethods(string host, int port, string userName, out string[] supportedMethods)
    {
        supportedMethods = Array.Empty<string>();
        try
        {
            using var tcp = new TcpClient();
            tcp.Connect(host, port);
            using var netStream = tcp.GetStream();
            using var reader = new StreamReader(netStream, Encoding.ASCII, false, 256, leaveOpen: true);

            // The SSH server should immediately send a line like "SSH-2.0-OpenSSH_8.9"
            var banner = reader.ReadLine();
            if (banner == null || !banner.StartsWith("SSH-"))
                return false;  // Not SSH :contentReference[oaicite:0]{index=0}
        }
        catch (SocketException)
        {
            return false;      // Port closed or not reachable
        }

        var noneAuth = new NoneAuthenticationMethod(userName);
        var connectionInfo = new ConnectionInfo(host, port, userName, noneAuth);

        using var ssh = new SshClient(connectionInfo);
        try
        {
            ssh.Connect();  // This will always throw SshAuthenticationException for "none"
        }
        catch (SshAuthenticationException ex)
        {
            // SSH-2 servers advertise allowed methods by failing the "none" request :contentReference[oaicite:1]{index=1}
            var msg = ex.Message;
            var start = msg.IndexOf('(');
            var end = msg.IndexOf(')');
            if (start >= 0 && end > start)
            {
                var list = msg.Substring(start + 1, end - start - 1);
                supportedMethods = list.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.Trim()).ToArray();
                return true;
            }
        }

        return false;
    }
}
