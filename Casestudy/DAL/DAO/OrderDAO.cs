using Casestudy.DAL.DomainClasses;
using Casestudy.DAL.Helpers;
using CaseStudyAPI.DAL.DomainClasses;
using Microsoft.EntityFrameworkCore;

namespace Casestudy.DAL.DAO
{
    public class OrderDAO
    {
        private readonly AppDbContext _db;
        public OrderDAO(AppDbContext ctx)
        {
            _db = ctx;
        }

        public async Task<List<Order>> GetAll(int id)
        {
            return await _db.Orders!.Where(order => order.CustomerId == id).ToListAsync<Order>();
        }

        public async Task<List<OrderDetailsHelper>> GetOrderDetails(int tid, string email)
        {
            Customer? customer = _db.Customers!.FirstOrDefault(customer => customer.Email == email);
            List<OrderDetailsHelper> allDetails = new();
            // LINQ way of doing INNER JOINS
            var results = from o in _db.Orders
                          join oi in _db.OrderLineItems! on o.Id equals oi.OrderId
                          join p in _db.Product! on oi.ProductId equals p.Id
                          where (o.CustomerId == customer!.Id && o.Id == tid)
                          select new OrderDetailsHelper
                          {
                              OrderId = o.Id,
                              ProductId = oi.ProductId,
                              ProductName = p.ProductName,
                              CustomerId = customer!.Id,
                              Description = p.Description,
                              QtyO = oi.QtyOrdered,
                              QtyS = oi.QtySold,
                              QtyB = oi.QtyBackOrdered,
                              ProductCost = p.MSRP,
                              OrderTotal = o.OrderAmount,
                              DateCreated = o.OrderDate.ToString("yyyy/MM/dd - hh:mm tt")
                          };
            allDetails = await results.ToListAsync();
            return allDetails;
        }

        public async Task<int> AddOrder(int customerid, OrderSelectionHelper[] selections)
        {
            int orderId = -1;
            // we need a transaction as multiple entities involved
            using (var _trans = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    Order order = new();
                    order.CustomerId = customerid;
                    order.OrderDate = System.DateTime.Now;
                    order.OrderAmount = 0;
                    //calculate the totals and then add the order row to the table
                    foreach (OrderSelectionHelper selection in selections)
                    {
                        order.OrderAmount += selection.Item!.MSRP * selection.Qty;
                    }
                    await _db.Orders!.AddAsync(order);
                    await _db.SaveChangesAsync();

                    // then add each item to the orderitems table
                    foreach (OrderSelectionHelper selection in selections)
                    {
                        OrderLineItem oItem = new();

                        Product product = _db.Product!.FirstOrDefault(p => p.Id == selection.Item.Id);

                        oItem.ProductId = selection.Item!.Id;
                        oItem.OrderId = order.Id;
                        oItem.SellingPrice = selection.Item.MSRP;
                        oItem.QtyOrdered = selection.Qty;

                        if (oItem.QtyOrdered < selection.Item.QtyOnHand)
                        {
                            product.QtyOnHand -= selection.Qty;
                            oItem.QtySold = selection.Qty;
                            oItem.QtyBackOrdered = 0;
                        }
                        else if (oItem.QtyOrdered > selection.Item.QtyOnHand)
                        {
                            product.QtyOnHand = 0;
                            product.QtyOnBackOrder += (selection.Qty - selection.Item.QtyOnHand);
                            oItem.QtySold = selection.Item.QtyOnHand;
                            oItem.QtyBackOrdered = selection.Qty - selection.Item.QtyOnHand;
                        }

                        await _db.OrderLineItems!.AddAsync(oItem);
                        _db.Product!.Update(product);
                        await _db.SaveChangesAsync();
                    }
                    // test trans by uncommenting out these 3 lines
                    //int x = 1;
                    //int y = 0;
                    //x = x / y;
                    await _trans.CommitAsync();
                    orderId = order.Id;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await _trans.RollbackAsync();
                }
            }
            return orderId;

        }
    }
}
