using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentData.Globals
{
    public class Invoice
    {
        private string title;
        private List<InvoiceRow> rows;
        private int price;
        private int bonuses;

        public string Title { get => title; set => title = value; }
        public List<InvoiceRow> Rows { get => rows; set => rows = value; }
        public int Price { get => price; set => price = value; }
        public int Bonuses { get => bonuses; set => bonuses = value; }
    }

    public class InvoiceRow
    {
        private string name;
        private int price;

        public string Name { get => name; set => name = value; }
        public int Price { get => price; set => price = value; }
    }
}
