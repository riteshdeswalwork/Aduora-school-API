using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.DTOs;
using sms.api.Interfaces;

namespace sms.api.Controllers.Admin
{
    [Route("api/designations")]
    [ApiController]
    [Authorize]
    public class DesignationController : ControllerBase
    {
        private readonly IDesignationManager _designationManager;

        public DesignationController(IDesignationManager designationManager)
        {
            _designationManager = designationManager;
        }

        // GET: api/designations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DesignationDTO>>> GetDesignations()
        {
            var designations = await _designationManager.GetAllDesignationsAsync();
            return Ok(designations);
        }

        // GET: api/designations/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<DesignationDTO>> GetDesignation(Guid id)
        {
            var designationDto = await _designationManager.GetDesignationByIdAsync(id);

            if (designationDto == null)
            {
                return NotFound();
            }

            return Ok(designationDto);
        }

        // POST: api/designations
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DesignationDTO>> CreateDesignation(CreateDesignationDTO designationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdDesignation = await _designationManager.CreateDesignationAsync(designationDto);
            return CreatedAtAction(nameof(GetDesignation), new { id = createdDesignation.Id }, createdDesignation);
        }

        // PUT: api/designations/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDesignation(Guid id, UpdateDesignationDTO designationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedDesignation = await _designationManager.UpdateDesignationAsync(id, designationDto);

            if (updatedDesignation == null)
            {
                return NotFound();
            }

            return Ok(updatedDesignation);
        }

        // DELETE: api/designations/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDesignation(Guid id)
        {
            var result = await _designationManager.DeleteDesignationAsync(id);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
