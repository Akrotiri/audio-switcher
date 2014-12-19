﻿// -----------------------------------------------------------------------
// Copyright (c) David Kean.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using AudioSwitcher.ApplicationModel;

namespace AudioSwitcher
{
    internal class Program
    {
        public static void Main(string[] parameters)
        {
            var catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());

            using (CompositionContainer container = new CompositionContainer(catalog))
            {
                IApplication application = container.GetExportedValue<IApplication>();
                application.Run();
            }
        }
    }
}