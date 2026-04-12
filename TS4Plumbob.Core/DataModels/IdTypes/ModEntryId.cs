namespace TS4Plumbob.Core.DataModels.IdTypes;

public record struct ModEntryId(Guid Value)
{
    public static implicit operator ModEntryId(Guid value) => new ModEntryId(value);
    public static explicit operator Guid(ModEntryId value) => value.Value;
}