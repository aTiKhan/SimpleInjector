﻿// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace SimpleInjector.Lifestyles
{
    using System;
    using System.Threading;

    /// <summary>
    /// Defines a lifestyle that caches instances during the lifetime of an explicitly defined scope using the
    /// <see cref="BeginScope(Container)">BeginScope</see>
    /// method. A scope is thread-specific, each thread should define its own scope. Scopes can be nested and
    /// nested scopes will get their own instance. Instances created by this lifestyle can be disposed when
    /// the created scope gets disposed.
    /// </summary>
    /// <example>
    /// The following example shows the usage of the <b>ThreadScopedLifestyle</b> class:
    /// <code lang="cs"><![CDATA[
    /// var container = new Container();
    /// container.Options.DefaultScopedLifestyle = new ThreadScopedLifestyle();
    ///
    /// container.Register<IUnitOfWork, EntityFrameworkUnitOfWork>(Lifestyle.Scoped);
    ///
    /// using (container.BeginLifetimeScope())
    /// {
    ///     var instance1 = container.GetInstance<IUnitOfWork>();
    ///
    ///     // This call will return the same instance.
    ///     var instance2 = container.GetInstance<IUnitOfWork>();
    ///
    ///     Assert.IsTrue(object.ReferenceEquals(instance1, instance2));
    ///
    ///     // Create a nested scope.
    ///     using (container.BeginLifetimeScope())
    ///     {
    ///         // A nested scope gets its own instance.
    ///         var instance3 = container.GetInstance<IUnitOfWork>();
    ///
    ///         Assert.IsFalse(object.ReferenceEquals(instance1, instance3));
    ///
    ///         // This call will return the same instance.
    ///         var instance4 = container.GetInstance<IUnitOfWork>();
    ///
    ///         Assert.IsTrue(object.ReferenceEquals(instance3, instance4));
    ///     }
    /// }
    /// ]]></code>
    /// </example>
    public class ThreadScopedLifestyle : ScopedLifestyle
    {
        private static readonly object ManagerKey = new object();

        /// <summary>Initializes a new instance of the <see cref="ThreadScopedLifestyle"/> class.
        /// The created and cached instance will be disposed when the created
        /// <see cref="Scope"/> instance gets disposed and when the created object implements
        /// <see cref="IDisposable"/>.
        /// </summary>
        public ThreadScopedLifestyle()
            : base("Thread Scoped")
        {
        }

        /// <summary>
        /// Begins a new scope for the given <paramref name="container"/>.
        /// Services, registered using the <see cref="ThreadScopedLifestyle"/> are cached during the
        /// lifetime of that scope. The scope should be disposed explicitly.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns>A new <see cref="Scope"/> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="container"/> is a null reference.</exception>
        /// <example>
        /// <code lang="cs"><![CDATA[
        /// using (ThreadScopedLifestyle.BeginScope())
        /// {
        ///     var handler = (IRequestHandler)container.GetInstance(handlerType);
        ///
        ///     handler.Handle(request);
        /// }
        /// ]]></code>
        /// </example>
        public static Scope BeginScope(Container container)
        {
            Requires.IsNotNull(container, nameof(container));

            return GetScopeManager(container).BeginScope();
        }

        /// <summary>
        /// Creates a delegate that upon invocation return the current <see cref="Scope"/> for this
        /// lifestyle and the given <paramref name="container"/>, or null when the delegate is executed outside
        /// the context of such scope.
        /// </summary>
        /// <param name="container">The container for which the delegate gets created.</param>
        /// <returns>A <see cref="Func{T}"/> delegate. This method never returns null.</returns>
        protected internal override Func<Scope?> CreateCurrentScopeProvider(Container container)
        {
            ScopeManager manager = GetScopeManager(container);

            return () => manager.CurrentScope;
        }

        /// <summary>
        /// Returns the current <see cref="Scope"/> for this lifestyle and the given
        /// <paramref name="container"/>, or null when this method is executed outside the context of a scope.
        /// </summary>
        /// <param name="container">The container instance that is related to the scope to return.</param>
        /// <returns>A <see cref="Scope"/> instance or null when there is no scope active in this context.</returns>
        protected override Scope? GetCurrentScopeCore(Container container) =>
            GetScopeManager(container).CurrentScope;

        /// <inheritdoc />
        protected override void SetCurrentScopeCore(Scope scope) =>
            GetScopeManager(scope.Container!).SetCurrentScope(scope);

        private static ScopeManager GetScopeManager(Container container) =>
            container.ContainerScope.GetOrSetItem(ManagerKey, CreateManager);

        private static ScopeManager CreateManager(Container container, object key)
        {
            var threadLocal = new ThreadLocal<Scope?>();

            var manager = new ScopeManager(container, () => threadLocal.Value, s => threadLocal.Value = s);

            container.ContainerScope.RegisterForDisposal(threadLocal);

            return manager;
        }
    }
}