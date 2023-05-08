namespace TrafficSystem
{
    public interface IVehicle
    {
        SplinePath Path { get; }
        Anchor CurrentAnchor { get; }

        int CurrentSpeedLimit { get; set; }
        float MetresPerSecond { get; set; }
    }
}
