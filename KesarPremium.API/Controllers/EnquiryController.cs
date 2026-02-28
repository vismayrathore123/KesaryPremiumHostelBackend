using KesarPremium.Core.DTOs.Request;
using KesarPremium.Core.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KesarPremium.API.Controllers
{
    [Route("api/enquiries")]
    public class EnquiryController : BaseController
    {
        private readonly IEnquiryService _enquiryService;
        public EnquiryController(IEnquiryService enquiryService) => _enquiryService = enquiryService;

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateEnquiryRequest request)
        {
            var result = await _enquiryService.CreateAsync(CurrentUserId, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMy()
        {
            var result = await _enquiryService.GetUserEnquiriesAsync(CurrentUserId);
            return Ok(result);
        }

        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            var result = await _enquiryService.GetAdminEnquiriesAsync(status);
            return Ok(result);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromBody] UpdateEnquiryRequest request)
        {
            var result = await _enquiryService.UpdateAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }

}
