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
        private readonly ILogger<ClientsController> _logger;

        public ClientsController(ApplicationDbContext context, ILogger<ClientsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientDTO>>> GetClients()
        {
            try
            {
                var clients = await _context.Clients
                    .Include(c => c.Assignments)
                    .Select(c => new ClientDTO
                    {
                        Id = c.Id,
                        Name = c.Name,
                        AssignedFormsCount = c.Assignments.Count
                    })
                    .ToListAsync();

                return Ok(clients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving clients");
                return StatusCode(500, "Error retrieving clients");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClientDTO>> GetClient(int id)
        {
            try
            {
                var client = await _context.Clients
                    .Include(c => c.Assignments)
                    .Where(c => c.Id == id)
                    .Select(c => new ClientDTO
                    {
                        Id = c.Id,
                        Name = c.Name,
                        AssignedFormsCount = c.Assignments.Count
                    })
                    .FirstOrDefaultAsync();

                if (client == null)
                {
                    return NotFound($"Client with ID {id} not found.");
                }

                return Ok(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client {ClientId}", id);
                return StatusCode(500, "Error retrieving client");
            }
        }

        [HttpPost]
        public async Task<ActionResult<ClientDTO>> CreateClient(ClientDTO clientDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(clientDto.Name))
                {
                    return BadRequest("Client name is required.");
                }

                var client = new Client
                {
                    Name = clientDto.Name.Trim()
                };

                _context.Clients.Add(client);
                await _context.SaveChangesAsync();

                var createdClient = new ClientDTO
                {
                    Id = client.Id,
                    Name = client.Name,
                    AssignedFormsCount = 0
                };

                return CreatedAtAction(nameof(GetClient), new { id = client.Id }, createdClient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client: {ClientName}", clientDto.Name);
                return StatusCode(500, "Error creating client");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(int id, ClientDTO clientDto)
        {
            try
            {
                if (id != clientDto.Id)
                {
                    return BadRequest("ID mismatch");
                }

                if (string.IsNullOrWhiteSpace(clientDto.Name))
                {
                    return BadRequest("Client name is required.");
                }

                var client = await _context.Clients.FindAsync(id);
                if (client == null)
                {
                    return NotFound($"Client with ID {id} not found.");
                }

                client.Name = clientDto.Name.Trim();

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ClientExists(id))
                    {
                        return NotFound($"Client with ID {id} not found.");
                    }
                    throw;
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client {ClientId}", id);
                return StatusCode(500, "Error updating client");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            try
            {
                var client = await _context.Clients
                    .Include(c => c.Assignments)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (client == null)
                {
                    return NotFound($"Client with ID {id} not found.");
                }

                if (client.Assignments.Any())
                {
                    _logger.LogWarning("Attempted to delete client {ClientId} with {FormCount} assigned forms", 
                        id, client.Assignments.Count);
                    return BadRequest("Cannot delete client with assigned forms. Please remove form assignments first.");
                }

                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting client {ClientId}", id);
                return StatusCode(500, "Error deleting client");
            }
        }

        private async Task<bool> ClientExists(int id)
        {
            return await _context.Clients.AnyAsync(c => c.Id == id);
        }
    }
}
