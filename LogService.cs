using Google.Cloud.Firestore;
using System;
using System.Threading.Tasks;

public class LogService
{
    private readonly FirestoreDb _db;

    public LogService()
    {
        _db = FirestoreDb.Create("rr-clan-management");
    }

    public async Task LogEventAsync(string name, string type, string message)
    {
        var log = new LogEntry
        {
            Name = name,
            Type = type,
            Message = message,
            Timestamp = Timestamp.GetCurrentTimestamp()
        };

        string dateId = DateTime.UtcNow.ToString("yyyy.MM.dd");

        string CleanForFirestore(string input) =>
            string.IsNullOrWhiteSpace(input) ? "unknown" :
            input.Replace("/", "_")
                 .Replace(".", "_")
                 .Replace("#", "_")
                 .Replace("[", "_")
                 .Replace("]", "_");

        string safeName = CleanForFirestore(name);
        string safeType = CleanForFirestore(type);
        string logId = $"{DateTime.UtcNow:HHmmss}_{safeName}_{safeType}";

        CollectionReference dailyLogs = _db.Collection(dateId + "_Logs");
        await dailyLogs.Document(logId).SetAsync(log);
    }

}
