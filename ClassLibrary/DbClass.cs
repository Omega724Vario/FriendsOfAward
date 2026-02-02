using Google.Protobuf.Collections;
using System.Data;

public class FoA_DA
{
    public int ID { get; set; } //in als NULL übergeben, Auto_Increment via sql
    public string Titel { get; set; }
    public string Schueler { get; set; }

    public FoA_DA(int id, string titel, string schueler)
    {
        ID = id;
        Titel = titel;
        Schueler = schueler;
    }
    public static List<(int, string)> GetDAs()
    {
        // id und Titel übergabe
        List<(int, string)> result = new List<(int, string)> ();

        try
        {
            DataTable dt = DbWrapper.Wrapper.RunQuery($"SELECT DaId, Titel FROM foA_dA;");
            foreach (DataRow row in dt.Rows)
            {
                int id = Convert.ToInt32(row[0]);
                string titel = Convert.ToString(row[1]);
                result.Add((id, titel)); 
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetDAs: {ex.Message}");
        }
        return result;
    }
    public static bool SearchAdmin(string username)
    {
        string sql = $"SELECT Benutzername FROM FoA_Admin WHERE Benutzername = '{username}'";
        DataTable dt = null; 
        try
        {
            dt = DbWrapper.Wrapper.RunQuery(sql);
            if (dt == null) return false;
            else return true; 
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false; 
        }
    }
    public static bool SaveDAtoDb(List<FoA_DA> foaDA)
    {
        try
        {
            CreateClassesSQL();
            DbWrapper.Wrapper.RunNonQuery($"SET FOREIGN_KEY_CHECKS = 0;" +
                $"TRUNCATE TABLE foa_da;" +
                $"TRUNCATE TABLE foa_qrcodes;" +
                $"TRUNCATE TABLE foa_voting_system;" +
                $"SET FOREIGN_KEY_CHECKS = 1;");
            foreach (FoA_DA da in foaDA)
            {
                string sql = $"INSERT INTO FoA_DA(Titel, Schueler) VALUES('{da.Titel}', '{da.Schueler}')";
                DbWrapper.Wrapper.RunNonQuery(sql);
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SaveDAtoDb: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    public static bool CreateClassesSQL()
    {
        try
        {
            DbWrapper.Wrapper.RunNonQuery(
                "CREATE TABLE IF NOT EXISTS FoA_DA (" +
                "DaId INT NOT NULL AUTO_INCREMENT, " +
                "Titel VARCHAR(100) NOT NULL, " +
                "Schueler VARCHAR(200) NOT NULL, " +
                "PRIMARY KEY (DaId))");

            DbWrapper.Wrapper.RunNonQuery(
                "CREATE TABLE IF NOT EXISTS FoA_Admin (" +
                "BenutzerId INT AUTO_INCREMENT," +
                "Benutzername VARCHAR(50) NOT NULL, UNIQUE" +
                "Passwort VARCHAR(255) " +
                "PRIMARY KEY(BenutzerId))");

            DbWrapper.Wrapper.RunNonQuery(
                "CREATE TABLE IF NOT EXISTS FoA_QrCodes (" +
                "QrID VARCHAR(8) NOT NULL, " +
                "PRIMARY KEY(QrID))");

            DbWrapper.Wrapper.RunNonQuery("CREATE TABLE if NOT EXISTS `FoA_Voting_System`" +
                "(`VotingId`INT NOT NULL AUTO_INCREMENT,`QrId` VARCHAR(8) NOT NULL," +
                "`Fav` INT NOT NULL,`DaOther1` INT NOT NULL," +
                "`DaOther2` INT NOT NULL,`DaOther3` INT NOT NULL," +
                "`DaOther4` INT NOT NULL,`DaOther5` INT NOT NULL," +
                "PRIMARY KEY(`VotingId`), FOREIGN KEY(`QrId`) " +
                "REFERENCES FoA_QrCodes(`QrId`) ON DELETE CASCADE," +
                "FOREIGN KEY(`Fav`) REFERENCES FoA_DA(`DaId`) ON DELETE CASCADE," +
                "FOREIGN KEY(`DaOther1`) REFERENCES FoA_DA(`DaId`) ON DELETE CASCADE," +
                "FOREIGN KEY(`DaOther2`) REFERENCES FoA_DA(`DaId`) ON DELETE CASCADE," +
                "FOREIGN KEY(`DaOther3`) REFERENCES FoA_DA(`DaId`) ON DELETE CASCADE," +
                "FOREIGN KEY(`DaOther4`) REFERENCES FoA_DA(`DaId`) ON DELETE CASCADE," +
                "FOREIGN KEY(`DaOther5`) REFERENCES FoA_DA(`DaId`) ON DELETE CASCADE);");

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CreateClassesSQL: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    public static List<string> UnUsedQrCodes()
    {
        List<string> qrIds = new List<string>();

        DataTable table = DbWrapper.Wrapper.RunQuery($"SELECT QrId FROM FoA_QrCodes " +
            $"WHERE QrId NOT IN (SELECT QrId FROM FoA_Voting_System)");
        foreach (DataRow row in table.Rows)
        {
            qrIds.Add(row["QrId"].ToString());
        }
        return qrIds;
    }

    public static (bool, string) ResetDb()
    {
        string fehler = "";
        try
        {
            DbWrapper.Wrapper.RunNonQuery("SET FOREIGN_KEY_CHECKS = 0;" +
            "TRUNCATE TABLE foa_da;TRUNCATE TABLE foa_qrcodes;" +
            "TRUNCATE TABLE foa_voting_system;" +
            "SET FOREIGN_KEY_CHECKS = 1;");
            return (true, fehler);
        }
        catch (Exception ex)
        {
            fehler = ex.ToString();
            return (false, fehler);
        }
    }
    public static bool SaveQRtoDb(List<string> qrCodes)
    {
        try
        {
            FoA_DA.CreateClassesSQL();
            foreach (string qrCode in qrCodes)
            {
                DbWrapper.Wrapper.RunNonQuery($"INSERT INTO FoA_QrCodes (QrID) VALUES ('{qrCode}')");
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }
}
public class FoA_Voting_System
{
    int VotingId { get; }
    string QrId { get; }
    int Fav { get; }
    int DaOther1 { get; }
    int DaOther2 { get; }
    int DaOther3 { get; }
    int DaOther4 { get; }
    int DaOther5 { get; }

    public FoA_Voting_System(int votingId, string qrId, int fav, int o1, int o2, int o3, int o4, int o5)
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
            int num = DbWrapper.Wrapper.RunNonQuery($"INSERT INTO foa_voting_system VALUES (NULL, {qr}, {titel[0]}, {titel[1]}, {titel[2]}, {titel[3]},{titel[4]};)");
            if (num != 1) return $"Ein Fehler ist aufgetreten! Gespeicherte Votes: {num}";
            return "";
        }
        catch (Exception ex)
        {
            return $"{ex}";
        }
    }
}
