using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrsNetWeb.Models;

namespace PrsNetWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LineItemsController : ControllerBase
    {
        private readonly PrsContext _context;

        public LineItemsController(PrsContext context)
        {
            _context = context;
        }

        // GET: api/LineItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LineItem>>> GetLineItems()
        {
            var lineItem = _context.LineItems.Include(l => l.Product)
                                             .Include(l => l.Request);
            return await lineItem.ToListAsync();
        }

        // GET: api/LineItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LineItem>> GetLineItem(int id)
        {
            var lineItem = await _context.LineItems.Include(l => l.Product)
                                                   .Include(l => l.Request)
                                                   .FirstOrDefaultAsync(l => l.Id == id);

            if (lineItem == null)
            {
                return NotFound();
            }

            return lineItem;
        }

        [HttpGet("lines-for-req/{requestId}")]
        public List<LineItem> LineItemsForRequest(int requestId)
        {

            List<LineItem> lineItem = _context.LineItems.Where(lineItem => lineItem.RequestId == requestId).ToList();

            return lineItem;

        }






        // PUT: api/LineItems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLineItem(int id, int requestId, LineItem lineItem)
        {
            if (id != lineItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(lineItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                RecalculateTotals(lineItem.RequestId);     
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LineItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }


            return NoContent();
        }

        // POST: api/LineItems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<LineItem>> PostLineItem(LineItem lineItem)
        {
            _context.LineItems.Add(lineItem);
            await _context.SaveChangesAsync();

            RecalculateTotals(lineItem.RequestId);    
            return CreatedAtAction("GetLineItem", new { id = lineItem.Id }, lineItem);
        }

        // DELETE: api/LineItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLineItem(int id, int requestId)
        {
            var lineItem = await _context.LineItems.FindAsync(id);
            if (lineItem == null)
            {
                return NotFound();
            }

            _context.LineItems.Remove(lineItem);
            await _context.SaveChangesAsync();

            RecalculateTotals(lineItem.RequestId);         

            return NoContent();
        }

        private bool LineItemExists(int id)
        {
            return _context.LineItems.Any(e => e.Id == id);
        }

        private void RecalculateTotals(int requestId)
        {
            //Insert,Update, Del has already occured
            var request = _context.Requests.Find(requestId);
            //Get all MC records for this user
            var req = _context.LineItems.Include(li => li.Product)
                                        .Where(li => li.RequestId == requestId);
            //Loop through all MCs and sum the PurchasePrice values
            decimal sum = 0;
            foreach (LineItem li in req)
            {
                sum += li.Quantity * li.Product.Price;
            }
                //Set sum in the User.CollectionValue property
                request.Total = sum;
                //Save user record
                _context.SaveChanges();


        }
    }
}
