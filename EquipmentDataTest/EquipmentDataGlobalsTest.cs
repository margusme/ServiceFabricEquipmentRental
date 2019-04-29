using Microsoft.VisualStudio.TestTools.UnitTesting;
using EquipmentData.Globals;
using Shouldly;
using System;

namespace EquipmentDataTest
{
    [TestClass]
    class EquipmentDataGlobalsTest
    {
        /// <summary>
        /// Testing only whats is coming in should be returned without modifications, so this is getters and setters testing
        /// </summary>
        [TestMethod]
        public void EquipmentOrder_Should_Give_Correct_Results()
        {
            var order = new EquipmentOrder();
            order.Days = 1;
            order.Days.ShouldBe(1);

            order.Name = "ABC";
            order.Name.ShouldBe("ABC");

            var now = DateTime.Now;
            order.OrderTime = now;
            order.OrderTime.ShouldBe(now);

            order.Type = EquipmentType.Regular;
            order.Type.ShouldBe(EquipmentType.Regular);
        }

        /// <summary>
        /// Testing only whats is coming in should be returned without modifications, so this is getters and setters testing
        /// </summary>
        [TestMethod]
        public void Invoice_Should_Give_Correct_Results()
        {
            var invoice = new Invoice();
            invoice.Rows.ShouldBe(null);
            invoice.Rows = new System.Collections.Generic.List<InvoiceRow>();
            invoice.Rows.Count.ShouldBe(0);

            invoice.Price = 100;
            invoice.Price.ShouldBe(100);

            invoice.Bonuses = 3;
            invoice.Bonuses.ShouldBe(3);

            invoice.Title = "ABC";
            invoice.Title.ShouldBe("ABC");
        }

        /// <summary>
        /// Testing only whats is coming in should be returned without modifications, so this is getters and setters testing
        /// </summary>
        [TestMethod]
        public void InvoiceRow_Should_Give_Correct_Results()
        {
            var invoiceRow = new InvoiceRow();
            invoiceRow.Name = "ABC";
            invoiceRow.Name.ShouldBe("ABC");

            invoiceRow.Price = 10;
            invoiceRow.Price.ShouldBe(10);
        }
    }
}
