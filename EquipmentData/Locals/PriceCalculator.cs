using System;
using System.Collections.Generic;
using EquipmentData.Globals;

namespace EquipmentData.Locals
{
    public class PriceCalculator
    {
        protected const int OneTimeFee = 100;
        protected const int PremiumDailyFee = 60;
        protected const int RegularDailyFee = 40;

        public static int GetEquipmentPrice(EquipmentType type, int days)
        {
            int result = 0;

            if (days == 0)
            {
                return result;
            }

            switch (type)
            {
                case EquipmentType.Heavy:
                    result = OneTimeFee + PremiumDailyFee * days;
                    break;
                case EquipmentType.Regular:
                    result = OneTimeFee + (days <= 2 ? PremiumDailyFee * days : PremiumDailyFee * 2) + (days > 2 ? (days - 2) * RegularDailyFee : 0);
                    break;
                case EquipmentType.Specialized:
                    result = (days <= 3 ? PremiumDailyFee * days : PremiumDailyFee * 3) + (days > 3 ? (days - 3) * RegularDailyFee : 0);
                    break;
            }

            return result;
        }

        public static int GetLoyaltyPoints(EquipmentType type)
        {
            return type == EquipmentType.Heavy ? 2 : 1;
        }

        public static Invoice GetInvoice(BasketOrderList basketOrderList)
        {
            var result = new Invoice();
            var rows = new List<InvoiceRow>();
            DateTime date = DateTime.MinValue;

            foreach (EquipmentOrder order in basketOrderList.Orders)
            {
                var orderPrice = GetEquipmentPrice(order.Type, order.Days);
                var orderBonuses = GetLoyaltyPoints(order.Type);

                result.Price += orderPrice;
                result.Bonuses += orderBonuses;
                if (date == DateTime.MinValue)
                {
                    date = order.OrderTime;
                }

                var row = new InvoiceRow() { Name = order.Name, Price = orderPrice };
                rows.Add(row);
            }

            result.Rows = rows;
            if (date != DateTime.MinValue)
            {
                result.Title = date.ToLongDateString() + " " + date.ToLongTimeString();
            }

            return result;
        }
    }
}
