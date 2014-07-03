﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Castle.Windsor;

namespace CookSharp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IWindsorContainer container;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            container = new WindsorContainer();
            container.Install(new Installer());

            var shell = container.Resolve<IShell>();
            shell.Run();
            container.Release(shell);
        }
    }
}
