using Google.Cloud.Firestore;
using System.Linq;
using System.Threading.Tasks;
using BCrypt;


public class FirebaseAuthService
{
    private readonly FirestoreDb _db;

    public FirebaseAuthService()
    {
        _db = FirestoreDb.Create("rr-clan-management");
    }

    public async Task<bool> AuthenticateUserAsync(string name, string password)
    {
        var usersRef = _db.Collection("Admins");
        var query = usersRef.WhereEqualTo("Name", name);
        var snapshot = await query.GetSnapshotAsync();

        if (!snapshot.Documents.Any())
            return false; // Nincs ilyen user

        var userDoc = snapshot.Documents.First();
        var storedHash = userDoc.GetValue<string>("PasswordHash");

        return BCrypt.Net.BCrypt.Verify(password, storedHash);
    }
}
