using ZstdSharp.Unsafe;

public class FoA_FriendsofAward
{
    int ID { get; }
    string Titel { get; set; }
    string Schueler { get; set; }
    int Count { get; set; }

    public FoA_FriendsofAward(int id, string titel, string schueler, int count)
    {
        ID = id;
        Titel = titel;
        Schueler = schueler;
        Count = count;
    }

    static bool SaveToDb(FoA_FriendsofAward foaDA)
    {
        try
        {
            DbWrapper.Wrapper.Open();
            DbWrapper.Wrapper.RunNonQuery($"CREATE TABLE IF NOT EXISTS FoA_FriendsOfAward (Nr AUTO_INCREMENT, " +
                $"Titel VARCHAR(100) NOT NULL, Schueler VARCHAR(200) NOT NULL");
            DbWrapper.Wrapper.RunNonQuery($"INSERT INTO VALUES {foaDA}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }
}