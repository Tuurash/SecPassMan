using SecPassMan.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SecPassMan.Services;

public static class GObservables
{

    public static string WebSocketUri = @"http://localhost:8080/";
    public static string encryptionKey = @"asfncvjknzxlcuvlhaiur12vmovm45u6u40p5tgj9gj";
    public static string GlobalMasterPass { get; set; }
    public static IEnumerable<SiteCredential> GlobalAccessSiteCreds { get; set; }

    public static string dbPath = "SpEncryptedDatabase.db";
    public static string RandomString(int length)
    {
        Random random = new Random();

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static bool IsValidEmailAddress(string s)
    {
        Regex regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
        return regex.IsMatch(s);
    }

}
