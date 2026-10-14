using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.DTOs;
using sms.api.Interfaces;
using System.Security.Claims;

namespace sms.api.Controllers.Library
{
    [Authorize]
    [Route("api/library")]
    [ApiController]
    public class LibraryController : ControllerBase
    {
        private readonly ILibraryManager _libraryManager;

        public LibraryController(ILibraryManager libraryManager)
        {
            _libraryManager = libraryManager;
        }

        #region Dashboard

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetLibraryDashboard()
        {
            try
            {
                var dashboard = await _libraryManager.GetLibraryDashboardAsync();
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving library dashboard", error = ex.Message });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetLibraryStats()
        {
            try
            {
                var dashboard = await _libraryManager.GetLibraryDashboardAsync();
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving library stats", error = ex.Message });
            }
        }

        #endregion

        #region Books

        [HttpGet("books")]
        public async Task<IActionResult> GetAllBooks()
        {
            try
            {
                var books = await _libraryManager.GetAllBooksAsync();
                return Ok(books);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving books", error = ex.Message });
            }
        }

        [HttpGet("books/{id}")]
        public async Task<IActionResult> GetBookById(Guid id)
        {
            try
            {
                var book = await _libraryManager.GetBookByIdAsync(id);
                if (book == null) return NotFound(new { message = "Book not found" });
                return Ok(book);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving book", error = ex.Message });
            }
        }

        #endregion

        #region Members

        [HttpGet("members")]
        public async Task<IActionResult> GetAllMembers()
        {
            try
            {
                var members = await _libraryManager.GetAllLibraryMembersAsync();
                return Ok(members);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving library members", error = ex.Message });
            }
        }

        [HttpGet("members/{id}")]
        public async Task<IActionResult> GetMemberById(Guid id)
        {
            try
            {
                var member = await _libraryManager.GetLibraryMemberByIdAsync(id);
                if (member == null) return NotFound(new { message = "Library member not found" });
                return Ok(member);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving library member", error = ex.Message });
            }
        }

        #endregion

        #region Issues

        [HttpGet("issues")]
        public async Task<IActionResult> GetAllIssues()
        {
            try
            {
                var issues = await _libraryManager.GetAllBookIssuesAsync();
                return Ok(issues);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving book issues", error = ex.Message });
            }
        }

        [HttpGet("issues/paged")]
        public async Task<IActionResult> GetIssuesPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? libraryMemberId = null,
            [FromQuery] bool? isReturned = null)
        {
            try
            {
                var result = await _libraryManager.GetBookIssuesPagedAsync(pageNumber, pageSize, searchTerm, libraryMemberId, isReturned);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving book issues", error = ex.Message });
            }
        }

        [HttpGet("issues/{id}")]
        public async Task<IActionResult> GetIssueById(Guid id)
        {
            try
            {
                var issue = await _libraryManager.GetBookIssueByIdAsync(id);
                if (issue == null) return NotFound(new { message = "Book issue not found" });
                return Ok(issue);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving book issue", error = ex.Message });
            }
        }

        #endregion

        #region Fines

        [HttpGet("fines")]
        public async Task<IActionResult> GetAllFines()
        {
            try
            {
                var fines = await _libraryManager.GetAllLibraryFinesAsync();
                return Ok(fines);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving library fines", error = ex.Message });
            }
        }

        [HttpGet("fines/paged")]
        public async Task<IActionResult> GetFinesPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? libraryMemberId = null,
            [FromQuery] string? status = null)
        {
            try
            {
                var result = await _libraryManager.GetLibraryFinesPagedAsync(pageNumber, pageSize, searchTerm, libraryMemberId, status);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving library fines", error = ex.Message });
            }
        }

        [HttpGet("fines/{id}")]
        public async Task<IActionResult> GetFineById(Guid id)
        {
            try
            {
                var fine = await _libraryManager.GetLibraryFineByIdAsync(id);
                if (fine == null) return NotFound(new { message = "Library fine not found" });
                return Ok(fine);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving library fine", error = ex.Message });
            }
        }

        #endregion

        #region Reservations

        [HttpGet("reservations")]
        public async Task<IActionResult> GetAllReservations()
        {
            try
            {
                var reservations = await _libraryManager.GetAllBookReservationsAsync();
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving book reservations", error = ex.Message });
            }
        }

        [HttpGet("reservations/{id}")]
        public async Task<IActionResult> GetReservationById(Guid id)
        {
            try
            {
                var reservation = await _libraryManager.GetBookReservationByIdAsync(id);
                if (reservation == null) return NotFound(new { message = "Book reservation not found" });
                return Ok(reservation);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving book reservation", error = ex.Message });
            }
        }

        #endregion

        #region Book Categories

        [HttpGet("categories")]
        public async Task<IActionResult> GetAllBookCategories()
        {
            try
            {
                var categories = await _libraryManager.GetAllBookCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving book categories", error = ex.Message });
            }
        }

