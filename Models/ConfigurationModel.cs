using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;

namespace NopBrasil.Plugin.Payments.PagSeguro.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Payments.EmailAdmin.PagSeguro")]
        public string PagSeguroEmail { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Token.PagSeguro")]
        public string PagSeguroToken { get; set; }

        [NopResourceDisplayName("Plugins.Payments.MethodDescription.PagSeguro")]
        public string PaymentMethodDescription { get; set; }
    }
}
