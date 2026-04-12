namespace TS4Plumbob.Core.DataModels.IdTypes;

public record struct ModId(Guid Value)
{
    public static implicit operator ModId(Guid value) => new ModId(value);
    public static explicit operator Guid(ModId value) => value.Value;
}