using ClosedXML.Excel;
using System.Data;

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

    /// <summary>
    /// Checks if a QR code has already been used (exists in the Vote table)
    /// </summary>
    /// <param name="qr">The QR code to check</param>
    /// <returns>True if the QR code has already been used, false otherwise</returns>
    public static bool HasQrCodeBeenUsed(string qr)
    {
        try
        {
            object? voteExists = DbWrapper.Wrapper.RunQueryScalar($"SELECT COUNT(*) FROM FoA_Voting WHERE QrId = '{qr}'");
            return voteExists != null && Convert.ToInt32(voteExists) > 0;
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
            if (!QrCodeExists(qr))
            {
                return "Ungültiger User code";
            }

            if (HasQrCodeBeenUsed(qr))
            {
                return "Dieser Code wurde bereits verwendet!";
            }

            int num = DbWrapper.Wrapper.RunNonQuery(@$"INSERT INTO FoA_Voting VALUES (NULL, '{qr}',
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

    public static List<(DA, int)> ShowVotings(List<(DA, int count)> voting)
    {
        //Ergebnisse vom Voting speichern

        string sql = @"SELECT d.DaId, d.Titel, d.Schueler,
       COALESCE(vc.cnt, 0) AS vote_count
FROM FoA_DA d
LEFT JOIN (
    SELECT DaId, COUNT(*) AS cnt FROM (
        SELECT Fav    AS DaId FROM FoA_Voting WHERE Fav IS NOT NULL
        UNION ALL SELECT DaOther1 AS DaId FROM FoA_Voting WHERE DaOther1 IS NOT NULL
        UNION ALL SELECT DaOther2 AS DaId FROM FoA_Voting WHERE DaOther2 IS NOT NULL
        UNION ALL SELECT DaOther3 AS DaId FROM FoA_Voting WHERE DaOther3 IS NOT NULL
        UNION ALL SELECT DaOther4 AS DaId FROM FoA_Voting WHERE DaOther4 IS NOT NULL
        UNION ALL SELECT DaOther5 AS DaId FROM FoA_Voting WHERE DaOther5 IS NOT NULL
    ) AS t
    GROUP BY DaId
) vc ON d.DaId = vc.DaId;";
        //ID | Titel | Schueler | Count
        DataTable da = DbWrapper.Wrapper.RunQuery(sql);
        DA result;
        //Schleife für jede DA in Db, pro durchgang wird
        foreach (DataRow row in da.Rows)
        {
            result = new DA((int)row[0], (string)row[1], (string)row[2]);
            int count = Convert.ToInt32(row[3]);
            voting.Add((result, count));
        }
        return voting; 
    }

    public static byte[] CreateExcelFile(List<(DA, int count)> voting)
    {
        XLWorkbook? workbook = new XLWorkbook();

        voting = voting
            .OrderByDescending(v => v.count)
            .ToList();

        // Referenz: https://www.youtube.com/watch?v=3DIKSjQMc5U
        IXLWorksheet worksheet = workbook.Worksheets.Add("FoA_Ergebnisse");
        worksheet.Cell("A1").Value = "Nr";
        worksheet.Cell("B1").Value = "Titel";
        worksheet.Cell("C1").Value = "Schüler";
        worksheet.Cell("D1").Value = "Count";

        int nr = 2;
        foreach ((DA, int) v in voting)
        {
            worksheet.Row(nr).Cell("A").Value = v.Item1.ID;
            worksheet.Row(nr).Cell("B").Value = v.Item1.Titel;
            worksheet.Row(nr).Cell("C").Value = v.Item1.Schueler;
            worksheet.Row(nr).Cell("D").Value = v.Item2;
            nr++;
        }

        IXLRange? range = worksheet.Range($"A1:D{voting.Count + 1}");
        IXLTable table = range.CreateTable();
        table.Theme = XLTableTheme.TableStyleLight10;

        worksheet.Cell($"A{voting.Count + 1}").Value = "Total Votes: ";
        worksheet.Cell($"D{voting.Count + 1}").FormulaA1 = $"SUM(D2:D{voting.Count + 1})";

        worksheet.Row(voting.Count + 1).Style.Font.Bold = true;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
