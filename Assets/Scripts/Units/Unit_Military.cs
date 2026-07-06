public class Unit_Military : AbstractUnit, ITransportable
{
    public int TransportCapacityUsage => unitSO.TransportConfig.GetTransportCapacityUsage();

    public void LoadInto(ITransporter transporter)
    {
        MoveTo(transporter.Transform);
        transporter.Load(this);
    }
}