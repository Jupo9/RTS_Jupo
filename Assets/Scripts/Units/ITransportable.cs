using UnityEngine;

public interface ITransportable
{
    public Transform Transform { get; }
    public int TransportCapacityUsage { get; }

    public void LoadInto(ITransporter transporter);
}
