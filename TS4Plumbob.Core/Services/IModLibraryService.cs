using IDEK.Tools.ShocktroopUtils.Services;

namespace TS4Plumbob.Core.DataModels;

public interface IModLibraryService : IService
{
    string Serialize();
    ModEntry? GetMod(Guid id);
    void AddMod(ModEntry modEntry);
    void RemoveMod(ModEntry modEntry);
    bool IsValidMod(ModEntry? mod);
}

public record ModLibraryValidationResult(ModValidationResult[] Results);
public record ModValidationResult(ModEntry ModRecord, bool IsValid, bool ExistsOnDisk);