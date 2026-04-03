namespace IDEK.Tools.ShocktroopUtils.Services
{
    /// <summary>
    /// Intermediary object that allows defining autonomous service creation (and its implementation) upon demand
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IServiceProvider<out T> //this could be covariant, according to the IDE, but I'm 
    {
        /// <summary>
        /// Defined by an implementation that specifies the procedure (the how) for creating the service (the what),
        /// called by the ServiceLocator (the when) on the behalf of a consumer (the why).
        /// </summary>
        /// <returns>A constructed service of type <see cref="T"/>.</returns>
        T Create();
    }
}