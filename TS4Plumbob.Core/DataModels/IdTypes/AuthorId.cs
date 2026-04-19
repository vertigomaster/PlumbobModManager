namespace TS4Plumbob.Core.DataModels.IdTypes;

//to avoid confusion

public record struct AuthorId(Guid Value)
{
    public static implicit operator AuthorId(Guid value) => new AuthorId(value);
    public static explicit operator Guid(AuthorId value) => value.Value;
}