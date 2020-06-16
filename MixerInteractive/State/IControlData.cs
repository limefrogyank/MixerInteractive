namespace MixerInteractive.State
{
    public interface IControlData
    {
        string ControlID { get; set; }
        bool Disabled { get; set; }
        string Kind { get; set; }
        Meta Meta { get; set; }
        GridPlacement Position { get; set; }
    }
}