using Google.Protobuf.Collections;
using System.Data;

public class DA
{
    public int ID { get; set; } //in als NULL übergeben, Auto_Increment via sql
    public string Titel { get; set; }
    public string Schueler { get; set; }

    public DA(int id, string titel, string schueler)
    {
        ID = id;
        Titel = titel;
        Schueler = schueler;
    }
    public static List<(int, string)> GetDAs()
    {
        // id und Titel übergabe
        List<(int, string)> result = new List<(int, string)> ();
        string sql = $"SELECT DaId, Titel FROM DA;";
        try
        {
            DataTable dt = DbWrapper.Wrapper.RunQuery(sql);
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
    public static bool IsAdmin(string username)
    {
        string sql = $"SELECT Benutzername FROM FoA_Admin WHERE Benutzername = '{username}'";
        DataTable dt = new DataTable(); 
        try
        {
            dt = DbWrapper.Wrapper.RunQuery(sql);
            if (dt.Rows.Count <= 0) return false;
            else return true; 
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false; 
        }
    }

    public static bool IsValidUser(string username, string password)
    {
        string sql = $"SELECT Benutzername FROM FoA_Admin WHERE Benutzername = '{username}' AND Passwort = '{password}'";
        DataTable dt = new DataTable();
        try
        {
            dt = DbWrapper.Wrapper.RunQuery(sql);
            if (dt.Rows.Count <= 0) return false;
            else return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }

    public static bool SaveDAtoDb(List<DA> foaDA)
    {
        try
        {
            CreateClassesSQL();
            foreach (DA da in foaDA)
            {
                string sql = $"INSERT INTO DA(Titel, Schueler) VALUES('{da.Titel}', '{da.Schueler}')";
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
                "CREATE TABLE IF NOT EXISTS DA (" +
                "DaId INT NOT NULL AUTO_INCREMENT, " +
                "Titel VARCHAR(100) NOT NULL, " +
                "Schueler VARCHAR(200) NOT NULL, " +
                "PRIMARY KEY (DaId))");

            DbWrapper.Wrapper.RunNonQuery(
                "CREATE TABLE IF NOT EXISTS FoA_Admin (" +
                "BenutzerId INT AUTO_INCREMENT," +
"Benutzername VARCHAR(50) NOT NULL UNIQUE,"+
"Passwort VARCHAR(255),"+
                "PRIMARY KEY(BenutzerId))");

            DbWrapper.Wrapper.RunNonQuery(
                "CREATE TABLE IF NOT EXISTS FoA_QrCodes (" +
                "QrID VARCHAR(8) NOT NULL, " +
                "PRIMARY KEY(QrID))");

            DbWrapper.Wrapper.RunNonQuery("CREATE TABLE if NOT EXISTS Vote" +
                "(VotingId INT NOT NULL AUTO_INCREMENT,QrId VARCHAR(8) NOT NULL," +
                "Fav INT NOT NULL,DaOther1 INT NOT NULL," +
                "DaOther2 INT NOT NULL,DaOther3 INT NOT NULL," +
                "DaOther4 INT NOT NULL,DaOther5 INT NOT NULL," +
                "PRIMARY KEY(VotingId), FOREIGN KEY(QrId) " +
                "REFERENCES FoA_QrCodes(QrId) ON DELETE CASCADE," +
                "FOREIGN KEY(Fav) REFERENCES DA(DaId) ON DELETE CASCADE," +
                "FOREIGN KEY(DaOther1) REFERENCES DA(DaId) ON DELETE CASCADE," +
                "FOREIGN KEY(DaOther2) REFERENCES DA(DaId) ON DELETE CASCADE," +
                "FOREIGN KEY(DaOther3) REFERENCES DA(DaId) ON DELETE CASCADE," +
                "FOREIGN KEY(DaOther4) REFERENCES DA(DaId) ON DELETE CASCADE," +
                "FOREIGN KEY(DaOther5) REFERENCES DA(DaId) ON DELETE CASCADE);");

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
            $"WHERE QrId NOT IN (SELECT QrId FROM Vote)");
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
            "TRUNCATE TABLE DA;TRUNCATE TABLE foa_qrcodes;" +
            "TRUNCATE TABLE Vote;" +
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
            DA.CreateClassesSQL();
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