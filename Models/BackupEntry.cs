namespace ProjectManagerApp.Models;

public class BackupEntry
{
    public string FileName { get; set; } = "";
    public long SizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }

    public string SizeDisplay => SizeBytes switch
    {
        < 1024 => $"{SizeBytes} B",
        < 1048576 => $"{SizeBytes / 1024.0:F1} KB",
        _ => $"{SizeBytes / 1048576.0:F1} MB"
    };
}
