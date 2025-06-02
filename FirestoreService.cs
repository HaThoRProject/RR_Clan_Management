using Google.Cloud.Firestore;
using RR_Clan_Management.Models;
using System;
using System.IO;

namespace RR_Clan_Management.Services
{
    public class FirestoreService
    {
        private readonly FirestoreDb _firestoreDb;

        public FirestoreService()
{
    // 🔹 Próbáljuk lekérni a környezeti változóból a hitelesítési fájl elérési útját
    string? credentialPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");

    // 🔸 Ha nincs beállítva, akkor fejlesztői gépen állítsuk be manuálisan
    if (string.IsNullOrEmpty(credentialPath))
    {
        credentialPath = Path.Combine(AppContext.BaseDirectory, "rr-clan-management.json");

        if (!File.Exists(credentialPath))
        {
            throw new FileNotFoundException("A Firestore hitelesítési fájl nem található!", credentialPath);
        }

        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialPath);
    }

    // 🔹 Inicializáljuk a Firestore kapcsolatot
    _firestoreDb = FirestoreDb.Create("rr-clan-management");
}


        public FirestoreDb GetDatabase()
        {
            return _firestoreDb;
        }

        public async Task AddPlayerAsync(Player player)
        {
            var docRef = _firestoreDb.Collection("players").Document();
            player.Id = docRef.Id;  // Generált Firestore dokumentum ID beállítása
            await docRef.SetAsync(player);
        }
        public async Task<Player> GetPlayerByIdAsync(string id)
        {
            var docRef = _firestoreDb.Collection("players").Document(id);
            var snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                return snapshot.ConvertTo<Player>();
            }

            return null;
        }

        public async Task UpdatePlayerAsync(Player player)
        {
            var docRef = _firestoreDb.Collection("players").Document(player.Id);
            await docRef.SetAsync(player, SetOptions.MergeAll);
        }



    }
}
