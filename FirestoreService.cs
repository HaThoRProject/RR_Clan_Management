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
            string credentialPath = @"E:\Learn\Visual Studio\RR_Clan_Management\rr-clan-management.json";

            if (!File.Exists(credentialPath))
            {
                throw new FileNotFoundException("A Firestore hitelesítési fájl nem található!", credentialPath);
            }

            // 🔹 Állítsuk be a GOOGLE_APPLICATION_CREDENTIALS környezeti változót!
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialPath);

            // 🔹 Inicializáljuk a Firestore kapcsolatot a beállított hitelesítéssel
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
