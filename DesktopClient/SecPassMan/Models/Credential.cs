using SQLite;
using System;
using System.Collections.Generic;

namespace SecPassMan.Models;

public class Credential
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string MasterPassword { get; set; }
    public bool Keep { get; set; }

}

public class SiteCredential
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }


    public string SiteUrl { get; set; }
    public string SiteUsername { get; set; }
    public string SitePassword { get; set; }

    public int CredentialId { get; set; }
}

public class SiteCredentialCollection
{
    public string type="SiteCredentialCollection";
    public List<SiteCredential> SiteCredentials { get; set; }
}

public class AcceptedSession
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string IPstring { get; set; }
    public DateTime InitTime{get; set; }

}