        [HttpGet("categories/{id}")]
        public async Task<IActionResult> GetBookCategoryById(Guid id)
        {
            try
            {
                var category = await _libraryManager.GetBookCategoryByIdAsync(id);
                if (category == null)
                    return NotFound(new { message = "Book category not found" });

                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving book category", error = ex.Message });
            }
        }

        [HttpPost("categories")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CreateBookCategory([FromBody] CreateBookCategoryDTO createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
                var category = await _libraryManager.CreateBookCategoryAsync(createDto, userId);

                return CreatedAtAction(nameof(GetBookCategoryById), new { id = category.Id }, category);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating book category", error = ex.Message });
            }
        }

        [HttpPut("categories/{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateBookCategory(Guid id, [FromBody] UpdateBookCategoryDTO updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var category = await _libraryManager.UpdateBookCategoryAsync(id, updateDto);
                if (category == null)
                    return NotFound(new { message = "Book category not found" });

                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating book category", error = ex.Message });
            }
        }

        [HttpDelete("categories/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBookCategory(Guid id)
        {
            try
            {
                var result = await _libraryManager.DeleteBookCategoryAsync(id);
                if (!result)
                    return NotFound(new { message = "Book category not found" });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting book category", error = ex.Message });
            }
        }

        #endregion

        #region Authors

        [HttpGet("authors")]
        public async Task<IActionResult> GetAllAuthors()
        {
            try
            {
                var authors = await _libraryManager.GetAllAuthorsAsync();
                return Ok(authors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving authors", error = ex.Message });
            }
        }

        [HttpGet("authors/{id}")]
        public async Task<IActionResult> GetAuthorById(Guid id)
        {
            try
            {
                var author = await _libraryManager.GetAuthorByIdAsync(id);
                if (author == null)
                    return NotFound(new { message = "Author not found" });

                return Ok(author);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving author", error = ex.Message });
            }
        }

        [HttpGet("authors/search")]
        public async Task<IActionResult> SearchAuthors([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return BadRequest(new { message = "Search term is required" });

                var authors = await _libraryManager.SearchAuthorsAsync(searchTerm);
                return Ok(authors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error searching authors", error = ex.Message });
            }
        }

        [HttpPost("authors")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CreateAuthor([FromBody] CreateAuthorDTO createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
                var author = await _libraryManager.CreateAuthorAsync(createDto, userId);

                return CreatedAtAction(nameof(GetAuthorById), new { id = author.Id }, author);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating author", error = ex.Message });
            }
        }

        [HttpPut("authors/{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateAuthor(Guid id, [FromBody] CreateAuthorDTO updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var author = await _libraryManager.UpdateAuthorAsync(id, updateDto);
                if (author == null)
                    return NotFound(new { message = "Author not found" });

                return Ok(author);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating author", error = ex.Message });
            }
        }

        [HttpDelete("authors/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAuthor(Guid id)
        {
            try
            {
                var result = await _libraryManager.DeleteAuthorAsync(id);
                if (!result)
                    return NotFound(new { message = "Author not found" });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting author", error = ex.Message });
            }
        }

        #endregion

        #region Publishers

        [HttpGet("publishers")]
        public async Task<IActionResult> GetAllPublishers()
        {
            try
            {
                var publishers = await _libraryManager.GetAllPublishersAsync();
                return Ok(publishers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving publishers", error = ex.Message });
            }
        }

        [HttpGet("publishers/{id}")]
        public async Task<IActionResult> GetPublisherById(Guid id)
        {
            try
            {
                var publisher = await _libraryManager.GetPublisherByIdAsync(id);
                if (publisher == null)
                    return NotFound(new { message = "Publisher not found" });

                return Ok(publisher);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving publisher", error = ex.Message });
            }
        }

        [HttpGet("publishers/search")]
        public async Task<IActionResult> SearchPublishers([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return BadRequest(new { message = "Search term is required" });

                var publishers = await _libraryManager.SearchPublishersAsync(searchTerm);
                return Ok(publishers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error searching publishers", error = ex.Message });
            }
        }

        [HttpPost("publishers")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CreatePublisher([FromBody] CreatePublisherDTO createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
                var publisher = await _libraryManager.CreatePublisherAsync(createDto, userId);

                return CreatedAtAction(nameof(GetPublisherById), new { id = publisher.Id }, publisher);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating publisher", error = ex.Message });
            }
        }

        [HttpPut("publishers/{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdatePublisher(Guid id, [FromBody] CreatePublisherDTO updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var publisher = await _libraryManager.UpdatePublisherAsync(id, updateDto);
                if (publisher == null)
                    return NotFound(new { message = "Publisher not found" });

                return Ok(publisher);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating publisher", error = ex.Message });
            }
        }

        [HttpDelete("publishers/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePublisher(Guid id)
        {
            try
            {
                var result = await _libraryManager.DeletePublisherAsync(id);
                if (!result)
                    return NotFound(new { message = "Publisher not found" });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting publisher", error = ex.Message });
            }
        }

        #endregion
    }
}
