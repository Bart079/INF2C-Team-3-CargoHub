public class UserEndpointPermissionModel
{
    public int ID { get; set; }
    public int User_ID { get; set; }
    public string Resource { get; set; }
    public string Action { get; set; }

    public UserEndpointPermissionModel()
    {
        Resource = "";
        Action = "";
    }

    public UserEndpointPermissionModel(
        int id,
        int userId,
        string resource,
        string action)
    {
        ID = id;
        User_ID = userId;
        Resource = resource;
        Action = action;
    }
}