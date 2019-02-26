using System;
using Ninject;

namespace ColiseeSharp.Api.Bindings {
    internal class LazyNamedBindingWrapper<T> : INamedBinding<T> {
        private readonly Lazy<T> _valueFactory;

        public string Name { get; }
        public T Value => this._valueFactory.Value;

        public LazyNamedBindingWrapper(Lazy<T> valueFactory, [Named("NAME")] string name) {
            this._valueFactory = valueFactory;
            this.Name = name;
        }
    }
}