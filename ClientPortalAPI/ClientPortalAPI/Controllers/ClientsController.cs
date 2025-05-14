using ClientPortalAPI.Data;
using ClientPortalAPI.DTOs;
using ClientPortalAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientPortalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet()]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<ClientDTO>>> GetClients()
        {
            var clients = await _context.Clients
                                        .Select(c => new ClientDTO
                                        {
                                            Id = c.Id,
                                            Name = c.Name,
                                        })
                                        .ToListAsync();

            return Ok(clients);
        }

        [HttpPost("createclient")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ClientDTO>> CreateClient(ClientDTO clientDto)
        {
            var client = new Client
            {
                Name = clientDto.Name,
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClients), new { id = client.Id }, client);
        }


    }
}
