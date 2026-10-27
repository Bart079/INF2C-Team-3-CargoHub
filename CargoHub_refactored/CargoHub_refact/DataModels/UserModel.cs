public class UserModel
{
    public int ID { get; set; }
    public string Api_Key { get; set; }
    public string App_Name { get; set; }

    public UserModel()
    {
        Api_Key = "";
        App_Name = "";
    }

    public UserModel(int id, string apiKey, string appName)
    {
        ID = id;
        Api_Key = apiKey;
        App_Name = appName;
    }
}