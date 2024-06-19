using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuctionsController : ControllerBase
    {
        private readonly AuctionDbContext _context;
        private readonly IMapper _mapper;

        public AuctionsController(AuctionDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuction(string date)
        {
            var query = _context.Auctions.OrderBy(x=>x.Item.Make).AsQueryable();
            //Modified during implementing sychronous communication between search and auction

            if (!string.IsNullOrEmpty(date))
            {
                //Returning the auctions which are greater that the given date
                query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
            }
            //Project to used for to project queryable data ConfigurationProvider get the mapping profile
            return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
            //Primary use 
            //var auctions = await _context.Auctions
            //    .Include(x => x.Item)
            //    .OrderBy(x => x.Item.Make)
            //    .ToListAsync();
            //return _mapper.Map<List<AuctionDto>>(auctions);

        }
        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById([FromRoute] Guid id)
        {
            var auction = await _context.Auctions
                .Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id);

            if(auction == null) return NotFound();

            return _mapper.Map<AuctionDto>(auction);

        }
        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuction)
        {
            var auction = _mapper.Map<Auction>(createAuction);
            //TODO: add current user as seller
            auction.Seller = "test";

            _context.Auctions.Add(auction);
            var result = await _context.SaveChangesAsync() > 0;
            if (!result) return BadRequest("Could not save changes to DB");
            //We want to show user that where the auction is created, so with the nameof keyword we are navigating the user to the 
            //action written above so that he can locate the created auction.
            //As GetAuctionById need id as parmeter it is passed with new {}
            //and then mapped the response.
            return CreatedAtAction(nameof(GetAuctionById),new { auction.Id},_mapper.Map<AuctionDto>(auction));
        }
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction([FromRoute]Guid id, [FromBody] UpdateAuctionDto updateAuction)
        {
            var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);
            if(auction == null) return NotFound();
            //Todo: check seller == username
            auction.Item.Make = updateAuction.Make??auction.Item.Make;
            auction.Item.Model = updateAuction.Model??auction.Item.Model;
            auction.Item.Color = updateAuction.Color??auction.Item.Color;
            auction.Item.Mileage = updateAuction.Mileage??auction.Item.Mileage;
            auction.Item.Year = updateAuction.Year ?? auction.Item.Year;

            var result = await _context.SaveChangesAsync() >0;
            if (result) return Ok();
            return BadRequest("Problem saving changes");
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction([FromRoute] Guid id)
        {
            var auction = await _context.Auctions.FindAsync(id);
            if (auction == null) return NotFound();
            //TODO: check seller == username
            _context.Auctions.Remove(auction);
            var result = await _context.SaveChangesAsync() > 0;
            if (!result) return BadRequest("Could not update DB");
            return Ok();
        }
    }
}
