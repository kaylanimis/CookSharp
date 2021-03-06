﻿using System.Diagnostics;
using System.Windows;
using Castle.MicroKernel.Registration;
using Microsoft.Practices.Prism.Logging;
using PrismContrib.WindsorExtensions;

namespace CookSharp.Prism
{
    public class BootStrapper : WindsorBootstrapper
    {
        private readonly IWindsorInstaller[] installers;

        public BootStrapper(params IWindsorInstaller[] installers)
        {
            this.installers = installers;
        }

        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<Shell>();
        }

        protected override ILoggerFacade CreateLogger()
        {
            return new LoggingFacade();
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            Container.Install(installers);
        }

        protected override void InitializeShell()
        {
            Application.Current.MainWindow = Shell as Window;
            Debug.Assert(Application.Current.MainWindow != null, "Application.Current.MainWindow != null");
            Application.Current.MainWindow.Show();
        }
    }
}