using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using NopBrasil.Plugin.Payments.PagSeguro.Controllers;
using NopBrasil.Plugin.Payments.PagSeguro.Services;

namespace NopBrasil.Plugin.Payments.PagSeguro.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig nopConfig)
        {
            builder.RegisterType<PaymentPagSeguroController>().AsSelf();
            builder.RegisterType<PagSeguroService>().As<IPagSeguroService>().InstancePerDependency();
            //.WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static"))
        }

        public int Order => 2;
    }
}
