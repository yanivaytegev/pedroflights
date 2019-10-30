using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication2.Models;


namespace WebApplication2.Controllers
{
    public class Item
    {
        private FlightTicket flights = new FlightTicket();
        private int quantity;

        public Item()
        { }

        public Item(FlightTicket flights, int quantity)
        {
            this.flights = flights;
            this.quantity = quantity;
        }

        public FlightTicket Flights { get => flights; set => flights = value; }
        public int Quantity { get => quantity; set => quantity = value; }
    }
}