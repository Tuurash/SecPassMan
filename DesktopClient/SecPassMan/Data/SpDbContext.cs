using Microsoft.EntityFrameworkCore;
using SecPassMan.Models;
using SecPassMan.Services;
using SQLite;
using SQLitePCL;
using System.Collections.Generic;
using System.IO;


namespace SecPassMan.Data;

public class SpDbContext : DbContext
{
    readonly SQLiteConnection database;
    

    public SpDbContext()
    {
        SQLitePCL.Batteries.Init();

        var provider = new SQLite3Provider_dynamic_cdecl();
        raw.SetProvider(provider);



        if (!File.Exists(GObservables.dbPath))
        {
            var connectionString = new SQLiteConnectionString(GObservables.dbPath, true, GObservables.encryptionKey);
            database = new SQLiteConnection(connectionString);

            database.CreateTable<Credential>();
            database.CreateTable<SiteCredential>();
        }
        else
        {
            // If it does exist, open the existing database
            var connectionString = new SQLiteConnectionString(GObservables.dbPath, true, GObservables.encryptionKey);
            database = new SQLiteConnection(connectionString);
        }




        database.CreateTable<Credential>();
        database.CreateTable<SiteCredential>();
    }

    //Master Credentials
    public List<Credential> GetMasterCredentials() => database.Table<Credential>().ToList();

    public void SaveMasterCredential(Credential credential) => database.Insert(credential);

    public void UpdateMasterCredential(Credential credential) => database.Update(credential);

    public void DeleteMasterCredential(Credential credential) => database.Delete(credential);

    //Site Credentials
    public List<SiteCredential> GetSiteCredentials(Credential credential) => database.Table<SiteCredential>().Where(s=>s.CredentialId==credential.Id).ToList();

    public void SaveSiteCredential(SiteCredential credential) => database.Insert(credential);

    public void UpdateSiteCredential(SiteCredential credential) => database.Update(credential);

    public void DeleteSiteCredential(SiteCredential credential) => database.Delete(credential);
}
