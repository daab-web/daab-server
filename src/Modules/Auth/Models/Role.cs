namespace Daab.Modules.Auth.Models;

public class Role
{
    public string Id { get; private set; }

    public string Name { get; private set; }

    public ICollection<User> Users { get; private set; } = [];

    public Role(string name)
    {
        Id = Ulid.NewUlid().ToString();

        Name = name;
    }
}
