using Nop.Services.Configuration;
using Nop.Services.Payments;
using System;
using Uol.PagSeguro;

namespace NopBrasil.Plugin.Payments.PagSeguro.Services
{
    public class PagSeguroService : IPagSeguroService
    {
        private readonly ISettingService _settingService;

        public PagSeguroService(ISettingService settingService)
        {
            this._settingService = settingService;
        }

        public Uri CreatePayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            // Seta as credenciais
            PagSeguroPaymentSetting pagSeguroPaymentSetting = _settingService.LoadSetting<PagSeguroPaymentSetting>();
            AccountCredentials credentials = new AccountCredentials(@pagSeguroPaymentSetting.PagSeguroEmail, @pagSeguroPaymentSetting.PagSeguroToken);

            PaymentRequest payment = new PaymentRequest();
            payment.Currency = postProcessPaymentRequest.Order.CustomerCurrencyCode;
            payment.Reference = postProcessPaymentRequest.Order.Id.ToString();

            LoadingItems(postProcessPaymentRequest, payment);
            LoadingShipping(postProcessPaymentRequest, payment);
            LoadingSender(postProcessPaymentRequest, payment);

            return Uol.PagSeguro.PaymentService.Register(credentials, payment);
        }

        private void LoadingSender(PostProcessPaymentRequest postProcessPaymentRequest, PaymentRequest payment)
        {
            payment.Sender = new Sender();
            payment.Sender.Email = postProcessPaymentRequest.Order.Customer.Email;
            payment.Sender.Name = postProcessPaymentRequest.Order.BillingAddress.FirstName + " " + postProcessPaymentRequest.Order.BillingAddress.LastName;
        }

        private void LoadingShipping(PostProcessPaymentRequest postProcessPaymentRequest, PaymentRequest payment)
        {
            payment.Shipping = new Shipping();
            payment.Shipping.ShippingType = ShippingType.NotSpecified;
            Address adress = new Address();
            adress.Complement = string.Empty;
            adress.District = string.Empty;
            adress.Number = string.Empty;
            if (postProcessPaymentRequest.Order.ShippingAddress != null)
            {
                adress.City = postProcessPaymentRequest.Order.ShippingAddress.City;
                adress.Country = postProcessPaymentRequest.Order.ShippingAddress.Country.Name;
                adress.PostalCode = postProcessPaymentRequest.Order.ShippingAddress.ZipPostalCode;
                adress.State = postProcessPaymentRequest.Order.ShippingAddress.StateProvince.Name;
                adress.Street = postProcessPaymentRequest.Order.ShippingAddress.Address1;
            }
        }

        private void LoadingItems(PostProcessPaymentRequest postProcessPaymentRequest, PaymentRequest payment)
        {
            foreach (var product in postProcessPaymentRequest.Order.OrderItems)
            {
                Item item = new Item();
                item.Amount = Math.Round(product.UnitPriceInclTax, 2);
                item.Description = product.Product.Name;
                item.Id = product.Id.ToString();
                item.Quantity = product.Quantity;
                if (product.ItemWeight.HasValue)
                    item.Weight = Convert.ToInt64(product.ItemWeight);
                payment.Items.Add(item);
            }
        }
    }
}

