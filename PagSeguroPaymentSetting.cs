using Nop.Core.Configuration;

namespace NopBrasil.Plugin.Payments.PagSeguro
{
    public class PagSeguroPaymentSetting : ISettings
    {
        public string PagSeguroEmail { get; set; }
        public string PagSeguroToken { get; set; }
    }
}
