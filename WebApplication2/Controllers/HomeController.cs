using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication2.Models;


namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        dbModel db = new dbModel();

        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult contact()
        {
            return View();
        }

        public ActionResult about()
        {
            return View();
        }

        public ActionResult registration()
        {
            Customer newCustomer = new Customer();

            return View(newCustomer);
        }

        [HttpPost]
        public ActionResult registration(Customer customer, HttpPostedFileBase imageFile = null)
        {

            string fileCustomerName = Path.GetFileNameWithoutExtension(imageFile.FileName);

            string extension = Path.GetExtension(imageFile.FileName);

            fileCustomerName = fileCustomerName + DateTime.Now.ToString("yymmssfff") + extension;

            customer.customerImage = "../../images/customerImage/" + fileCustomerName;

            fileCustomerName = Path.Combine(Server.MapPath("~/images/customerImage/"), fileCustomerName);

            imageFile.SaveAs(fileCustomerName);



            if (db.Customers.Any(p => p.customerUserName == customer.customerUserName))
            {
                Response.Output.Write("<script>alert('user already exist')</script>");
                return View("", customer);
            }

            customer.customerActive = true;
            db.Customers.Add(customer);
            db.SaveChanges();
            Response.Output.Write("<script>alert('registration successfully')</script>");
            return View("Index");
        }

        public ActionResult login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult login(Customer customer)
        {
            var user = db.Customers.SingleOrDefault(m => m.customerUserName.Equals(customer.customerUserName) && m.customerUserPassword.Equals(customer.customerUserPassword));
            if (user != null)
            {
                Session["userId"] = user.customerID.ToString();
                Session["userName"] = user.customerUserName.ToString();
                Response.Output.Write("<script>alert('welcome " + user.customerFirstName + " " + user.customerLastName + "')</script>");
                Response.Output.Write("<script> window.location.href = '../home/Index'</script>");
            }
            else
            {
                Response.Output.Write("<script>alert('wrong user name or password')</script>");
            }

            return View();
        }

        public ActionResult loggedin()
        {
            if (Session["userId"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("login");
            }
        }

        public ActionResult profile()
        {

            if (int.TryParse(Session["userId"].ToString(), out int result))
            {
                Customer customer = db.Customers.FirstOrDefault(x => x.customerID == result);
                return View(customer);
            }
            return View("Index");
        }

        public ActionResult addFlight()
        {
            FlightTicket flightTicket = new FlightTicket();

            return View(flightTicket);
        }

        [HttpPost]
        public ActionResult addFlight(FlightTicket flightTicket)
        {
            string fileFlightTicketName = Path.GetFileNameWithoutExtension(flightTicket.imageFile.FileName);

            string extension = Path.GetExtension(flightTicket.imageFile.FileName);

            fileFlightTicketName = fileFlightTicketName + DateTime.Now.ToString("yymmssfff") + extension;

            flightTicket.flightTicketPicture = "../../images/flightImage/" + fileFlightTicketName;

            fileFlightTicketName = Path.Combine(Server.MapPath("~/images/flightImage/"), fileFlightTicketName);

            flightTicket.imageFile.SaveAs(fileFlightTicketName);



            flightTicket.flightActive = true;

            if (int.TryParse(Session["userId"].ToString(), out int result))
            {
                flightTicket.customerID = result;
            }
            db.FlightTickets.Add(flightTicket);
            db.SaveChanges();
            Response.Output.Write("<script>alert('youre flight add succssfully')</script>");

            return View("Index");
        }

        public ActionResult logout()
        {
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult search(string Location, DateTime? checkin_date, DateTime? checkout_date, int Guest)
        {

            if (Location != null && checkin_date != null && checkout_date != null)
            {
                List<FlightTicket> flight = db.FlightTickets.Where(p => p.flightDestanation == Location && p.flightBackDate == checkout_date && p.flighGoDate == checkin_date).ToList();

                return View(flight);
            }

            if (Location != null && checkin_date != null)
            {
                List<FlightTicket> flight = db.FlightTickets.Where(p => p.flightDestanation == Location && p.flighGoDate == checkin_date).ToList();

                return View(flight);
            }

            if (Location != null)
            {
                List<FlightTicket> flight = db.FlightTickets.Where(p => p.flightDestanation == Location).ToList();

                return View(flight);
            }

            else
            {
                List<FlightTicket> flight = db.FlightTickets.Where(p => p.flightDestanation == Location && p.flightBackDate == checkout_date && p.flighGoDate == checkin_date && p.flightTicketQuantity == Guest).ToList();

                return View(flight);
            }

        }

        public ActionResult addToCart()
        {
            if (Session["cart"] == null)
            {
                Session["cart"] = new List<Item>();
            }

            return View();
        }

        [HttpPost]
        public ActionResult addToCart(int id)
        {

            if (Session["cart"] == null)
            {
                List<Item> cart = new List<Item>();
                cart.Add(new Item(db.FlightTickets.Find(id), 1));
                Session["cart"] = cart;
                Session["count"] = cart.Count();
            }
            else
            {
                List<Item> cart = (List<Item>)Session["cart"];
                int index = isExisting(id);
                if (index == -1)
                    cart.Add(new Item(db.FlightTickets.Find(id), 1));
                else
                    cart[index].Quantity++;
                Session["cart"] = cart;
                Session["count"] = cart.Count();
            }

            return View();

        }

        private int isExisting(int id)
        {
            List<Item> cart = (List<Item>)Session["cart"];
            for (int i = 0; i < cart.Count; i++)
                if (cart[i].Flights.flightID == id)
                    return i;
            return -1;
        }

        public ActionResult removeFromCart(int id)
        {

            List<Item> cart = (List<Item>)Session["cart"];
            int index = isExisting(id);
            if (index == 0 && cart[0].Quantity == 1)
            {
                cart.RemoveAt(0);
                return View("index");
            }
            else
            {
                if (cart[index].Quantity == 1)
                {
                    cart.RemoveAt(index);
                }
                else
                {
                    cart[index].Quantity--;
                }
                Session["cart"] = cart;
                Session["count"] = cart.Count();
            }

            return View("addToCart");
        }

        public ActionResult finalPurchase()
        {
            foreach (var item in (List<Item>)Session["cart"])
            {
                for (int i = 0; i < item.Quantity; i++)
                {

                    FlightTicket result = db.FlightTickets.SingleOrDefault(p => p.flightID == item.Flights.flightID);

                    if (result != null)
                    {
                        bool flightActive = false;
                        result.flightActive = flightActive;

                        db.SaveChanges();
                    }

                    orderDetail(result);
                }
            }
            Response.Output.Write("<script>alert('thank you for buying with Pedro-Flight')</script>");

            Session["cart"] = new List<Item>();
            return View("Index");
        }

        public void orderDetail(FlightTicket result)
        {
            Order order = new Order();

            order.customerID = result.customerID;
            order.flightID = result.flightID;
            order.orderPrice = result.flightPrice;
            order.orderDate = DateTime.Now.ToString();
            db.Orders.Add(order);
            db.SaveChanges();
        }

        public ActionResult flightDetails(int id)
        {
            FlightTicket flightTicket = db.FlightTickets.FirstOrDefault(x => x.flightID == id);

            return View(flightTicket);
        }
    }
}

