namespace IDEK.Tools.ShocktroopUtils.Services;

public interface IAsyncServiceProvider<T>
{
    Task<T> CreateAsync();
}