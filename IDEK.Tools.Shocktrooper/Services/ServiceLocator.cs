/*
* Author: Dayvid jones
* http://www.dayvid.com
* Copyright (c) Superhero Robot 2018
* http://www.superherorobot.com
* Manged by Dorkbots
* http://www.dorkbots.com/
* Version: 1
* 
* Licence Agreement
*
* You may distribute and modify this class freely, provided that you leave this header intact,
* and add appropriate headers to indicate your changes. Credit is appreciated in applications
* that use this code, but is not required.
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
* 
* Author: Chris Crowell
* Added IsRegistered method to check if a service is registered.
*
* Author: Julian Noel (IDEK Studios)
* Adapted for projects using Shocktroop-Utils, made it not Unity specific, and tidied up a little.
* Added binding workflow - intended more for non-Unity usages, as Unity services are often components in-scene.
* Added rudimentary Disposable pattern support 
*/

#nullable enable
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IDEK.Tools.Logging;

namespace IDEK.Tools.ShocktroopUtils.Services
{
    /// <summary>
    /// This class is used in the Service Locator Pattern (read more -> https://en.wikipedia.org/wiki/Service_locator_pattern).
    /// The service locator pattern is a design pattern used in software development to encapsulate the processes involved
    /// in obtaining a service with a strong abstraction layer.
    /// In other words, separating the process of GETTING the actual service from the process of USING the service.
    /// This lets you change/swap out/update services without needing to refactor your project.
    /// Especially important for iteration-heavy projects like games or things involving live-ops.
    /// <para/>
    /// This pattern uses a central registry known as the "service locator" which on request returns the information
    /// necessary to perform a certain task.
    /// </summary>
    /// <remarks>
    /// How to use:
    /// <br/>
    /// <see cref="TryRegister"/> a given class instance to act as the go to service object instance for that type.
    /// The type PARAMETER you specify will be the type used to retrieve the object you pass in.
    /// Thusly, it is recommended (though not required) to use an interface for this type parameter in order to
    /// further reduce coupling. The object instance MUST be resolvable to the type parameter (so either being the class, inheriting from it, or implementing it).
    /// <para/>
    /// Even more ideally, this should be a project-specific type describing the functionality your project needs from this kind of service.
    /// If the needed implementation being used is third party, is may also be wise to actually register a wrapper class that isolates the integration
    /// 
    /// </remarks>
    public static class ServiceLocator
    {
        /// <summary>
        /// Exception reserved for incorrect and fatal <see cref="ServiceLocator"/> logic.
        /// <para/>
        /// This exception distinguishes internal <see cref="ServiceLocator"/> issues
        /// from faults caused by incorrect usage of the <see cref="ServiceLocator"/>.
        /// </summary>
        /// <remarks>
        /// If you encounter one of these, contact the author of <see cref="ServiceLocator"/>
        /// by submitting a github issue or through other means.
        /// </remarks>
        public class InternalException : Exception
        {
            public InternalException(Type serviceType, string? message = null)
                : base(message ?? $"Internal service locator exception regarding type: {serviceType.FullName}")
            {
                ServiceType = serviceType;
            }

            public Type ServiceType { get; }
        }

        public static bool loggingEnabled = false;
        
        private static readonly ConcurrentDictionary<Type, object> _services = new();
        
        /// <summary>
        /// dict of explicitly specified procedures for jumpstarting given services.
        /// </summary>
        /// <remarks>
        /// Read/write must be protected to avoid race conditions in multithreaded contexts.
        /// </remarks>
        private static readonly ConcurrentDictionary<Type, Func<object>> _jumpstarters = new();
        /// <inheritdoc cref="_jumpstarters"/>
        private static readonly ConcurrentDictionary<Type, Func<Task<object>>> _asyncJumpstarters = new();

        /// <summary>
        /// Tracks all active jumpstarter tasks
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Task> _runningJumpstarters = new();

        private static readonly ConcurrentDictionary<Type, SemaphoreSlim> _serviceMutateGate = new();

