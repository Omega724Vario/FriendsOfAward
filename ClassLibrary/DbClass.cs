using MySql.Data.MySqlClient;
using Mysqlx.Connection;
using System.Data;
using ZstdSharp.Unsafe;

public class FoA_DA
{
    int ID { get; } //in als NULL übergeben, Auto_Increment via sql
    string Abteilung { get; }
    string Titel { get; set; }
    string Schueler { get; set; }

    public FoA_DA(int id, string abteilung, string titel, string schueler)
    {
        ID = id;
        Abteilung = abteilung;
        Titel = titel;
        Schueler = schueler;
    }

    public static bool SaveDAtoDb(List<FoA_DA> foaDA)
    {
        try
        {
            CreateClassesSQL();
            foreach(FoA_DA da in foaDA)
            DbWrapper.Wrapper.RunNonQuery(
                $"INSERT INTO FoA_DA (Abteilung, Titel, Schueler) VALUES ('{da.Abteilung}', '{da.Titel}', '{da.Schueler}')");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }
    public class FoA_QrCodes
    {
        string QrId { get; set; }
        public FoA_QrCodes(string qrId)
        {
            QrId = qrId;
        }
        static bool SaveQRtoDb(List<FoA_QrCodes> qrCodes)
        {
            try
            {
                CreateClassesSQL();
                foreach(FoA_QrCodes qrCode in qrCodes)
                {
                    DbWrapper.Wrapper.RunNonQuery($"INSERT INTO FoA_QrCodes (QrID) VALUES ('{qrCode.QrId}')");
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
        int DaId { get; }

        public FoA_Voting_System(int votingId, string qrId, int daId)
        {
            VotingId = votingId;
            QrId = qrId;
            DaId = daId;
        }
    }
    static bool CreateClassesSQL()
    {
        try
        {
            DbWrapper.Wrapper.RunNonQuery($"CREATE TABLE IF NOT EXISTS FoA_DA (DaId INT NOT NULL AUTO_INCREMENT, Abteilung VARCHAR(3) NOT NULL" +
                $"Titel VARCHAR(100) NOT NULL, Schueler VARCHAR(200) NOT NULL, PRIMARY KEY (DaId)");
            DbWrapper.Wrapper.RunNonQuery($"CREATE TABLE IF NOT EXISTS FoA_QrCodes " +
                $"(QrID VARCHAR(8) NOT NULL, PRIMARY KEY(QrID)");
            DbWrapper.Wrapper.RunNonQuery($"CREATE TABLE if NOT EXISTS FoA_Voting_System " +
                $"(VotingId INT NOT NULL AUTO_INCREMENT, QrId VARCHAR(8) NOT NULL, DaId INT NOT NULL, " +
                $"PRIMARY KEY (VotingId), FOREIGN KEY (QrId) REFERENCES FoA_QrCodes (QrId) ON DELETE CASCADE, " +
                $"FOREIGN KEY (DaId) REFERENCES FoA_DA (DaId) ON DELETE CASCADE");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }

    static List<string> UnUsedQrCodes ()
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
}