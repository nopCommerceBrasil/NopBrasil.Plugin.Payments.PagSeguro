using System;
using System.Collections.Generic;
using Nop.Core.Plugins;
using Nop.Services.Payments;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using NopBrasil.Plugin.Payments.PagSeguro.Controllers;
using Nop.Core.Domain.Payments;
using NopBrasil.Plugin.Payments.PagSeguro.Services;
using Nop.Services.Localization;
using NopBrasil.Plugin.Payments.PagSeguro.Task;
using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Orders;

namespace NopBrasil.Plugin.Payments.PagSeguro
{
    public class PagSeguroPaymentProcessor : BasePlugin, IPaymentMethod
    {
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPagSeguroService _pagSeguroService;
        private readonly ISettingService _settingService;
        private readonly PagSeguroPaymentSetting _pagSeguroSetting;
        private readonly CheckPaymentTask _checkPaymentTask;

        public PagSeguroPaymentProcessor(ILogger logger, IHttpContextAccessor httpContextAccessor, IPagSeguroService pagSeguroService, ISettingService settingService, PagSeguroPaymentSetting pagSeguroSetting, CheckPaymentTask checkPaymentTask)
        {
            this._logger = logger;
            this._httpContextAccessor = httpContextAccessor;
            this._pagSeguroService = pagSeguroService;
            this._settingService = settingService;
            this._pagSeguroSetting = pagSeguroSetting;
            this._checkPaymentTask = checkPaymentTask;
        }

        public override void Install()
        {
            this.AddOrUpdatePluginLocaleResource("NopBrasil.Plugins.Payments.PagSeguro.Fields.Redirection", "Você será redirecionado para a pagina do Uol PagSeguro");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.EmailAdmin.PagSeguro", "Email - PagSeguro");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Token.PagSeguro", "Token - PagSeguro");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.MethodDescription.PagSeguro", "Descrição que será exibida no checkout");
            _checkPaymentTask.InstallTask();
            base.Install();
        }

        public override void Uninstall()
        {
            _checkPaymentTask.UninstallTask();
            _settingService.DeleteSetting<PagSeguroPaymentSetting>();
            this.DeletePluginLocaleResource("NopBrasil.Plugins.Payments.PagSeguro.Fields.Redirection");
            this.DeletePluginLocaleResource("Plugins.Payments.EmailAdmin.PagSeguro");
            this.DeletePluginLocaleResource("Plugins.Payments.Token.PagSeguro");
            this.DeletePluginLocaleResource("Plugins.Payments.MethodDescription.PagSeguro");
            base.Uninstall();
        }

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var processPaymentResult = new ProcessPaymentResult()
            {
                NewPaymentStatus = PaymentStatus.Pending
            };
            return processPaymentResult;
        }

        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            try
            {
                Uri uri = _pagSeguroService.CreatePayment(postProcessPaymentRequest);
                this._httpContextAccessor.HttpContext.Response.Redirect(uri.AbsoluteUri);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
            }
        }

        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart) => 0;

        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest) => new CapturePaymentResult();

        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest) => new RefundPaymentResult();

        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest) => new VoidPaymentResult();

        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest) => new ProcessPaymentResult();

        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest) => new CancelRecurringPaymentResult();

        public bool CanRePostProcessPayment(Order order) => false;

        public Type GetControllerType() => typeof(PaymentPagSeguroController);

        public bool SupportCapture => false;

        public bool SupportPartiallyRefund => false;

        public bool SupportRefund => false;

        public bool SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public bool HidePaymentMethod(IList<Nop.Core.Domain.Orders.ShoppingCartItem> cart) => false;

        public IList<string> ValidatePaymentForm(IFormCollection form) => new List<string>();

        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form) => new ProcessPaymentRequest();

        public void GetPublicViewComponent(out string viewComponentName) => viewComponentName = "PaymentPagSeguro";

        public bool SkipPaymentInfo => false;

        public string PaymentMethodDescription => _pagSeguroSetting.PaymentMethodDescription;
    }
}
