using ClosedXML.Excel;
using System.Data;

public class Vote
{
    int VotingId { get; }
    string QrId { get; }
    int Fav { get; }
    int DaOther1 { get; }
    int DaOther2 { get; }
    int DaOther3 { get; }
    int DaOther4 { get; }
    int DaOther5 { get; }

    public Vote(int votingId, string qrId, int fav, int o1, int o2, int o3, int o4, int o5)
    {
        VotingId = votingId;
        QrId = qrId;
        Fav = fav;
        DaOther1 = o1;
        DaOther2 = o2;
        DaOther3 = o3;
        DaOther4 = o4;
        DaOther5 = o5;
    }

    ///<summary>
    ///(string qrCode, Liste mit id von Diplomarbeiten (erster wert=top-fav)
    ///</summary>
    public static string SaveVote(string qr, List<int> titel)
    {
        try
        {
            int num = DbWrapper.Wrapper.RunNonQuery($"INSERT INTO foa_Voting VALUES (NULL, {qr}, {titel[0]}, {titel[1]}, {titel[2]}, {titel[3]},{titel[4]};)");
            if (num != 1) return $"Ein Fehler ist aufgetreten! Gespeicherte Votes: {num}";
            return "";
        }
        catch (Exception ex)
        {
            return $"{ex}";
        }
    }

    public static List<(DA, int)> ShowVotings(List<(DA, int count)> voting)
    {
        //Ergebnisse vom Voting speichern

        string sql = $"SELECT d.DaId, d.Titel, d.Schueler," +
        "COUNT(v.DaId) AS vote_count FROM foa_da AS d INNER JOIN foa_voting AS v ON d.DaId = v.DaId GROUP BY d.DaId, d.Titel, d.Schueler;";
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
