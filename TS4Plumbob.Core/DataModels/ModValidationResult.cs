namespace TS4Plumbob.Core.DataModels;

public record ModValidationResult
{
    public ModEntry ModRecord { get; init; }
    public bool IsValid { get; init; }
    public bool ExistsOnDisk { get; init; }

    public ModValidationResult() { }

    public ModValidationResult(ModEntry modRecord, bool isValid, bool existsOnDisk)
    {
        ModRecord = modRecord;
        IsValid = isValid;
        ExistsOnDisk = existsOnDisk;
    }
}