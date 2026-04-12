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

using System.Collections.Generic;
using System;

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
    /// <see cref="Register"/> a given class instance to act as the go to service object instance for that type.
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
        public static bool loggingEnabled = false;
        
        private static readonly Dictionary<Type, object> Services = new();
        
        /// <summary>
        /// dict of explicitly specified procedures for jumpstarting given services.
        /// </summary>
        private static readonly Dictionary<Type, Func<object>> Jumpstarters = new();
        
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
        public static void BindJumpstarter<TService, TProvider>() where TProvider : IServiceProvider<TService>, new()
        {
            Jumpstarters[typeof(TService)] = () => new TProvider().Create();
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
        public static void BindJumpstarter<TService>(Func<object> jumpstarter)
        {
            Jumpstarters[typeof(TService)] = jumpstarter;
        }
        
        /// <summary>
        /// You need to register the service before it can be Resolved or accessed. You can only register one instance of one type.
        /// </summary>
        /// <param name="serviceInstance">An instance of the service</param>
        /// <typeparam name="T">The service type</typeparam>
        /// <remarks>
        /// It is HIGHLY recommended to use an interface (or a wrapper class) for <see cref="T"/>, as this quarantines the actual
        /// service from all the project usages touching it. This prevents the chaos of updates or
        /// other changes to the service from rippling out to the rest of your project's growing and iterative codebase.
        /// <para/>
        /// An interface will be easier to set up,
        /// but if you don't have direct edit access to the service class source, a wrapper is the next best thing.
        /// </remarks>
        /// <returns>
        /// The registered service of type <see cref="T"/>. If one was already registered, it returns that instead of the inputted one.
        /// </returns>
        public static T Register<T>(object serviceInstance)
        {
            if (Services.ContainsKey(typeof(T)))
            {
                LogError("Service is already registered! Please unregister the current service before registering a new instance.");
                return (T)Services[typeof(T)];
            }
            else if (serviceInstance is T)
            {
                Log("Register called successfully.");
                Services[typeof(T)] = serviceInstance;
                Log($"{typeof(T)} : {serviceInstance.GetType()}");
                
                if (serviceInstance is IService patternedService)
                    patternedService.OnRegister(typeof(T));
                
                return (T)serviceInstance;
            }
            else
            {
                LogError("Object does not derived from the service type!");
                return default(T);
            }
        }

        /// <inheritdoc cref="Register{T}" />
        ///<returns><c>true</c> if the service was newly registered,
        /// <c>false</c> if one was already registered or
        /// if it otherwise fails. This version does NOT emit log errors.
        /// </returns>
        public static bool TryRegister<T>(object serviceInstance)
        {
            if (Services.ContainsKey(typeof(T)))
            {
                return false;
            }
            else if (serviceInstance is T)
            {
                Log("TryRegister called successfully.");
                Services[typeof(T)] = serviceInstance;
                Log($"{typeof(T)} : {serviceInstance.GetType()}");
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void ReregisterBoundService<T>()
        {
            if(!Jumpstarters.ContainsKey(typeof(T))) 
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
        public static void Unregister<T>()
        {
            if (Services.TryGetValue(typeof(T), out object? instance))
            {
                if (instance is IService patternedService)
                    patternedService.OnUnregister(typeof(T));
                Services.Remove(typeof(T));
            }
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
            if (TryResolve(out T activeService) && activeService.Equals(serviceInstance))
            {
                Unregister<T>();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the service. This will fail if the service was not first registered via the `Register` method.
        /// </summary>
        /// <typeparam name="T">The service type</typeparam>
        /// <returns>Returns a registered instance of the type requested.</returns>
        /// <remarks>we keep this fast to avoid slowing down most use cases.
        /// If you are unsure if the service is available at the time of calling,
        /// either rework your code so that it will be available,
        /// or, if you're in a pinch, call either <see cref="IsRegistered{T}"/> or <see cref="TryJumpStart{T}"/>.
        /// </remarks>
        public static T Resolve<T>(bool jumpStartIfNotFound = true)
        {
            // return (T)Services[typeof(T)];
            bool success = Services.TryGetValue(typeof(T), out object instance);
            if(success) return (T)instance;

            if (!jumpStartIfNotFound) return default;
            // TryAutoBindJumpstarter<T>();
#if UNITY_5_3_OR_NEWER
            if (typeof(UnityEngine.Component).IsAssignableFrom(typeof(T)))
            {
                Log("Service not registered, but is a UnityEngine.Component - instantiating one now!");
                var serviceGObj = new UnityEngine.GameObject($"[Service] {typeof(T).Name} (Jumpstarted)");

                //double cast bypass trick (avoids expensive reflection and turns into nice IL)
                T newComponentService = (T)(object)serviceGObj.AddComponent(typeof(T));

                Register<T>(newComponentService);
                return newComponentService;
            }
#endif

            if (Jumpstarters.TryGetValue(typeof(T), out Func<object> jumpstarter))
            {
                T output = (T)jumpstarter?.Invoke();

                Register<T>(output);
                return output;
            }

            return default;
        }

        public static bool TryResolve<T>(out T serviceInstance, bool jumpStartIfNotFound = false)
        {
            bool success = Services.TryGetValue(typeof(T), out object instance);

            if (success)
            {
                serviceInstance = (T)instance;
                return true;
            }

            if (!jumpStartIfNotFound)
            {
                serviceInstance = default(T);
                return false;
            }
            
            // TryAutoBindJumpstarter<T>();
#if UNITY_5_3_OR_NEWER
            if (typeof(UnityEngine.Component).IsAssignableFrom(typeof(T)))
            {
                Log("Service not registered, but is a UnityEngine.Component - instantiating one now!");
                var serviceGObj = new UnityEngine.GameObject($"[Service] {nameof(T)} (Jumpstarted)");

                //double cast bypass trick (avoids expensive reflection and turns into nice IL)
                T newComponentService = (T)(object)serviceGObj.AddComponent(typeof(T));

                Register<T>(newComponentService);
                serviceInstance = newComponentService;
                return true;
            }
#endif

            if (Jumpstarters.TryGetValue(typeof(T), out Func<object> jumpstarter))
            {
                T output = (T)jumpstarter?.Invoke();

                Register<T>(output);
                serviceInstance = output;
                return true;
            }
            
            serviceInstance = default(T);
            return false;
        }
        
        //this is becoming very overkill right now
        // private static bool TryAutoBindJumpstarter<T>()
        // {
        //     if (!typeof(T).GetCustomAttributes(typeof(AutoJumpstartAttribute), true).Any())
        //         return false;
        //     
        //     //it has the attribute, lets attempt the jumpstart by locating its service provider...how?
        // }

        /// <summary>
        /// Clears all registered services. BUT does not perform any dispose or cleanup on the services.
        /// </summary>
        public static void Reset()
        {
            Services.Clear();
        }

        /// <summary>
        /// Checks if a service is registered.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsRegistered<T>()
        {
            return Services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Only works with concrete classes, but enables the service to be lazily loaded automatically.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ResolveJumpStart<T>() where T : new()
        {
            if(TryResolve(out T activeService)) return activeService;

#if UNITY_5_3_OR_NEWER
            if (typeof(UnityEngine.Component).IsAssignableFrom(typeof(T)))
            {
                var serviceGObj = new UnityEngine.GameObject($"[Service] {nameof(T)} (Jumpstarted)");
                
                //double cast bypass trick (avoids expensive reflection and turns into nice IL)
                T newComponentService = (T)(object)serviceGObj.AddComponent(typeof(T));
                
                Register<T>(newComponentService);
                return newComponentService;
            }
#endif

            if (Jumpstarters.TryGetValue(typeof(T), out Func<object> jumpstarter))
            {
                T output = (T)jumpstarter?.Invoke();

                Register<T>(output);
                return output;
            }
            
            T newService = new T();
            
            Register<T>(newService);
            return newService;
        }
        
        //TODO: use an actual Logger class

        private static void Log(string msg)
        {
            if (!loggingEnabled) return;
            
            string formattedMsg = "Service Locator: " + msg;
#if UNITY_5_3_OR_NEWER
            UnityEngine.Debug.Log(formattedMsg);
#else
            Console.WriteLine(formattedMsg);
#endif
        }

        private static void LogError(string msg)
        {
            if (!loggingEnabled) return;
            string formattedMsg = "Service Locator: [Error] " + msg;
#if UNITY_5_3_OR_NEWER
            UnityEngine.Debug.LogError(formattedMsg);
#else
            Console.WriteLine(formattedMsg);
#endif
        }
    }
}
