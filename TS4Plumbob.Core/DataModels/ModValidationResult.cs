namespace TS4Plumbob.Core.DataModels;

public record ModValidationResult(ModEntry ModRecord, bool IsValid, bool ExistsOnDisk);