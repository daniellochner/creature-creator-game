using DanielLochner.Assets;

public class CustomIdOption : OptionSelector.Option
{
    public override string Name => MapName;

    private int Index => Id.IndexOf("#");

    public string MapId => Id.Substring(0, Index);
    public string MapName => Id.Substring(Index + 1);
}