using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace NopBrasil.Plugin.Payments.PagSeguro.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Payments.EmailAdmin.PagSeguro")]
        public string PagSeguroEmail { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Token.PagSeguro")]
        public string PagSeguroToken { get; set; }
    }
}
