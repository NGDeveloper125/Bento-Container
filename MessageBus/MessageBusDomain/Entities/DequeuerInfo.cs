using System.Net;

namespace MessageBusDomain.Entities;

public class DequeuerInfo
{
    public Address Address { get; init; }
    public Port Port { get; init; }

    public DequeuerInfo(string address, string port)
    {
        if (string.IsNullOrEmpty(address)) throw new ArgumentNullException(nameof(address));
        if (string.IsNullOrEmpty(port)) throw new ArgumentNullException(nameof(port));

        if (!IPAddress.TryParse(address, out IPAddress? iPAddress) && address != "localhost") throw new ArgumentException("Invalid IP address", nameof(address));
        if (!int.TryParse(port, out var portNumber) || int.Parse(port) < 1 || int.Parse(port) > 65535) throw new ArgumentException("Invalid port number", nameof(port));
        string fullAddress = $@"tcp://{address}";
        Address = new Address(fullAddress);
        Port = new Port(portNumber);
    }
}