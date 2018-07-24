using Nop.Core;
using Nop.Services.Configuration;
using Nop.Web.Framework.Controllers;
using NopBrasil.Plugin.Payments.PagSeguro.Services;
using NopBrasil.Plugin.Payments.PagSeguro.Models;
using Nop.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Services.Security;

namespace NopBrasil.Plugin.Payments.PagSeguro.Controllers
{
    public class PaymentPagSeguroController : BasePaymentController
    {
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IPagSeguroService _pagSeguroService;
        private readonly PagSeguroPaymentSetting _pagSeguroPaymentSetting;
        private readonly IPermissionService _permissionService;

        public PaymentPagSeguroController(ISettingService settingService, IWebHelper webHelper, IPagSeguroService pagSeguroService, PagSeguroPaymentSetting pagSeguroPaymentSetting,
            IPermissionService permissionService)
        {
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._pagSeguroService = pagSeguroService;
            this._pagSeguroPaymentSetting = pagSeguroPaymentSetting;
            this._permissionService = permissionService;
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            var model = new ConfigurationModel()
            {
                PagSeguroToken = _pagSeguroPaymentSetting.PagSeguroToken,
                PagSeguroEmail = _pagSeguroPaymentSetting.PagSeguroEmail,
                PaymentMethodDescription = _pagSeguroPaymentSetting.PaymentMethodDescription
            };
            return View(@"~/Plugins/Payments.PagSeguro/Views/PaymentPagSeguro/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [Area(AreaNames.Admin)]
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

            return View(@"~/Plugins/Payments.PagSeguro/Views/PaymentPagSeguro/Configure.cshtml", model);
        }
    }
}
