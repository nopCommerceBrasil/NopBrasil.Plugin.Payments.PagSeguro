using System;
using System.Collections.Generic;
using Nop.Core.Plugins;
using Nop.Services.Payments;
using System.Web.Routing;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using System.Web;
using NopBrasil.Plugin.Payments.PagSeguro.Controllers;
using Nop.Core.Domain.Payments;
using NopBrasil.Plugin.Payments.PagSeguro.Services;
using Nop.Services.Localization;

namespace NopBrasil.Plugin.Payments.PagSeguro
{
    public class PagSeguroPaymentProcessor : BasePlugin, IPaymentMethod
    {
        private readonly HttpContextBase _httpContext;
        private readonly ILogger _logger;
        private readonly IPagSeguroService _pagSeguroService;
        private readonly ISettingService _settingService;
        private readonly PagSeguroPaymentSetting _pagSeguroSetting;

        public PagSeguroPaymentProcessor(ILogger logger, HttpContextBase httpContext, IPagSeguroService pagSeguroService, ISettingService settingService, PagSeguroPaymentSetting pagSeguroSetting)
        {
            this._httpContext = httpContext;
            this._logger = logger;
            this._pagSeguroService = pagSeguroService;
            this._settingService = settingService;
            this._pagSeguroSetting = pagSeguroSetting;
        }

        public override void Install()
        {
            this.AddOrUpdatePluginLocaleResource("NopBrasil.Plugins.Payments.PagSeguro.Fields.Redirection", "Você será redirecionado para a pagina do Uol PagSeguro");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.EmailAdmin.PagSeguro", "Email - PagSeguro");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Token.PagSeguro", "Token - PagSeguro");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MethodDescription.PagSeguro", "Descrição que será exibida no checkout");
            base.Install();
        }

        public override void Uninstall()
        {
            _settingService.DeleteSetting<PagSeguroPaymentSetting>();
            this.DeletePluginLocaleResource("NopBrasil.Plugins.Payments.PagSeguro.Fields.Redirection");
            this.DeletePluginLocaleResource("Plugins.Payments.EmailAdmin.PagSeguro");
            this.DeletePluginLocaleResource("Plugins.Payments.Token.PagSeguro");
            this.DeletePluginLocaleResource("Plugins.Payments.MethodDescription.PagSeguro");
            base.Uninstall();
        }

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var processPaymentResult = new ProcessPaymentResult();
            processPaymentResult.NewPaymentStatus = PaymentStatus.Pending;
            return processPaymentResult;
        }

        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            try
            {
                Uri uri = _pagSeguroService.CreatePayment(postProcessPaymentRequest);
                this._httpContext.Response.Redirect(uri.AbsoluteUri);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
            }
        }

        //permitir configurar o valor de acréscimo no pedido
        public decimal GetAdditionalHandlingFee(IList<Nop.Core.Domain.Orders.ShoppingCartItem> cart) => 0;

        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest) => new CapturePaymentResult();

        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest) => new RefundPaymentResult();

        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest) => new VoidPaymentResult();

        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest) => new ProcessPaymentResult();

        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest) => new CancelRecurringPaymentResult();

        public bool CanRePostProcessPayment(Nop.Core.Domain.Orders.Order order) => false;

        public void GetConfigurationRoute(out string actionName, out string controllerName, out System.Web.Routing.RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentPagSeguro";
            routeValues = new RouteValueDictionary()
            {
                { "Namespaces", "NopBrasil.Plugin.Payments.PagSeguro.Controllers" },
                { "area", null }
            };
        }

        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out System.Web.Routing.RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentPagSeguro";
            routeValues = new RouteValueDictionary()
            {
                { "Namespaces", "NopBrasil.Plugin.Payments.PagSeguro.Controllers" },
                { "area", null }
            };
        }

        public Type GetControllerType() => typeof(PaymentPagSeguroController);

        public bool SupportCapture => false;

        public bool SupportPartiallyRefund => false;

        public bool SupportRefund => false;

        public bool SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public bool HidePaymentMethod(IList<Nop.Core.Domain.Orders.ShoppingCartItem> cart) => false;

        public bool SkipPaymentInfo => false;

        public string PaymentMethodDescription => _pagSeguroSetting.PaymentMethodDescription;
    }
}
