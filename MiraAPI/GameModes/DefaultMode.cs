namespace MiraAPI.GameModes;

[RegisterGameMode]
public class DefaultMode : CustomGameMode
{
    public override string Name => "Default";
    public override string Description => "Default Among Us GameMode";
    public override int Id => 0;
}