namespace PhoneCompare.Config;

public static class FirebaseConfig
{
    public const string ApiKey = "AIzaSyBodBeRZNRS0pz4mvqputYEGh39hxzizXI";
    public const string ProjectId = "phonecompare-c404d";
    public const string AuthBaseUrl = "https://identitytoolkit.googleapis.com/v1";
    public const string FirestoreBaseUrl = "https://firestore.googleapis.com/v1";

    public static readonly HashSet<string> AdminEmails = new(StringComparer.OrdinalIgnoreCase)
    {
        "selgasjenel@gmail.com"
    };

    public static bool IsAdmin(string? email) => 
        !string.IsNullOrEmpty(email) && AdminEmails.Contains(email);
}
