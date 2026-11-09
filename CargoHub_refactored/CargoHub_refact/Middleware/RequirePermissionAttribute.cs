namespace Project.Middleware;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
public sealed class RequirePermissionAttribute : Attribute
{
    public string Resource { get; }

    public RequirePermissionAttribute(string resource)
    {
        if (string.IsNullOrWhiteSpace(resource))
            throw new ArgumentException("A resource is required.", nameof(resource));

        Resource = resource;
    }
}
