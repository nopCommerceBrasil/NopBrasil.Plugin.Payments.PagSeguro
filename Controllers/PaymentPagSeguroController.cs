using Nop.Services.Configuration;
using Nop.Web.Framework.Controllers;
using NopBrasil.Plugin.Payments.PagSeguro.Models;
using Nop.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Services.Security;
using Nop.Services.Messages;
using Nop.Services.Localization;

namespace NopBrasil.Plugin.Payments.PagSeguro.Controllers
{
    [Area(AreaNames.Admin)]
    public class PaymentPagSeguroController : BasePaymentController
    {
        private readonly ISettingService _settingService;
        private readonly PagSeguroPaymentSetting _pagSeguroPaymentSetting;
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;

        public PaymentPagSeguroController(ISettingService settingService, PagSeguroPaymentSetting pagSeguroPaymentSetting, IPermissionService permissionService, INotificationService notificationService, ILocalizationService localizationService)
        {
            this._settingService = settingService;
            this._pagSeguroPaymentSetting = pagSeguroPaymentSetting;
            this._permissionService = permissionService;
            this._notificationService = notificationService;
            this._localizationService = localizationService;
        }

        [AuthorizeAdmin]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var model = new ConfigurationModel()
            {
                PagSeguroToken = _pagSeguroPaymentSetting.PagSeguroToken,
                PagSeguroEmail = _pagSeguroPaymentSetting.PagSeguroEmail,
                PaymentMethodDescription = _pagSeguroPaymentSetting.PaymentMethodDescription
            };
            return View(@"~/Plugins/Payments.PagSeguro/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            _pagSeguroPaymentSetting.PagSeguroToken = model.PagSeguroToken;
            _pagSeguroPaymentSetting.PagSeguroEmail = model.PagSeguroEmail;
            _pagSeguroPaymentSetting.PaymentMethodDescription = model.PaymentMethodDescription;
            _settingService.SaveSetting(_pagSeguroPaymentSetting);
            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            return View(@"~/Plugins/Payments.PagSeguro/Views/Configure.cshtml", model);
        }
    }
}
