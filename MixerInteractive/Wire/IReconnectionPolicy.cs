namespace MixerInteractive.Wire
{
    public interface IReconnectionPolicy
    {
        long BaseDelay { get; set; }
        long MaxDelay { get; set; }

        long Next();
        void Reset();
    }
}