        /// <summary>
        /// Binds the given provider to the output type
        /// </summary>
        /// <typeparam name="TService">
        /// The type used to request the service - may be an interface that the actual
        /// concrete service implements or a concrete class as well.
        /// </typeparam>
        /// <typeparam name="TProvider">The bundled service provider type which
        /// facilitates both simple and complex factory logic for creating an
        /// object that is reasonably compatible with <see cref="TService"/>.
        /// <br/>
        /// Note that the service object will only resolve if it can be casted as a <see cref="TService"/>.
        /// </typeparam>
        /// <remarks>
        /// overkill for engines like Unity, where service components can be
        /// pre-spawned in the scene, but quite useful in other .NET environments
        /// </remarks>
        public static void Bind<TService, TProvider>() where TProvider : IServiceProvider<TService>, new()
        {
            _jumpstarters[typeof(TService)] = () => new TProvider().Create();
        }
        
        public static void BindAsync<TService, TProvider>() where TProvider : IAsyncServiceProvider<TService>, new()
        {
            _asyncJumpstarters[typeof(TService)] = async () =>
                await new TProvider().CreateAsync() ?? throw new InvalidOperationException(
                    "Bound Async service provider returned null instead of a service.");
        }

        /// <summary>
        /// Binds the
        /// </summary>
        /// <param name="jumpstarter">
        /// The given function that will, presumably, produce a type
        /// that is reasonably compatible with <see cref="TService"/>.
        /// <br/>
        /// Note that the service object will only resolve if it can be casted as a <see cref="TService"/>.
        /// </param>
        /// <typeparam name="TService">
        /// The type used to request the service - may be an interface that the actual
        /// concrete service implements or a concrete class as well.
        /// </typeparam>
        /// <remarks>
        /// overkill for engines like Unity, where service components can be
        /// pre-spawned in the scene, but quite useful in other .NET environments
        /// </remarks>
        public static void Bind<TService>(Func<object> jumpstarter)
        {
            _jumpstarters[typeof(TService)] = jumpstarter;
            Log($"{typeof(TService)} bound.");
        }
        
        //TODO:
        public static void BindAsync<TService>(Func<Task<object>> jumpstarter)
        {
            _asyncJumpstarters[typeof(TService)] = jumpstarter;
            Log($"{typeof(TService)} bound.");
        }

        /// <summary>
        /// You need to register the service before it can be Resolved or accessed. You can only register one instance of one type.
        /// </summary>
        /// <param name="serviceInstance">An instance of the service</param>
        /// <typeparam name="TService">The service type</typeparam>
        /// ///<returns><c>true</c> if the service was newly registered,
        /// <c>false</c> if one was already registered or
        /// if it otherwise fails. This version does NOT emit log errors.
        /// </returns>
        /// <remarks>
        /// It is HIGHLY recommended to use an interface (or a wrapper class) for <see cref="TService"/>, as this quarantines the actual
        /// service from all the project usages touching it. This prevents the chaos of updates or
        /// other changes to the service from rippling out to the rest of your project's growing and iterative codebase.
        /// <para/>
        /// An interface will be easier to set up,
        /// but if you don't have direct edit access to the service class source, a wrapper is the next best thing.
        /// </remarks>
        private static RegistrationResult TryRegister<TService>(
            object serviceInstance) =>
            TryRegister(typeof(TService), serviceInstance);
        
        private static RegistrationResult TryRegister(
            Type type,
            object serviceInstance)
        {
            if (!type.IsInstanceOfType(serviceInstance))
            {
                LogError($"Registration Failed; Object {serviceInstance} does not derive from the service type!");
                return RegistrationResult.InvalidInstance;
            }

            //if add fails, someone beat us to it. Silently fail; we can't guarantee the given instance was registered.
            if (!_services.TryAdd(type, serviceInstance)) return RegistrationResult.AlreadyRegistered;

            Log($"Registered {type} => {serviceInstance.GetType()}");
            if (serviceInstance is IService patternedService)
                patternedService.OnRegister(type);

            return RegistrationResult.Success;
        }

        public enum RegistrationResult
        {
            Success = 1,
            AlreadyRegistered = 2,
            InvalidInstance = 4
        }

