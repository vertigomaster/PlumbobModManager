namespace IDEK.Tools.ShocktroopUtils.Services;

public interface IService
{
    void OnRegister(Type type);
    void OnUnregister(Type type);
}