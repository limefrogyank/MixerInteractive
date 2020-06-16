namespace MixerInteractive.State
{
    public interface IControl : IControlData
    {
        void OnUpdate(IControlData controlData);
    }
}