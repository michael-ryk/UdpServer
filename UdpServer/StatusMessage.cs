public class StatusMessage
{
    public string Status { get; set; } = "Unknown";
    public DateTime Timestamp { get; set; }
}

public class CommandMessage
{
    public string Command { get; set; } = "";
    public string Payload { get; set; } = "";
}