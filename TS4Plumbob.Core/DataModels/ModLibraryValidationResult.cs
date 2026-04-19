namespace TS4Plumbob.Core.DataModels;

public record ModLibraryValidationResult
{
    public ModValidationResult[] Results { get; init; }

    public ModLibraryValidationResult() { }

    public ModLibraryValidationResult(ModValidationResult[] results)
    {
        Results = results;
    }

    //it's immutable, so we can cache the results! We don't pre-cache though,
    //since not every record will use all the queries.
    private bool? _hasErrors = null;
    private bool? _hasMissingMods = null;

    public bool HasErrors
    {
        get
        {
            if (_hasErrors == null)
                _hasErrors = Results.Any(r => !r.IsValid);
            return _hasErrors ?? false;
        }
    }

    public bool HasMissingMods
    {
        get
        {
            if (_hasMissingMods == null)
                _hasMissingMods = Results.Any(r => !r.ExistsOnDisk);
            return _hasMissingMods ?? false;
        }
    }

    public bool HasIssues => HasErrors || HasMissingMods;
}