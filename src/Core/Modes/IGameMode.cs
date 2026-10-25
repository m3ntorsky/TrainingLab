namespace TrainingLab.Core.Modes;

public interface IGameMode
{
    public GameModeDescriptor Descriptor { get; }

    public void Start();

    public void Stop();
}