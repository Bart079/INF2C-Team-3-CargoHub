public class UserModel
{
    public int ID { get; set; }
    public string Api_Key_Hash { get; set; }
    public string App_Name { get; set; }

    public UserModel()
    {
        Api_Key_Hash = "";
        App_Name = "";
    }

    public UserModel(int id, string apiKeyHash, string appName)
    {
        ID = id;
        Api_Key_Hash = apiKeyHash;
        App_Name = appName;
    }
}