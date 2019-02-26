using System;
using ColiseeSharp.Api.Bindings;
using Ninject;
using Ninject.Syntax;

namespace ColiseeSharp.Api.Extensions {
    public static class ModuleExtensions {
        /// <summary>Binds a runtime mode with a particular name that can be invoked when the program starts using command line arguments.</summary>
        /// <typeparam name="T">The concrete type of the runtime mode to bind.</typeparam>
        /// <param name="root">The binding root.</param>
        /// <param name="name">The name of the runtime mode. This name must be specified in the command line arguments for this runtime mode to be used.</param>
        public static IBindingWithOrOnSyntax<T> BindRuntimeMode<T>(this IBindingRoot root, string name) where T : class, IRuntimeMode {
            // Create bindings for the runtime mode so it can be constructed by the container
            IBindingWithOrOnSyntax<T> binding = root.Bind<IRuntimeMode>().To<T>().InSingletonScope().Named(name);

            // Create a named binding for the runtime mode used by the main program
            root.Bind<INamedBinding<IRuntimeMode>>().To<LazyNamedBindingWrapper<T>>().InSingletonScope().Named(name);
            root.Bind<string>().ToConstant(name).WhenInjectedInto<LazyNamedBindingWrapper<T>>().Named("NAME");
            root.BindLazy<T>().WhenInjectedInto<LazyNamedBindingWrapper<T>>();

            return binding;
        }

        /// <summary>Creates a binding for <see cref="Lazy{T}"/> which allows the type to be constructed on demand rather than when injected.</summary>
        /// <typeparam name="T">The type to create a lazy binding for.</typeparam>
        /// <param name="root">The binding root.</param>
        /// <returns>The lazy binding syntax.</returns>
        public static IBindingWhenInNamedWithOrOnSyntax<Lazy<T>> BindLazy<T>(this IBindingRoot root) {
            return root.Bind<Lazy<T>>().ToMethod(context => new Lazy<T>(() => context.Kernel.Get<T>()));
        }
    }
}
