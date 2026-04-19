namespace TS4Plumbob.Core.DataModels.IdTypes;

public record struct RigId(Guid Value)
{
    public static implicit operator RigId(Guid value) => new RigId(value);
    public static explicit operator Guid(RigId value) => value.Value;
}