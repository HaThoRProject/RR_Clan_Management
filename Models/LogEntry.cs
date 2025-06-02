using Google.Cloud.Firestore;

[FirestoreData]
public class LogEntry
{
    [FirestoreProperty]
    public string Name { get; set; }

    [FirestoreProperty]
    public string Type { get; set; }  // Pl.: Login, Update, Delete, Error

    [FirestoreProperty]
    public string Message { get; set; }

    [FirestoreProperty]
    public Timestamp Timestamp { get; set; }

}
