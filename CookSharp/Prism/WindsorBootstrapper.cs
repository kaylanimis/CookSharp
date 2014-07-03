﻿using System;
using System.Globalization;
using System.Windows;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Castle.Windsor.Configuration.Interpreters.XmlProcessor;
using Castle.Windsor.Installer;
using CommonServiceLocator.WindsorAdapter;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Regions.Behaviors;
using Microsoft.Practices.ServiceLocation;

namespace CookSharp.Prism
{
    /// <summary>
    /// Base class that provides a basic bootstrapping sequence that
    /// registers most of the Composite Application Library assets
    /// in a <see cref="IWindsorContainer"/>.
    /// </summary>
    /// <remarks>
    /// This class must be overridden to provide application specific configuration.
    /// 
    /// This was taken from: https://github.com/bszafko/PrismContrib.WindsorExtensions
    /// TODO: Point to the NuGet version when it's updated.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public abstract class WindsorBootstrapper : Bootstrapper
    {
        private bool useDefaultConfiguration = true;

        /// <summary>
        /// Gets the default <see cref="IWindsorContainer"/> for the application.
        /// </summary>
        /// <value>The default <see cref="IWindsorContainer"/> instance.</value>
        public IWindsorContainer Container { get; protected set; }


        /// <summary>
        /// Run the bootstrapper process.
        /// </summary>
        /// <param name="runWithDefaultConfiguration">If <see langword="true"/>, registers default Composite Application Library services in the container. This is the default behavior.</param>
        public override void Run(bool runWithDefaultConfiguration)
        {
            this.useDefaultConfiguration = runWithDefaultConfiguration;

            this.Logger = this.CreateLogger();
            if (this.Logger == null)
            {
                throw new InvalidOperationException(Properties.Resources.NullLoggerFacadeException);
            }

            this.Logger.Log(Properties.Resources.LoggerCreatedSuccessfully, Category.Debug, Priority.Low);

            this.Logger.Log(Properties.Resources.CreatingModuleCatalog, Category.Debug, Priority.Low);
            this.ModuleCatalog = this.CreateModuleCatalog();
            if (this.ModuleCatalog == null)
            {
                throw new InvalidOperationException(Properties.Resources.NullModuleCatalogException);
            }

            this.Logger.Log(Properties.Resources.ConfiguringModuleCatalog, Category.Debug, Priority.Low);
            this.ConfigureModuleCatalog();

            this.Logger.Log(Properties.Resources.CreatingWindsorContainer, Category.Debug, Priority.Low);
            this.Container = this.CreateContainer();
            if (this.Container == null)
            {
                throw new InvalidOperationException(Properties.Resources.NullUnityContainerException);
            }

            this.Logger.Log(Properties.Resources.ConfiguringUnityContainer, Category.Debug, Priority.Low);
            this.ConfigureContainer();

            this.Logger.Log(Properties.Resources.ConfiguringServiceLocatorSingleton, Category.Debug, Priority.Low);
            this.ConfigureServiceLocator();

            this.Logger.Log(Properties.Resources.ConfiguringRegionAdapters, Category.Debug, Priority.Low);
            this.ConfigureRegionAdapterMappings();

            this.Logger.Log(Properties.Resources.ConfiguringDefaultRegionBehaviors, Category.Debug, Priority.Low);
            this.ConfigureDefaultRegionBehaviors();

            this.Logger.Log(Properties.Resources.RegisteringFrameworkExceptionTypes, Category.Debug, Priority.Low);
            this.RegisterFrameworkExceptionTypes();

            this.Logger.Log(Properties.Resources.CreatingShell, Category.Debug, Priority.Low);
            this.Shell = this.CreateShell();
            if (this.Shell != null)
            {
                this.Logger.Log(Properties.Resources.SettingTheRegionManager, Category.Debug, Priority.Low);
                RegionManager.SetRegionManager(this.Shell, this.Container.Resolve<IRegionManager>());

                this.Logger.Log(Properties.Resources.UpdatingRegions, Category.Debug, Priority.Low);
                RegionManager.UpdateRegions();

                this.Logger.Log(Properties.Resources.InitializingShell, Category.Debug, Priority.Low);
                this.InitializeShell();
            }

            this.Logger.Log(Properties.Resources.InitializingModules, Category.Debug, Priority.Low);
            this.InitializeModules();

            this.Logger.Log(Properties.Resources.BootstrapperSequenceCompleted, Category.Debug, Priority.Low);
        }

        /// <summary>
        /// Configures the LocatorProvider for the <see cref="ServiceLocator" />.
        /// </summary>
        protected override void ConfigureServiceLocator()
        {
            ServiceLocator.SetLocatorProvider(() => this.Container.Resolve<IServiceLocator>());
        }

        /// <summary>
        /// Registers in the <see cref="IWindsorContainer"/> the <see cref="Type"/> of the Exceptions
        /// that are not considered root exceptions by the <see cref="ExceptionExtensions"/>.
        /// </summary>
        protected override void RegisterFrameworkExceptionTypes()
        {
            base.RegisterFrameworkExceptionTypes();

            ExceptionExtensions.RegisterFrameworkExceptionType(
                typeof(ConfigurationProcessingException));

            ExceptionExtensions.RegisterFrameworkExceptionType(
                typeof(Castle.MicroKernel.ComponentNotFoundException));
        }

        /// <summary>
        /// Configures the <see cref="IWindsorContainer"/>. May be overwritten in a derived class to add specific
        /// type mappings required by the application.
        /// </summary>
        protected virtual void ConfigureContainer()
        {
            Container.Register(Component.For<ILoggerFacade>().Instance(Logger));

            Container.Register(Component.For<IModuleCatalog>().Instance(ModuleCatalog));

            if (useDefaultConfiguration)
            {
                Container.Register(Component.For<IWindsorContainer>().Instance(Container));
                RegisterTypeIfMissing(typeof(IServiceLocator), typeof(WindsorServiceLocator), true);
                RegisterTypeIfMissing(typeof(IModuleInitializer), typeof(ModuleInitializer), true);
                RegisterTypeIfMissing(typeof(IModuleManager), typeof(ModuleManager), true);
                RegisterTypeIfMissing(typeof(RegionAdapterMappings), typeof(RegionAdapterMappings), true);
                RegisterTypeIfMissing(typeof(IRegionManager), typeof(RegionManager), true);
                RegisterTypeIfMissing(typeof(IEventAggregator), typeof(EventAggregator), true);
                RegisterTypeIfMissing(typeof(IRegionViewRegistry), typeof(RegionViewRegistry), true);
                RegisterTypeIfMissing(typeof(IRegionBehaviorFactory), typeof(RegionBehaviorFactory), true);
                RegisterTypeIfMissing(typeof(IRegionNavigationJournalEntry), typeof(RegionNavigationJournalEntry), false);
                RegisterTypeIfMissing(typeof(IRegionNavigationJournal), typeof(RegionNavigationJournal), false);
                RegisterTypeIfMissing(typeof(IRegionNavigationService), typeof(RegionNavigationService), false);
                RegisterTypeIfMissing(typeof(IRegionNavigationContentLoader), typeof(RegionNavigationContentLoader), true);
                RegisterTypeIfMissing(typeof(DelayedRegionCreationBehavior), typeof(DelayedRegionCreationBehavior), false);

                // register region adapters
                Container.Register(Classes.FromAssemblyContaining<IRegionAdapter>().BasedOn<IRegionAdapter>().LifestyleTransient());

                // register region behaviors
                Container.Register(Classes.FromAssemblyContaining<IRegionBehavior>().BasedOn<IRegionBehavior>().LifestyleTransient());
            }
        }

        /// <summary>
        /// Initializes the modules. May be overwritten in a derived class to use a custom Modules Catalog
        /// </summary>
        protected override void InitializeModules()
        {
            IModuleManager manager;

            try
            {
                manager = this.Container.Resolve<IModuleManager>();
            }
            catch (ComponentNotFoundException ex)
            {
                if (ex.Message.Contains("IModuleCatalog"))
                {
                    throw new InvalidOperationException(Properties.Resources.NullModuleCatalogException);
                }

                throw;
            }

            manager.Run();
        }

        /// <summary>
        /// Creates the <see cref="IWindsorContainer"/> that will be used as the default container.
        /// </summary>
        /// <returns>A new instance of <see cref="IWindsorContainer"/>.</returns>
        protected virtual IWindsorContainer CreateContainer()
        {
            return new WindsorContainer();
        }

        /// <summary>
        /// Registers a type in the container only if that type was not already registered.
        /// </summary>
        /// <param name="fromType">The interface type to register.</param>
        /// <param name="toType">The type implementing the interface.</param>
        /// <param name="registerAsSingleton">Registers the type as a singleton.</param>
        protected void RegisterTypeIfMissing(Type fromType, Type toType, bool registerAsSingleton)
        {
            if (fromType == null)
            {
                throw new ArgumentNullException("fromType");
            }
            if (toType == null)
            {
                throw new ArgumentNullException("toType");
            }
            if (Container.Kernel.HasComponent(fromType))
            {
                Logger.Log(
                    String.Format(CultureInfo.CurrentCulture,
                                  Properties.Resources.TypeMappingAlreadyRegistered,
                                  fromType.Name), Category.Debug, Priority.Low);
            }
            else
            {
                if (registerAsSingleton)
                {
                    Container.Register(Castle.MicroKernel.Registration.Component.For(fromType)
                        .ImplementedBy(toType)
                        .LifeStyle.Singleton);
                }
                else
                {
                    Container.Register(Castle.MicroKernel.Registration.Component.For(fromType)
                        .ImplementedBy(toType)
                        .LifeStyle.Transient);
                }
            }
        }
    }
}