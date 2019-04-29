using Microsoft.VisualStudio.TestTools.UnitTesting;
using EquipmentData.Globals;
using EquipmentData.Locals;
using Shouldly;
using System.Linq;
using System;

namespace EquipmentDataTest
{
    [TestClass]
    public class EquipmentDataLocalsTest
    {
        [TestMethod]
        public void BasketOrderList_Should_Give_Correct_Results()
        {
            BasketOrderList list = new BasketOrderList();
            list.Orders.ShouldBeEmpty();

            var order = new EquipmentOrder();

            list.Add(order);
            list.Orders.ShouldContain(order);

            var order2 = new EquipmentOrder();
            list.Add(order2);
            list.Orders.ShouldContain(order2);

            var first = list.Orders.First();
            first.ShouldBe(order);

            var last = list.Orders.Last();
            last.ShouldBe(order2);
        }

        [TestMethod]
        public void PriceCalculator_EquipmentPrice_Should_Be_Correct()
        {
            var type = EquipmentType.Heavy;

            var price = PriceCalculator.GetEquipmentPrice(type, 0);
            price.ShouldBe(0);

            for (int i = 1; i < 10; i++)
            {
                price = PriceCalculator.GetEquipmentPrice(type, i);
                price.ShouldBe(100 + i * 60);
            }

            type = EquipmentType.Regular;

            price = PriceCalculator.GetEquipmentPrice(type, 0);
            price.ShouldBe(0);

            price = PriceCalculator.GetEquipmentPrice(type, 1);
            price.ShouldBe(100 + 60);

            price = PriceCalculator.GetEquipmentPrice(type, 2);
            price.ShouldBe(100 + 120);

            price = PriceCalculator.GetEquipmentPrice(type, 3);
            price.ShouldBe(100 + 120 + 40);

            price = PriceCalculator.GetEquipmentPrice(type, 4);
            price.ShouldBe(100 + 120 + 80);

            type = EquipmentType.Specialized;

            price = PriceCalculator.GetEquipmentPrice(type, 0);
            price.ShouldBe(0);

            price = PriceCalculator.GetEquipmentPrice(type, 1);
            price.ShouldBe(60);

            price = PriceCalculator.GetEquipmentPrice(type, 2);
            price.ShouldBe(120);

            price = PriceCalculator.GetEquipmentPrice(type, 3);
            price.ShouldBe(180);

            price = PriceCalculator.GetEquipmentPrice(type, 4);
            price.ShouldBe(180 + 40);

            price = PriceCalculator.GetEquipmentPrice(type, 5);
            price.ShouldBe(180 + 80);
        }

        [TestMethod]
        public void PriceCalculator_GetLoyaltyPoints_Should_Be_Correct()
        {
            var points = PriceCalculator.GetLoyaltyPoints(EquipmentType.Heavy);
            points.ShouldBe(2);

            points = PriceCalculator.GetLoyaltyPoints(EquipmentType.Regular);
            points.ShouldBe(1);

            points = PriceCalculator.GetLoyaltyPoints(EquipmentType.Specialized);
            points.ShouldBe(1);
        }

        [TestMethod]
        public void PriceCalculator_GetInvoice_Should_Be_Correct()
        {
            var orderList = new BasketOrderList();
            var invoice = PriceCalculator.GetInvoice(orderList);
            invoice.Rows.Count.ShouldBe(0);
            invoice.Price.ShouldBe(0);
            invoice.Bonuses.ShouldBe(0);

            var order = new EquipmentOrder();
            orderList.Add(order);
            invoice = PriceCalculator.GetInvoice(orderList);
            invoice.Rows.Count.ShouldBe(1);

            orderList = new BasketOrderList();
            order = new EquipmentOrder() { Days = 1, Name = "ABC", OrderTime = DateTime.Now, Type = EquipmentType.Heavy };
            orderList.Add(order);
            invoice = PriceCalculator.GetInvoice(orderList);
            invoice.Rows.Count.ShouldBe(1);
            invoice.Rows.First().Name.ShouldBe("ABC");
            invoice.Rows.First().Price.ShouldBe(160);
            invoice.Price.ShouldBe(160);
            invoice.Bonuses.ShouldBe(2);

            order = new EquipmentOrder() { Days = 2, Name = "BCE", OrderTime = DateTime.Now, Type = EquipmentType.Regular };
            orderList.Add(order);
            invoice = PriceCalculator.GetInvoice(orderList);
            invoice.Rows.Count.ShouldBe(2);
            invoice.Rows.Last().Name.ShouldBe("BCE");
            invoice.Rows.Last().Price.ShouldBe(220);
            invoice.Price.ShouldBe(380);
            invoice.Bonuses.ShouldBe(3);

            order = new EquipmentOrder() { Days = 4, Name = "DEF", OrderTime = DateTime.Now, Type = EquipmentType.Specialized };
            orderList.Add(order);
            invoice = PriceCalculator.GetInvoice(orderList);
            invoice.Rows.Count.ShouldBe(3);
            invoice.Rows.Last().Name.ShouldBe("DEF");
            invoice.Rows.Last().Price.ShouldBe(220);
            invoice.Price.ShouldBe(600);
            invoice.Bonuses.ShouldBe(4);
        }
    }
}
