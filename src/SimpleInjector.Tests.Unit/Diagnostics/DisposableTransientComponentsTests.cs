﻿namespace SimpleInjector.Diagnostics.Tests.Unit
{
    using System.Linq;
    using Lifestyles;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class RegistrationExtensions
    {
        public static void SuppressDiagnosticWarning(this Registration registration, DiagnosticType type)
        {
            registration.SuppressDiagnosticWarning(type, "Some random justification that we don't care about.");
        }
    }

    [TestClass]
    public class DisposableTransientComponentsTests
    {
        [TestMethod]
        public void Analyze_TransientRegistrationForDisposableComponent_Warning()
        {
            // Arrange
            var container = new Container();

            container.Register<IPlugin, DisposablePlugin>();

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var results = Analyzer.Analyze(container).OfType<DisposableTransientComponentDiagnosticResult>()
                .ToArray();

            // Assert
            Assert.AreEqual(1, results.Length, Actual(results));
            Assert.AreEqual(
                "DisposablePlugin is registered for IPlugin as transient, but implements IDisposable.",
                results.Single().Description,
                Actual(results));
        }

        [TestMethod]
        public void Analyze_TransientRegistrationForConcreteDisposableComponent_Warning()
        {
            // Arrange
            var container = new Container();

            container.Register<DisposablePlugin>();

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var results = Analyzer.Analyze(container).OfType<DisposableTransientComponentDiagnosticResult>()
                .ToArray();

            // Assert
            Assert.AreEqual(1, results.Length, Actual(results));
            Assert.AreEqual(
                "DisposablePlugin is registered as transient, but implements IDisposable.",
                results.Single().Description,
                Actual(results));
        }

        [TestMethod]
        public void Analyze_TransientRegistrationForAsyncDisposableComponent_Warning()
        {
            // Arrange
            var container = new Container();

            container.Register<IPlugin, AsyncDisposablePlugin>();

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var results = Analyzer.Analyze(container).OfType<DisposableTransientComponentDiagnosticResult>()
                .ToArray();

            // Assert
            Assert.AreEqual(1, results.Length, Actual(results));
            Assert.AreEqual(
                "AsyncDisposablePlugin is registered for IPlugin as transient, but implements IAsyncDisposable.",
                results.Single().Description,
                Actual(results));
        }

        [TestMethod]
        public void Analyze_TransientRegistrationForComponentImplementingBothIDisposableAndIAsyncDisposable_Warning()
        {
            // Arrange
            var container = new Container();

            container.Register<IPlugin, DisposableAsyncDisposablePlugin>();

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var results = Analyzer.Analyze(container).OfType<DisposableTransientComponentDiagnosticResult>()
                .ToArray();

            // Assert
            Assert.AreEqual(1, results.Length, Actual(results));
            Assert.AreEqual(
                "DisposableAsyncDisposablePlugin is registered for IPlugin as transient, but implements IDisposable.",
                results.Single().Description,
                Actual(results));
        }

        [TestMethod]
        public void Analyze_TransientRegistrationForDisposableComponent_ReturnsSeverityWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<IPlugin, DisposablePlugin>();

            container.Verify(VerificationOption.VerifyOnly);

            // Act
            var result = Analyzer.Analyze(container).OfType<DisposableTransientComponentDiagnosticResult>().First();

            // Assert
            Assert.AreEqual(DiagnosticSeverity.Warning, result.Severity);
        }

        [TestMethod]
        public void Analyze_TransientRegistrationForDisposableComponentWithSuppressDiagnosticWarning_NoWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<IPlugin, DisposablePlugin>();

            Registration registration = container.GetRegistration(typeof(IPlugin)).Registration;

            registration.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent);

            container.Verify();

            // Act
            var results = Analyzer.Analyze(container).OfType<DisposableTransientComponentDiagnosticResult>()
                .ToArray();

            // Assert
            Assert.AreEqual(0, results.Length, Actual(results));
        }

        [TestMethod]
        public void Analyze_SingletonRegistrationForDisposableComponent_NoWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<IPlugin, DisposablePlugin>(Lifestyle.Singleton);

            container.Verify();

            // Act
            var results = Analyzer.Analyze(container).OfType<DisposableTransientComponentDiagnosticResult>()
                .ToArray();

            // Assert
            Assert.AreEqual(0, results.Length, Actual(results));
        }

        [TestMethod]
        public void Analyze_ScopedRegistrationForDisposableComponent_NoWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<IPlugin, DisposablePlugin>(new ThreadScopedLifestyle());

            container.Verify();

            // Act
            var results = Analyzer.Analyze(container).OfType<DisposableTransientComponentDiagnosticResult>()
                .ToArray();

            // Assert
            Assert.AreEqual(0, results.Length, Actual(results));
        }

        [TestMethod]
        public void Analyze_TransientRegistrationForComponentThatsNotDisposable_NoWarning()
        {
            // Arrange
            var container = new Container();

            container.Register<IPlugin, SomePluginImpl>();

            container.Verify();

            // Act
            var results = Analyzer.Analyze(container).OfType<DisposableTransientComponentDiagnosticResult>()
                .ToArray();

            // Assert
            Assert.AreEqual(0, results.Length, Actual(results));
        }

        private static string Actual(DisposableTransientComponentDiagnosticResult[] results) =>
            "actual: " + string.Join(" - ", results.Select(r => r.Description));
    }
}