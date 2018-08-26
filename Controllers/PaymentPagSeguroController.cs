using Nop.Services.Configuration;
using Nop.Web.Framework.Controllers;
using NopBrasil.Plugin.Payments.PagSeguro.Models;
using Nop.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Services.Security;

namespace NopBrasil.Plugin.Payments.PagSeguro.Controllers
{
    [Area(AreaNames.Admin)]
    public class PaymentPagSeguroController : BasePaymentController
    {
        private readonly ISettingService _settingService;
        private readonly PagSeguroPaymentSetting _pagSeguroPaymentSetting;
        private readonly IPermissionService _permissionService;

        public PaymentPagSeguroController(ISettingService settingService, PagSeguroPaymentSetting pagSeguroPaymentSetting, IPermissionService permissionService)
        {
            this._settingService = settingService;
            this._pagSeguroPaymentSetting = pagSeguroPaymentSetting;
            this._permissionService = permissionService;
        }

        [AuthorizeAdmin]
        public IActionResult Configure()
        {
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

            return View(@"~/Plugins/Payments.PagSeguro/Views/Configure.cshtml", model);
        }
    }
}
