using KesarPremium.Core.DTOs.Request;
using KesarPremium.Core.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KesarPremium.API.Controllers
{
    [Route("api/hostels")]
    public class HostelController : BaseController
    {
        private readonly IHostelService _hostelService;
        public HostelController(IHostelService hostelService) => _hostelService = hostelService;

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] HostelSearchRequest request)
        {
            var result = await _hostelService.SearchAsync(request);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var result = await _hostelService.GetDetailAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("category/{name}")]
        public async Task<IActionResult> GetByCategory(string name)
        {
            var result = await _hostelService.GetByCategoryAsync(name);
            return Ok(result);
        }

        [HttpGet("location/{locationId}")]
        public async Task<IActionResult> GetByLocation(int locationId)
        {
            var result = await _hostelService.GetByLocationAsync(locationId);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateHostelRequest request)
        {
            var result = await _hostelService.CreateAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromBody] UpdateHostelRequest request)
        {
            var result = await _hostelService.UpdateAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _hostelService.DeleteAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }
    }
}
