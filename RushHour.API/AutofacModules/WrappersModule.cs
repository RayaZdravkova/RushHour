using Autofac;
using System.Reflection;

namespace RushHour.API.AutofacModules
{
    public class WrappersModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var assembly = Assembly.Load("RushHour.API");

            builder.RegisterAssemblyTypes(assembly)
                    .Where(t => t.Name.EndsWith("Wrapper"))
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();

        }
    }
}
