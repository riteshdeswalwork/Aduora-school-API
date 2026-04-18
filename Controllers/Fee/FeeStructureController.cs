using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Interfaces;
using sms.api.Models;

namespace sms.api.Controllers.Fee
{
    [Route("api/fee-structures")]
    [Authorize]
    public class FeeStructureController : ApiControllerBase
    {
        private readonly IFeeStructureManager _feeStructureManager;

        public FeeStructureController(IFeeStructureManager feeStructureManager)
        {
            _feeStructureManager = feeStructureManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAll([FromQuery] Guid? academicYearId = null)
        {
            if (academicYearId.HasValue && academicYearId.Value != Guid.Empty)
            {
                var byYear = await _feeStructureManager.GetFeeStructuresByAcademicYearIdAsync(academicYearId.Value);
                return Ok(byYear);
            }
            var feeStructures = await _feeStructureManager.GetAllAsync();
            return Ok(feeStructures);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var feeStructure = await _feeStructureManager.GetByIdAsync(id);
            if (feeStructure == null)
                return NotFound();

            return Ok(feeStructure);
        }

        [HttpGet("class/{classId}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetByClassId(Guid classId)
        {
            var feeStructures = await _feeStructureManager.GetFeeStructuresByClassIdAsync(classId);
            return Ok(feeStructures);
        }

        [HttpGet("active")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetActive()
        {
            var feeStructures = await _feeStructureManager.GetActiveFeeStructuresAsync();
            return Ok(feeStructures);
        }

        [HttpGet("academic-year/{academicYear}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetByAcademicYear(string academicYear)
        {
            var feeStructures = await _feeStructureManager.GetFeeStructuresByAcademicYearAsync(academicYear);
            return Ok(feeStructures);
        }

        [HttpGet("class/{classId}/type/{feeType}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetByClassAndType(Guid classId, string feeType)
        {
            var feeStructure = await _feeStructureManager.GetFeeStructureByClassAndTypeAsync(classId, feeType);
            if (feeStructure == null)
                return NotFound();

            return Ok(feeStructure);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] FeeStructure feeStructure)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            feeStructure.Id = Guid.NewGuid();
            feeStructure.CreatedAt = DateTime.UtcNow;

            var createdFeeStructure = await _feeStructureManager.CreateAsync(feeStructure);
            return CreatedAtAction(nameof(GetById), new { id = createdFeeStructure.Id }, createdFeeStructure);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] FeeStructure feeStructure)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingFeeStructure = await _feeStructureManager.GetByIdAsync(id);
            if (existingFeeStructure == null)
                return NotFound();

            feeStructure.Id = id;
            feeStructure.CreatedAt = existingFeeStructure.CreatedAt;
            feeStructure.UpdatedAt = DateTime.UtcNow;

            var updatedFeeStructure = await _feeStructureManager.UpdateAsync(feeStructure);
            return Ok(updatedFeeStructure);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _feeStructureManager.DeleteAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpPost("{id}/lines")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddLine(Guid id, [FromBody] FeeStructureLine line)
        {
            if (line == null || !ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                line.FeeStructureId = id;
                var created = await _feeStructureManager.AddLineAsync(id, line);
                return Created($"/api/fee-structures/{id}/lines/{created.Id}", created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/lines/{lineId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateLine(Guid id, Guid lineId, [FromBody] FeeStructureLine line)
        {
            if (line == null || !ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _feeStructureManager.UpdateLineAsync(id, lineId, line);
            if (updated == null)
                return NotFound();

            return Ok(updated);
        }

        [HttpDelete("{id}/lines/{lineId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteLine(Guid id, Guid lineId)
        {
            try
            {
                var ok = await _feeStructureManager.DeleteLineAsync(id, lineId);
                if (!ok)
                    return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
