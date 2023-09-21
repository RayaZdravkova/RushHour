using Autofac;
using System.Reflection;

namespace RushHour.API.AutofacModules
{
    public class ExtensionsModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var assembly = Assembly.Load("RushHour.DependencyInjection");

            builder.RegisterAssemblyTypes(assembly)
                    .Where(t => t.Name.EndsWith("Extension"))
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();

        }
    }
}
