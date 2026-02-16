public class MusicTrack
{
    public string Title { get; set; } = "";
    public string FilePath { get; set; } = "";
    public string Artist { get; set; } = "";
    public string Album { get; set; } = "";
    public string? ArtworkUrl { get; set; }

    public bool MetadataLoaded { get; set; }

    public List<string> Images { get; set; } = new List<string>();

    public override string ToString()
    {
        return Title;
    }
}
