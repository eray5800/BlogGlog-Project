using BAL;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlogProjeAPI.Controllers.Admin
{
    [Route("api/admin/[controller]/[action]")]
    [Authorize(Roles = "Admin")]
    public class WriterRequestController : ControllerBase
    {
        private readonly WriterRequestService _writerRequestService;
        private readonly RoleService _roleService;

        public WriterRequestController(WriterRequestService writerRequestService, RoleService roleService)
        {
            _writerRequestService = writerRequestService;
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllWriterRequests()
        {
            IEnumerable<WriterRequest> writerRequests = await _writerRequestService.GetAllWriterRequestsAsync();
            return Ok(writerRequests);
        }

        [HttpPost("{requestID}")]
        public async Task<IActionResult> ApproveWriterRequest(Guid requestID)
        {
            var writerRequest = await _writerRequestService.GetWriterRequestByIDAsync(requestID);
            if (writerRequest == null)
            {
                return NotFound("Writer request not found.");
            }

            // Approve the writer request
            var approveResult = await _writerRequestService.DeleteWriterRequestAsync(requestID);
            if (!approveResult)
            {
                return BadRequest("An error occurred while approving the writer request.");
            }

            try
            {
                // Assign the "Writer" role to the user
                await _roleService.AssignRoleAsync(writerRequest.User.Id, "Writer");
                return Ok("Writer request approved and role assigned successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception as needed
                return BadRequest($"Writer request approved, but role assignment failed: {ex.Message}");
            }
        }


        [HttpPost("{requestID}")]
        public async Task<IActionResult> RejectWriterRequest(Guid requestID)
        {
            var result = await _writerRequestService.DeleteWriterRequestAsync(requestID);
            if (result)
            {
                return Ok("Writer request rejected successfully.");
            }

            return BadRequest("An error occurred while rejecting the writer request.");
        }
    }
}
