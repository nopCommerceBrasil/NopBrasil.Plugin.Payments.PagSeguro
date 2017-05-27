using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Payments;
using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using NopBrasil.Plugin.Payments.PagSeguro.Services;
using NopBrasil.Plugin.Payments.PagSeguro.Models;

namespace NopBrasil.Plugin.Payments.PagSeguro.Controllers
{
    public class PaymentPagSeguroController : BasePaymentController
    {
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IPagSeguroService _pagSeguroService;
        private readonly PagSeguroPaymentSetting _pagSeguroPaymentSetting;

        public PaymentPagSeguroController(ISettingService settingService, IWebHelper webHelper, IPagSeguroService pagSeguroService, PagSeguroPaymentSetting pagSeguroPaymentSetting)
        {
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._pagSeguroService = pagSeguroService;
            this._pagSeguroPaymentSetting = pagSeguroPaymentSetting;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel() { PagSeguroToken = _pagSeguroPaymentSetting.PagSeguroToken, PagSeguroEmail = _pagSeguroPaymentSetting.PagSeguroEmail };
            return View(@"~/Plugins/Payments.PagSeguro/Views/PaymentPagSeguro/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            _pagSeguroPaymentSetting.PagSeguroToken = model.PagSeguroToken;
            _pagSeguroPaymentSetting.PagSeguroEmail = model.PagSeguroEmail;
            _settingService.SaveSetting(_pagSeguroPaymentSetting);

            return View(@"~/Plugins/Payments.PagSeguro/Views/PaymentPagSeguro/Configure.cshtml", model);
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo() => View("~/Plugins/Payments.PagSeguro/Views/PaymentPagSeguro/PaymentInfo.cshtml");

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form) => new List<String>();

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form) => new ProcessPaymentRequest();
    }
}
