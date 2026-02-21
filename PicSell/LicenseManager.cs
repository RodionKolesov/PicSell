using System;

[Serializable]
public class LicenseData
{
    public string UserName { get; set; }
    public string UserProfile { get; set; }
    public bool disableStat { get; set; }
    public bool disablePlugin { get; set; }
    public bool disableDraw { get; set; }
    public DateTime ExpirationDate { get; set; }
}

public class LicenseManager
{
    public static bool IsFullVersion => true;
    public static bool disableDraw => false;
    public static bool disableStat => false;
    public static bool disablePlugin => false;
    public static DateTime ExpirationDate => DateTime.MaxValue;
    public static string CurrentUserName => "Full User";
    public static string CurrentUserProfile => "Full";

    public static void Initialize()
    {
        // License check removed — always full version
    }

    public static void CheckForLicense()
    {
        // License check removed — always full version
    }
}
