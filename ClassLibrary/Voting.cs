public class Vote
{
    string QrId { get; }
    int Fav { get; }
    int DaOther1 { get; }
    int DaOther2 { get; }
    int DaOther3 { get; }
    int DaOther4 { get; }
    int DaOther5 { get; }

    public Vote(string qrId, int fav, int o1, int o2, int o3, int o4, int o5)
    {
        QrId = qrId;
        Fav = fav;
        DaOther1 = o1;
        DaOther2 = o2;
        DaOther3 = o3;
        DaOther4 = o4;
        DaOther5 = o5;
    }

    /// <summary>
    /// Checks if a QR code exists in the database
    /// </summary>
    /// <param name="qr">The QR code to check</param>
    /// <returns>True if the QR code exists, false otherwise</returns>
    public static bool QrCodeExists(string qr)
    {
        try
        {
            object? qrExists = DbWrapper.Wrapper.RunQueryScalar($"SELECT COUNT(*) FROM FoA_QrCodes WHERE QrID = '{qr}'");
            return qrExists != null && Convert.ToInt32(qrExists) > 0;
        }
        catch
        {
            return false;
        }
    }

    ///<summary>
    ///(string qrCode, Liste mit id von Diplomarbeiten (erster wert=top-fav)
    ///</summary>
    public string SaveVote(string qr)
    {
        try
        {
            bool validQrCode = QrCodeExists(qr);
            if (!validQrCode)
            {
                return "Ungültiger User code";
            }

            int num = DbWrapper.Wrapper.RunNonQuery(@$"INSERT INTO foa_voting VALUES (NULL, '{qr}',
                {FormatId(Fav)},
                {FormatId(DaOther1)},
                {FormatId(DaOther2)},
                {FormatId(DaOther3)},
                {FormatId(DaOther4)},
                {FormatId(DaOther5)})");

            if (num != 1) return $"Ein Fehler ist aufgetreten! Gespeicherte Votes: {num}";
            return "";
        }
        catch (Exception ex)
        {
            return $"{ex}";
        }
    }

    private static string FormatId(int id)
    {
        return id == -1 ? "NULL" : id.ToString();
    }
}
