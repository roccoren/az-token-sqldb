namespace AzureSqlTokenAuth.Models;

public class User
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateUserRequest
{
    public required string Name { get; set; }
    public required string Email { get; set; }
}

public class UpdateUserRequest
{
    public required string Name { get; set; }
    public required string Email { get; set; }
}