        // /// <inheritdoc cref="_TryRegister_Internal{TService}" />
        // ///<returns><c>true</c> if the service was newly registered,
        // /// <c>false</c> if one was already registered or
        // /// if it otherwise fails. This version does NOT emit log errors.
        // /// </returns>
        // public static bool TryRegister<T>(object serviceInstance)
        // {
        //     if (_services.ContainsKey(typeof(T)))
        //     {
        //         return false;
        //     }
        //     else if (serviceInstance is T)
        //     {
        //         // Log("TryRegister called successfully.");
        //         // _services[typeof(T)] = serviceInstance;
        //         // Log($"{typeof(T)} :=> {serviceInstance.GetType()}");
        //         _TryRegister_Internal<T>(serviceInstance);
        //         return true;
        //     }
        //     else
        //     {
        //         return false;
        //     }
        // }

        public static void ReregisterBoundService<T>()
        {
            if(!_jumpstarters.ContainsKey(typeof(T)))
                throw new InvalidOperationException(
                    $"No bound service found for type {typeof(T)}. Ensure a jumpstarter " +
                    $"is bound for this type before attempting to reregister it.");
            
            Unregister<T>();
            Resolve<T>();
        }

        /// <summary>
        /// Unregisters current instance of a service.
        /// </summary>
        /// <typeparam name="T">The service type</typeparam>
        public static void Unregister<T>() => Unregister(typeof(T));
        

        public static void Unregister(Type type)
        {
            if (!_services.TryGetValue(type, out object? instance)) return;
            
            Log($"Unregistering {type}...");
            if (instance is IService patternedService)
                patternedService.OnUnregister(type);
            _services.Remove(type, out _);
            Log($"{type} unregistered.");
        }

