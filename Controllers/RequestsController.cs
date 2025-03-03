using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Identity.Client;
using PrsNetWeb.Models;

namespace PrsNetWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestsController : ControllerBase
    {
        private readonly PrsContext _context;

        public RequestsController(PrsContext context)
        {
            _context = context;
        }

        // GET: api/Requests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Request>>> GetRequests()
        {
            var requests = _context.Requests.Include(r => r.User);
            return await requests.ToListAsync();
        }

        // GET: api/Requests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Request>> GetRequest(int id)
        {
            var request = await _context.Requests.Include(r => r.User)
                                                 .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            return request;
        }

        //GET: api/Requests/list-review/userid
        [HttpGet("list-review/{userId}")]

        //public async Task<ActionResult> GetRequestForReview(int userId)
        //{
        //show all requests in REVIEW for a userID that is not this userId
        //request.UserId != userid
        //request.Status = "REVIEW"
        public async Task<ActionResult<IEnumerable<Request>>> GetRequestForReview(int userId)
        {
            var requests = _context.Requests.Where(r => r.Status == "REVIEW")
                                            .Where(r => r.UserId != userId);


            return await requests.ToListAsync();
        }




        //}






        // PUT: api/Requests/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRequest(int id, Request request)
        {
            if (id != request.Id)
            {
                return BadRequest();
            }

            _context.Entry(request).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RequestExists(id))
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


        [HttpPut("submit-review/{id}")]
        public Request SubmitRequestForReview(int id)
        {
            //get request for id
            var request = _context.Requests.FirstOrDefault(r => r.Id == id);
            if (request.Total <= 50.00m)
                request.Status = "APPROVED";
            else
                //update request status "REVIEW"
                request.Status = "REVIEW";
            request.SubmittedDate = DateTime.Now;
            // save changes
            _context.SaveChanges();
            //return updated request
            return request;
        }

        [HttpPut("approve/{id}")]
        public Request ApproveRequest(int id)
        {
            //get request for id in REVIEW status
            var request = _context.Requests.Where(r => r.Status == "REVIEW")
                                           .FirstOrDefault(r => r.Id == id);

            //update to APPROVED
            request.Status = "APPROVED";
            //save request

            _context.SaveChanges();
            return request;
        }


        [HttpPut("reject/{id}")]
        public Request RejectRequest(int id, Request request)
        {
            //get request for id in REVIEW status
            //var request = _context.Requests.Where(r => r.Status == "REVIEW")
            //                               .FirstOrDefault(r => r.Id == id);
            //reasonForRejection != null

            if(request.ReasonForRejection != null)
            {
            //update to REJECTED
            request.Status = "REJECTED";
            }
            _context.Entry(request).State = EntityState.Modified;
            //save request
            _context.SaveChanges();
            return request;
        }








        // POST: api/Requests

        [HttpPost]
        public async Task<ActionResult<Request>> PostRequest(RequestCreate rc)
        {

            Request request = new Request();
            request.UserId = rc.UserId;
            request.RequestNumber = getNextRequestNumber();
            request.Description = rc.Description;
            request.Justification = rc.Justification;
            request.DateNeeded = rc.DateNeeded;
            request.DeliveryMode = rc.DeliveryMode;
            //method to generate requestnumber  - GetReqNum
            //to be done later
            
            //Status string "NEW"
            request.Status = "NEW";
            //Total int (0.0)
            request.Total = 0.0m;
            //Submitted Date = currentDate
            request.SubmittedDate = DateTime.Now;
            _context.Requests.Add(request);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRequest", new { id = request.Id }, request);
        }

        private string getNextRequestNumber()
        {
            // requestNumber format: R2409230011
            // 11 chars, 'R' + YYMMDD + 4 digit # w/ leading zeros
            string requestNbr = "R";
            // add YYMMDD string
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            requestNbr += today.ToString("yyMMdd");
            // get maximum request number from db
            string maxReqNbr = _context.Requests.Max(r => r.RequestNumber);
            String reqNbr = "";
            if (maxReqNbr != null)
            {
                // get last 4 characters, convert to number
                String tempNbr = maxReqNbr.Substring(7);
                int nbr = Int32.Parse(tempNbr);
                nbr++;
                // pad w/ leading zeros
                reqNbr += nbr;
                reqNbr = reqNbr.PadLeft(4, '0');
            }
            else
            {
                reqNbr = "0001";
            }
            requestNbr += reqNbr;
            return requestNbr;

        }







        // DELETE: api/Requests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var request = await _context.Requests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            _context.Requests.Remove(request);
            await _context.SaveChangesAsync();

            return NoContent();
        }



        private bool RequestExists(int id)
        {
            return _context.Requests.Any(e => e.Id == id);
        }
    }
}