        /// <summary>
        /// Attempts to unregister the specific instance.
        /// Only works if that instance is a registered service.
        /// <br/>
        /// A great guard to use when cleaning up.
        /// </summary>
        /// <param name="serviceInstance">The given instance being unregistered.</param>
        /// <typeparam name="T">The type of the given instance being unregistered.</typeparam>
        /// <returns>
        /// True if <see cref="serviceInstance"/> was already registered and
        /// if we were able to successfully unregister it.
        /// <para/>
        /// False if the given type is not registered or if the given instance is
        /// NOT the registered instance.
        /// </returns>
        public static bool TryUnregister<T>(object serviceInstance)
        {
            var activeService = Resolve<T>();
            if (activeService != null && activeService.Equals(serviceInstance))
            {
                Unregister<T>();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Resolves the service type to the currently implemented instance.
        /// <br/>
        /// This will fail if the service was not first registered via a binding
        /// or manually calling <see cref="TryRegister{T}"/>.
        /// <para/>
        /// NOT THREAD SAFE.
        /// <br/>
        /// For use outside single-threaded contexts, see <see cref="ResolveAsync"/>.
        /// </summary>
        /// <typeparam name="T">The service type</typeparam>
        /// <returns>Returns a registered instance of the type requested.</returns>
        /// <remarks>we keep this fast to avoid slowing down most use cases.
        /// If you are unsure if the service is available at the time of calling,
        /// either rework your code so that it will be available,
        /// or, if you're in a pinch, call <see cref="IsRegistered{T}"/>.
        /// </remarks>
        public static T? Resolve<T>(bool jumpStartIfNotFound = true)
        {
            TryResolve(out T? serviceInstance, jumpStartIfNotFound);
            return serviceInstance;
        }

        public static bool TryResolve<TService>(out TService? serviceInstance, bool jumpStartIfNotFound = true)
        {
            Type serviceType = typeof(TService);

            if (_TryQuickResolve(serviceType, out var quickResolveResult)) {
                serviceInstance = (TService?)quickResolveResult;
                return true;
            }

            if (!jumpStartIfNotFound) {
                serviceInstance = default;
                return false;
            }

            if(_Jumpstart(out serviceInstance)) return true;

            //TODO: confirm unity service provider is set up
            if (_TryResolveUnityComponentService(out serviceInstance)) return true;

            _AssertTypeJumpstarterIsNotAsync<TService>();

            return false;
        }


        public static async Task<TService?> ResolveAsync<TService>(bool jumpStartIfNotFound = true)
        {
            Type serviceType = typeof(TService);

            if(_TryQuickResolve(serviceType, out var serviceInstance))
                return (TService?)serviceInstance;

            if (!jumpStartIfNotFound) return default;

            //start with the async jumpstarter tasks in a threadsafe way
            var potentialService = await _AttemptAsyncJumpstartAsync_ThreadSafe<TService>(serviceType);
            if(potentialService != null) return potentialService;

            //no async jumpstarters found, check synchro jumpstarters in a threadsafe way
            return await _AttemptGatedJumpstartAsync_ThreadSafe<TService>(serviceType);
        }

        private static bool _TryQuickResolve(Type serviceType,
            out object? serviceInstance)
        {
            if (_services.TryGetValue(serviceType, out object instance))
            {
                serviceInstance = instance ??
                    throw new InternalException(serviceType,
                        $"Null Service instance returned from internal " +
                        $"service dictionary for type {serviceType.FullName}");
                return true;
            }

            serviceInstance = default;
            return false;
        }

        private static void _AssertTypeJumpstarterIsNotAsync<TService>()
        {
            if (!_asyncJumpstarters.ContainsKey(typeof(TService))) return;

            throw new InvalidOperationException(
                $"Async services must be resolved with {nameof(ResolveAsync)}(), not {nameof(Resolve)}(). " +
                $"The type {typeof(TService).FullName} is bound to an {nameof(IAsyncServiceProvider<TService>)}/async jumpstarter. " +
                $"\nUse {nameof(ResolveAsync)}() when possible, as it can handle both asynchronous " +
                $"and synchronous bindings.");
        }

        private static async Task<TService> _AttemptAsyncJumpstartAsync_ThreadSafe<TService>(
            Type serviceType)
        {
            //track or start (and store) the jumpstarter.
            Task<TService>? jumpstarterTask = _runningJumpstarters.GetOrAdd(serviceType,
                _ => _JumpstartAsync<TService>()) as Task<TService>;

            //while the JumpstartAsync task may have a null RESULT, the TASK itself should not be null.
            if (jumpstarterTask == null) throw new InternalException(serviceType,
                $"Null jumpstarter task for type {serviceType} stored in _runningJumpstarters. " +
                $"dictionary is corrupt.");

            //Await the async jumpstarter, which may return null if it "successfully fails"
            //to find a jumpstarter.
            TService potentialService = await jumpstarterTask;
            return potentialService;
        }

        /// <summary>
        /// Attempts to run a type's synchronous jumpstarter in a thread-safe way
        /// </summary>
        /// <param name="serviceType"></param>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        private static async Task<TService?> _AttemptGatedJumpstartAsync_ThreadSafe<TService>(
            Type serviceType)
        {
            var gate = _serviceMutateGate.GetOrAdd(serviceType, _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync();

            try
            {
                //double-check
                if (_services.TryGetValue(serviceType, out object existing))
                    return (TService)existing;

                //success or not, this is our last stop,
                //so we don't check the bool returned from _Jumpstart
                _Jumpstart(out TService? resolve);
                return resolve;
            }
            finally
            {
                gate.Release();
            }
        }

        // [Obsolete("Use Resolve() and ResolveAsync for now. This function is out of date.", true)]

        /// <summary>
        /// Clears all registered services. Does not clear bindings.
        /// </summary>
        public static void ClearAllServices()
        {
            Log("Resetting Service Locator...");
            
            foreach (KeyValuePair<Type, object> service in _services)
            {
                Unregister(service.Key);
                
                if(service.Value is IDisposable disposable)
                    disposable.Dispose();
            }
            
            //just for good measure
            _services.Clear();
            Log("Service Locator Reset. All services unregistered.");
        }

        /// <summary>
        /// Clears all registered bindings. Does not clear services.
        /// </summary>
        public static void ClearAllBindings()
        {
            Log("Resetting Service Locator bindings...");
            
            _jumpstarters.Clear();
            _asyncJumpstarters.Clear();
            
            Log("Service Locator bindings reset.");
        }

        /// <summary>
        /// Fully resets the service locator in all aspects.
        /// </summary>
        public static void Reset()
        {
            ClearAllServices();
            ClearAllBindings();
        }

        public static bool TryUnbind<T>()
        {
            bool success = _jumpstarters.Remove(typeof(T), out _) || _asyncJumpstarters.Remove(typeof(T), out _);
            if (success) Log($"{typeof(T)} unbound.");
            return success;
        }

        /// <summary>
        /// Checks if a service is registered.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsRegistered<T>()
        {
            return _services.ContainsKey(typeof(T));
        }

        //TODO: use an actual Logger class

        private static void Log(string msg)
        {
            if (!loggingEnabled) return;
            
            string formattedMsg = "Service Locator: " + msg;
#if UNITY_5_3_OR_NEWER
            UnityEngine.Debug.Log(formattedMsg);
#else
            ConsoleLog.Log(formattedMsg);
            // Console.WriteLine(formattedMsg);
#endif
        }

        private static void LogError(string msg)
        {
            if (!loggingEnabled) return;
            string formattedMsg = "Service Locator: [Error] " + msg;
#if UNITY_5_3_OR_NEWER
            UnityEngine.Debug.LogError(formattedMsg);
#else
            ConsoleLog.LogError(formattedMsg);
            // Console.WriteLine(formattedMsg);
#endif
        }

        private static bool _Jumpstart<T>(out T? resolve)
        {
            if (_jumpstarters.TryGetValue(typeof(T), out Func<object> jumpstarter))
            {
                Log($"Jumpstarting {typeof(T)}...");
                T output = (T)jumpstarter.Invoke();
                Log($"{typeof(T)} now resolvable.");
                

                TryRegister<T>(output);
                resolve = output;
                return true;
            }

            resolve = default;
            return false;
        }

        private static async Task<T?> _JumpstartAsync<T>()
        {
            if (_asyncJumpstarters.TryGetValue(typeof(T), out Func<Task<object>> jumpstarter))
            {
                Log($"Jumpstarting {typeof(T)}...");
                T output = (T)await jumpstarter.Invoke();
                Log($"{typeof(T)} now resolvable.");
                
                TryRegister<T>(output);
                return output;
            }

            return default;
        }

        #region Retrieval and Querying

        /// <summary>
        /// Returns all registered services. Does not resolve anything,
        /// and does not include bindings.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object> GetAllRegistered() => _services.Values;

        /// <summary>
        /// Returns all registered service types.
        /// Does not resolve anything, and does not include bindings.
        /// </summary>
        /// <remarks>
        /// Reminder that these are the types the services were REGISTERED as,
        /// not the runtime types they currently resolve to.
        /// </remarks>
        public static IEnumerable<Type> GetAllRegisteredTypes() => _services.Keys;

        public static IEnumerable<Type> GetAllBoundTypes() =>
            _jumpstarters.Keys.Concat(_asyncJumpstarters.Keys);

        public static IEnumerable<Type> GetAllBoundSyncTypes() => _jumpstarters.Keys;

        public static IEnumerable<Type> GetAllBoundAsyncTypes() => _asyncJumpstarters.Keys;

        public static IEnumerable<Func<object>> GetAllBoundSyncJumpstarters() =>
            _jumpstarters.Values;

        public static IEnumerable<Func<Task<object>>> GetAllBoundAsyncJumpstarters() =>
            _asyncJumpstarters.Values;

        /// <returns>True if there is synchronous provider logic bound to this type.</returns>
        public static bool IsBoundSync<T>() => _jumpstarters.ContainsKey(typeof(T));

        /// <returns>True if there is asynchronous provider logic bound to this type.</returns>
        public static bool IsBoundAsync<T>() => _asyncJumpstarters.ContainsKey(typeof(T));

        /// <returns>
        /// True if there is provider logic (be it async or not)
        /// bound to this type.
        /// </returns>
        public static bool IsBound<T>() => IsBoundSync<T>() || IsBoundAsync<T>();

        #endregion

        [Obsolete("This will be removed in a future release. " +
            "Use/create a unity component service provider instead.")]
        private static bool _TryResolveUnityComponentService<T>(
            out T? newComponentService)
        {
#if UNITY_5_3_OR_NEWER
            if (typeof(UnityEngine.Component).IsAssignableFrom(typeof(T)))
            {
                Log("Service not registered, but is a UnityEngine.Component - instantiating one now!");
                var serviceGObj = new UnityEngine.GameObject($"[Service] {typeof(T).Name} (Jumpstarted)");

                //double cast bypass trick (avoids expensive reflection and turns into nice IL)
                newComponentService = (T)(object)serviceGObj.AddComponent(typeof(T));

                //not thread safe to begin with, intended for single-threaded usecases
                //In single-threaded cases, this always succeeds since we already confirmed there isn't
                //a prepregistered service and that it is of the correct type.
                TryRegister<T>(newComponentService);
                return true;
            }

            newComponentService = default;
            return false;
#else
            return false;
#endif
        }
    }
}
