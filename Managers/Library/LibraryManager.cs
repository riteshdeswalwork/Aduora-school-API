using Dapper;
using Microsoft.EntityFrameworkCore;
using sms.api.Data;
using sms.api.DTOs;
using sms.api.Interfaces.Library;
using sms.api.Interfaces.Shared;
using sms.api.Managers.Shared;
using sms.api.Models;
using sms.api.Services.Data;

namespace sms.api.Managers.Library
{
    public class LibraryManager : ILibraryManager
    {
        private readonly ApplicationDbContext _context;
        private readonly ISqlDataAccess _sqlDataAccess;

        public LibraryManager(ApplicationDbContext context, ISqlDataAccess sqlDataAccess)
        {
            _context = context;
            _sqlDataAccess = sqlDataAccess;
        }

        #region Dashboard

        public async Task<LibraryDashboardDTO> GetLibraryDashboardAsync()
        {
            var totalBooks = await _context.Books.CountAsync(b => b.IsActive);
            var availableBooks = await _context.Books.CountAsync(b => b.IsActive && b.IsAvailable);
            var issuedBooks = totalBooks - availableBooks;
            var reservedBooks = await _context.BookReservations.CountAsync(br => br.Status == "Active");

            var totalMembers = await _context.LibraryMembers.CountAsync(lm => lm.IsActive);
            var activeMembers = await _context.LibraryMembers.CountAsync(lm => lm.IsActive && !lm.IsSuspended);
            var suspendedMembers = await _context.LibraryMembers.CountAsync(lm => lm.IsActive && lm.IsSuspended);

            var totalIssues = await _context.BookIssues.CountAsync(bi => bi.IsActive);
            var overdueIssues = await _context.BookIssues.CountAsync(bi => bi.IsActive && !bi.IsReturned && bi.DueDate < DateTime.Now);
            var todayIssues = await _context.BookIssues.CountAsync(bi => bi.IssueDate.Date == DateTime.Today);
            var todayReturns = await _context.BookIssues.CountAsync(bi => bi.ReturnDate.HasValue && bi.ReturnDate.Value.Date == DateTime.Today);

            var totalFines = await _context.LibraryFines.SumAsync(lf => lf.Amount);
            var pendingFines = await _context.LibraryFines.SumAsync(lf => lf.PendingAmount);
            var pendingReservations = await _context.BookReservations.CountAsync(br => br.Status == "Active");

            // Get popular books (most issued)
            var popularBooks = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.TotalIssues)
                .Take(5)
                .Select(b => MapBookToDTO(b))
                .ToListAsync();

            // Get recent issues
            var recentIssues = await _context.BookIssues
                .Include(bi => bi.Book)
                .Include(bi => bi.LibraryMember)
                    .ThenInclude(lm => lm.User)
                .Where(bi => bi.IsActive)
                .OrderByDescending(bi => bi.CreatedAt)
                .Take(5)
                .Select(bi => MapBookIssueToDTO(bi))
                .ToListAsync();

            // Get overdue books
            var overdueBooks = await _context.BookIssues
                .Include(bi => bi.Book)
                .Include(bi => bi.LibraryMember)
                    .ThenInclude(lm => lm.User)
                .Where(bi => bi.IsActive && !bi.IsReturned && bi.DueDate < DateTime.Now)
                .OrderBy(bi => bi.DueDate)
                .Take(10)
                .Select(bi => MapBookIssueToDTO(bi))
                .ToListAsync();

            // Get pending fines
            var pendingFinesList = await _context.LibraryFines
                .Include(lf => lf.LibraryMember)
                    .ThenInclude(lm => lm.User)
                .Include(lf => lf.BookIssue)
                    .ThenInclude(bi => bi.Book)
                .Where(lf => lf.Status == "Pending")
                .OrderByDescending(lf => lf.Amount)
                .Take(10)
                .Select(lf => MapLibraryFineToDTO(lf))
                .ToListAsync();

            return new LibraryDashboardDTO
            {
                TotalBooks = totalBooks,
                AvailableBooks = availableBooks,
                IssuedBooks = issuedBooks,
                ReservedBooks = reservedBooks,
                TotalMembers = totalMembers,
                ActiveMembers = activeMembers,
                SuspendedMembers = suspendedMembers,
                TotalIssues = totalIssues,
                OverdueIssues = overdueIssues,
                TodayIssues = todayIssues,
                TodayReturns = todayReturns,
                TotalFines = totalFines,
                PendingFines = pendingFines,
                PendingReservations = pendingReservations,
                PopularBooks = popularBooks,
                RecentIssues = recentIssues,
                OverdueBooks = overdueBooks,
                PendingFinesList = pendingFinesList
            };
        }

        #endregion

        #region Book Category Management

        public async Task<IEnumerable<BookCategoryDTO>> GetAllBookCategoriesAsync()
        {
            var categories = await _context.BookCategories
                .Include(bc => bc.CreatedBy)
                .Include(bc => bc.Books)
                .OrderBy(bc => bc.Name)
                .ToListAsync();

            return categories.Select(MapBookCategoryToDTO);
        }

        public async Task<BookCategoryDTO?> GetBookCategoryByIdAsync(Guid id)
        {
            var category = await _context.BookCategories
                .Include(bc => bc.CreatedBy)
                .Include(bc => bc.Books)
                .FirstOrDefaultAsync(bc => bc.Id == id);

            return category != null ? MapBookCategoryToDTO(category) : null;
        }

        public async Task<BookCategoryDTO> CreateBookCategoryAsync(CreateBookCategoryDTO createDto, Guid createdByUserId)
        {
            // Check if code is unique
            if (await _context.BookCategories.AnyAsync(bc => bc.Code == createDto.Code))
            {
                throw new InvalidOperationException($"Category code '{createDto.Code}' already exists.");
            }

            var category = new BookCategory
            {
                Id = Guid.NewGuid(),
                Name = createDto.Name,
                Description = createDto.Description,
                Code = createDto.Code,
                ColorCode = createDto.ColorCode,
                MaxIssueDays = createDto.MaxIssueDays,
                MaxRenewalCount = createDto.MaxRenewalCount,
                IsReferenceOnly = createDto.IsReferenceOnly,
                FinePerDay = createDto.FinePerDay,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.BookCategories.Add(category);
            await _context.SaveChangesAsync();

            return await GetBookCategoryByIdAsync(category.Id) ?? throw new InvalidOperationException("Failed to create category.");
        }

        public async Task<BookCategoryDTO?> UpdateBookCategoryAsync(Guid id, UpdateBookCategoryDTO updateDto)
        {
            var category = await _context.BookCategories.FindAsync(id);
            if (category == null) return null;

            if (!string.IsNullOrEmpty(updateDto.Name))
                category.Name = updateDto.Name;

            if (updateDto.Description != null)
                category.Description = updateDto.Description;

            if (!string.IsNullOrEmpty(updateDto.ColorCode))
                category.ColorCode = updateDto.ColorCode;

            if (updateDto.IsActive.HasValue)
                category.IsActive = updateDto.IsActive.Value;

            if (updateDto.MaxIssueDays.HasValue)
                category.MaxIssueDays = updateDto.MaxIssueDays.Value;

            if (updateDto.MaxRenewalCount.HasValue)
                category.MaxRenewalCount = updateDto.MaxRenewalCount.Value;

            if (updateDto.IsReferenceOnly.HasValue)
                category.IsReferenceOnly = updateDto.IsReferenceOnly.Value;

            if (updateDto.FinePerDay.HasValue)
                category.FinePerDay = updateDto.FinePerDay.Value;

            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetBookCategoryByIdAsync(id);
        }

        public async Task<bool> DeleteBookCategoryAsync(Guid id)
        {
            var category = await _context.BookCategories
                .Include(bc => bc.Books)
                .FirstOrDefaultAsync(bc => bc.Id == id);

            if (category == null) return false;

            // Cannot delete if there are books in this category
            if (category.Books.Any())
            {
                throw new InvalidOperationException("Cannot delete category that contains books.");
            }

            _context.BookCategories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Author Management

        public async Task<IEnumerable<AuthorDTO>> GetAllAuthorsAsync()
        {
            var authors = await _context.Authors
                .Include(a => a.CreatedBy)
                .Include(a => a.BookAuthors)
                .Where(a => a.IsActive)
                .OrderBy(a => a.Name)
                .ToListAsync();

            return authors.Select(MapAuthorToDTO);
        }

        public async Task<AuthorDTO?> GetAuthorByIdAsync(Guid id)
        {
            var author = await _context.Authors
                .Include(a => a.CreatedBy)
                .Include(a => a.BookAuthors)
                .FirstOrDefaultAsync(a => a.Id == id);

            return author != null ? MapAuthorToDTO(author) : null;
        }

        public async Task<AuthorDTO> CreateAuthorAsync(CreateAuthorDTO createDto, Guid createdByUserId)
        {
            var author = new Author
            {
                Id = Guid.NewGuid(),
                Name = createDto.Name,
                Biography = createDto.Biography,
                Nationality = createDto.Nationality,
                DateOfBirth = createDto.DateOfBirth,
                DateOfDeath = createDto.DateOfDeath,
                Website = createDto.Website,
                Email = createDto.Email,
                PhotoUrl = createDto.PhotoUrl,
                Education = createDto.Education,
                Awards = createDto.Awards,
                NotableWorks = createDto.NotableWorks,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            return await GetAuthorByIdAsync(author.Id) ?? throw new InvalidOperationException("Failed to create author.");
        }

        public async Task<AuthorDTO?> UpdateAuthorAsync(Guid id, CreateAuthorDTO updateDto)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null) return null;

            author.Name = updateDto.Name;
            author.Biography = updateDto.Biography;
            author.Nationality = updateDto.Nationality;
            author.DateOfBirth = updateDto.DateOfBirth;
            author.DateOfDeath = updateDto.DateOfDeath;
            author.Website = updateDto.Website;
            author.Email = updateDto.Email;
            author.PhotoUrl = updateDto.PhotoUrl;
            author.Education = updateDto.Education;
            author.Awards = updateDto.Awards;
            author.NotableWorks = updateDto.NotableWorks;
            author.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetAuthorByIdAsync(id);
        }

        public async Task<bool> DeleteAuthorAsync(Guid id)
        {
            var author = await _context.Authors
                .Include(a => a.BookAuthors)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (author == null) return false;

            // Cannot delete if author has books
            if (author.BookAuthors.Any())
            {
                throw new InvalidOperationException("Cannot delete author who has books.");
            }

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<AuthorDTO>> SearchAuthorsAsync(string searchTerm)
        {
            var authors = await _context.Authors
                .Include(a => a.CreatedBy)
                .Include(a => a.BookAuthors)
                .Where(a => a.IsActive &&
                           (a.Name.Contains(searchTerm) ||
                            (a.Biography != null && a.Biography.Contains(searchTerm)) ||
                            (a.Nationality != null && a.Nationality.Contains(searchTerm))))
                .OrderBy(a => a.Name)
                .ToListAsync();

            return authors.Select(MapAuthorToDTO);
        }

        #endregion

        // Mapping methods will be added in the next part...
        private static BookCategoryDTO MapBookCategoryToDTO(BookCategory category)
        {
            return new BookCategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Code = category.Code,
                ColorCode = category.ColorCode,
                IsActive = category.IsActive,
                MaxIssueDays = category.MaxIssueDays,
                MaxRenewalCount = category.MaxRenewalCount,
                IsReferenceOnly = category.IsReferenceOnly,
                FinePerDay = category.FinePerDay,
                CreatedByName = category.CreatedBy?.FullName ?? string.Empty,
                CreatedAt = category.CreatedAt,
                TotalBooks = category.TotalBooks,
                AvailableBooks = category.AvailableBooks,
                IssuedBooks = category.IssuedBooks
            };
        }

        private static AuthorDTO MapAuthorToDTO(Author author)
        {
            return new AuthorDTO
            {
                Id = author.Id,
                Name = author.Name,
                Biography = author.Biography,
                Nationality = author.Nationality,
                DateOfBirth = author.DateOfBirth,
                DateOfDeath = author.DateOfDeath,
                Website = author.Website,
                Email = author.Email,
                PhotoUrl = author.PhotoUrl,
                Education = author.Education,
                Awards = author.Awards,
                NotableWorks = author.NotableWorks,
                IsActive = author.IsActive,
                CreatedAt = author.CreatedAt,
                IsAlive = author.IsAlive,
                Age = author.Age,
                TotalBooks = author.TotalBooks,
                ShortBio = author.ShortBio
            };
        }

        private static BookDTO MapBookToDTO(Book book)
        {
            var authors = (book.BookAuthors ?? new List<BookAuthor>())
                .Where(ba => ba.Author != null)
                .Select(ba => MapAuthorToDTO(ba.Author!))
                .ToList();

            return new BookDTO
            {
                Id = book.Id,
                Title = book.Title,
                Subtitle = book.Subtitle,
                ISBN = book.ISBN,
                ISBN13 = book.ISBN13,
                CategoryId = book.CategoryId,
                CategoryName = book.Category?.Name ?? string.Empty,
                PublisherId = book.PublisherId,
                PublisherName = book.Publisher?.Name ?? string.Empty,
                Edition = book.Edition,
                PublicationYear = book.PublicationYear,
                Language = book.Language,
                Pages = book.Pages,
                Format = book.Format,
                Description = book.Description,
                AccessionNumber = book.AccessionNumber,
                CallNumber = book.CallNumber,
                Location = book.Location,
                Barcode = book.Barcode,
                PurchasePrice = book.PurchasePrice,
                AcquisitionDate = book.AcquisitionDate,
                Condition = book.Condition,
                IsAvailable = book.IsAvailable,
                IsActive = book.IsActive,
                CoverImageUrl = book.CoverImageUrl,
                AverageRating = book.AverageRating,
                ReviewCount = book.ReviewCount,
                TotalIssues = book.TotalIssues,
                LastIssuedDate = book.LastIssuedDate,
                CreatedAt = book.CreatedAt,
                DisplayTitle = book.DisplayTitle,
                PrimaryAuthor = book.PrimaryAuthor,
                AllAuthors = book.AllAuthors,
                StatusDisplay = book.StatusDisplay,
                ConditionColor = book.ConditionColor,
                IsPopular = book.IsPopular,
                Authors = authors
            };
        }

        private static BookIssueDTO MapBookIssueToDTO(BookIssue bookIssue)
        {
            return new BookIssueDTO
            {
                Id = bookIssue.Id,
                BookId = bookIssue.BookId,
                BookTitle = bookIssue.Book?.Title ?? "Unknown Book",
                BookISBN = bookIssue.Book?.ISBN ?? string.Empty,
                LibraryMemberId = bookIssue.LibraryMemberId,
                MemberName = bookIssue.LibraryMember?.DisplayName ?? "Unknown Member",
                MembershipNumber = bookIssue.LibraryMember?.MembershipNumber ?? string.Empty,
                IssueNumber = bookIssue.IssueNumber,
                IssueDate = bookIssue.IssueDate,
                DueDate = bookIssue.DueDate,
                ReturnDate = bookIssue.ReturnDate,
                IsReturned = bookIssue.IsReturned,
                RenewalCount = bookIssue.RenewalCount,
                FineAmount = bookIssue.FineAmount,
                PendingFineAmount = bookIssue.PendingFineAmount,
                FineReason = bookIssue.FineReason,
                IssueCondition = bookIssue.IssueCondition,
                ReturnCondition = bookIssue.ReturnCondition,
                IsLost = bookIssue.IsLost,
                IsDamaged = bookIssue.IsDamaged,
                IssuedByName = bookIssue.IssuedBy?.FullName ?? string.Empty,
                CreatedAt = bookIssue.CreatedAt,
                IsOverdue = bookIssue.IsOverdue,
                DaysOverdue = bookIssue.DaysOverdue,
                DaysUntilDue = bookIssue.DaysUntilDue,
                Status = bookIssue.Status,
                StatusColor = bookIssue.StatusColor,
                CanBeRenewed = bookIssue.CanBeRenewed,
                NeedsAttention = bookIssue.NeedsAttention,
                AttentionReason = bookIssue.AttentionReason
            };
        }

        private static LibraryFineDTO MapLibraryFineToDTO(LibraryFine fine)
        {
            return new LibraryFineDTO
            {
                Id = fine.Id,
                LibraryMemberId = fine.LibraryMemberId,
                MemberName = fine.LibraryMember?.DisplayName ?? "Unknown Member",
                BookIssueId = fine.BookIssueId,
                BookTitle = fine.BookIssue?.Book?.Title,
                FineNumber = fine.FineNumber,
                FineType = fine.FineType,
                Amount = fine.Amount,
                PaidAmount = fine.PaidAmount,
                PendingAmount = fine.PendingAmount,
                Status = fine.Status,
                FineDate = fine.FineDate,
                DueDate = fine.DueDate,
                PaidDate = fine.PaidDate,
                Description = fine.Description,
                Reason = fine.Reason,
                OverdueDays = fine.OverdueDays,
                IsWaived = fine.IsWaived,
                WaiverReason = fine.WaiverReason,
                CreatedAt = fine.CreatedAt,
                IsPending = fine.IsPending,
                IsPaid = fine.IsPaid,
                IsOverdue = fine.IsOverdue,
                StatusColor = fine.StatusColor,
                FineTypeDisplay = fine.FineTypeDisplay,
                PaymentStatus = fine.PaymentStatus,
                NeedsAttention = fine.NeedsAttention
            };
        }

        private static PublisherDTO MapPublisherToDTO(Publisher publisher)
        {
            return new PublisherDTO
            {
                Id = publisher.Id,
                Name = publisher.Name,
                Description = publisher.Description,
                Address = publisher.Address,
                City = publisher.City,
                State = publisher.State,
                Country = publisher.Country,
                PostalCode = publisher.PostalCode,
                Phone = publisher.Phone,
                Email = publisher.Email,
                Website = publisher.Website,
                ContactPerson = publisher.ContactPerson,
                EstablishedDate = publisher.EstablishedDate,
                Specialization = publisher.Specialization,
                IsActive = publisher.IsActive,
                Rating = publisher.Rating,
                CreatedAt = publisher.CreatedAt,
                TotalBooks = publisher.TotalBooks,
                FullAddress = publisher.FullAddress,
                PrimaryContact = publisher.PrimaryContact
            };
        }

        private static LibraryMemberDTO MapLibraryMemberToDTO(LibraryMember member)
        {
            return new LibraryMemberDTO
            {
                Id = member.Id,
                UserId = member.UserId,
                UserName = member.User?.FullName ?? "Unknown User",
                MembershipNumber = member.MembershipNumber,
                MemberType = member.MemberType,
                MembershipStartDate = member.MembershipStartDate,
                MembershipEndDate = member.MembershipEndDate,
                IsActive = member.IsActive,
                IsSuspended = member.IsSuspended,
                MaxBooksAllowed = member.MaxBooksAllowed,
                CurrentBooksIssued = member.CurrentBooksIssued,
                TotalBooksIssued = member.TotalBooksIssued,
                PendingFineAmount = member.PendingFineAmount,
                Phone = member.Phone,
                Email = member.Email,
                CreatedAt = member.CreatedAt,
                CanIssueBooks = member.CanIssueBooks,
                HasOverdueBooks = member.HasOverdueBooks,
                MembershipStatus = member.MembershipStatus,
                ContactInfo = member.ContactInfo,
                AvailableBookSlots = member.AvailableBookSlots,
                IsMembershipExpired = member.IsMembershipExpired,
                MemberTypeDisplay = member.MemberTypeDisplay
            };
        }

        private static BookReservationDTO MapBookReservationToDTO(BookReservation reservation)
        {
            return new BookReservationDTO
            {
                Id = reservation.Id,
                BookId = reservation.BookId,
                BookTitle = reservation.Book?.Title ?? "Unknown Book",
                LibraryMemberId = reservation.LibraryMemberId,
                MemberName = reservation.LibraryMember?.DisplayName ?? "Unknown Member",
                ReservationNumber = reservation.ReservationNumber,
                ReservationDate = reservation.ReservationDate,
                ExpiryDate = reservation.ExpiryDate,
                Status = reservation.Status,
                Priority = reservation.Priority,
                FulfilledDate = reservation.FulfilledDate,
                FulfilledByName = reservation.FulfilledBy?.FullName,
                CreatedAt = reservation.CreatedAt,
                IsActive = reservation.IsActive,
                IsExpired = reservation.IsExpired,
                DaysUntilExpiry = reservation.DaysUntilExpiry,
                StatusColor = reservation.StatusColor,
                QueuePosition = reservation.QueuePosition,
                CanBeFulfilled = reservation.CanBeFulfilled,
                CanBeCancelled = reservation.CanBeCancelled,
                EstimatedWaitTime = reservation.EstimatedWaitTime
            };
        }

        public async Task<IEnumerable<PublisherDTO>> GetAllPublishersAsync()
        {
            var publishers = await _context.Publishers
                .Include(p => p.Books)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
            return publishers.Select(MapPublisherToDTO);
        }

        public async Task<PublisherDTO?> GetPublisherByIdAsync(Guid id)
        {
            var publisher = await _context.Publishers
                .Include(p => p.Books)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
            return publisher == null ? null : MapPublisherToDTO(publisher);
        }

        public async Task<PublisherDTO> CreatePublisherAsync(CreatePublisherDTO createDto, Guid createdByUserId)
        {
            var publisher = new Publisher
            {
                Id = Guid.NewGuid(),
                Name = createDto.Name,
                Description = createDto.Description,
                Address = createDto.Address,
                City = createDto.City,
                State = createDto.State,
                Country = createDto.Country,
                PostalCode = createDto.PostalCode,
                Phone = createDto.Phone,
                Email = createDto.Email,
                Website = createDto.Website,
                ContactPerson = createDto.ContactPerson,
                EstablishedDate = createDto.EstablishedDate,
                Specialization = createDto.Specialization,
                IsActive = true,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Publishers.Add(publisher);
            await _context.SaveChangesAsync();
            return MapPublisherToDTO(publisher);
        }

        public async Task<PublisherDTO?> UpdatePublisherAsync(Guid id, CreatePublisherDTO updateDto)
        {
            var publisher = await _context.Publishers.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
            if (publisher == null) return null;
            publisher.Name = updateDto.Name;
            publisher.Description = updateDto.Description;
            publisher.Address = updateDto.Address;
            publisher.City = updateDto.City;
            publisher.State = updateDto.State;
            publisher.Country = updateDto.Country;
            publisher.PostalCode = updateDto.PostalCode;
            publisher.Phone = updateDto.Phone;
            publisher.Email = updateDto.Email;
            publisher.Website = updateDto.Website;
            publisher.ContactPerson = updateDto.ContactPerson;
            publisher.EstablishedDate = updateDto.EstablishedDate;
            publisher.Specialization = updateDto.Specialization;
            publisher.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return MapPublisherToDTO(publisher);
        }

        public async Task<bool> DeletePublisherAsync(Guid id)
        {
            var publisher = await _context.Publishers.Include(p => p.Books).FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
            if (publisher == null) return false;
            if (publisher.Books.Any(b => b.IsActive)) throw new InvalidOperationException("Cannot delete publisher with active books.");
            publisher.IsActive = false;
            publisher.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<PublisherDTO>> SearchPublishersAsync(string searchTerm)
        {
            var publishers = await _context.Publishers
                .Include(p => p.Books)
                .Where(p => p.IsActive && (p.Name.Contains(searchTerm) || (p.Description != null && p.Description.Contains(searchTerm))))
                .OrderBy(p => p.Name)
                .ToListAsync();
            return publishers.Select(MapPublisherToDTO);
        }

        public async Task<IEnumerable<BookDTO>> GetAllBooksAsync()
        {
            var books = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
                .Where(b => b.IsActive)
                .OrderBy(b => b.Title)
                .ToListAsync();
            return books.Select(MapBookToDTO);
        }

        public async Task<BookDTO?> GetBookByIdAsync(Guid id)
        {
            var book = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
                .FirstOrDefaultAsync(b => b.Id == id && b.IsActive);
            return book == null ? null : MapBookToDTO(book);
        }

        public async Task<BookDTO?> GetBookByISBNAsync(string isbn)
        {
            var book = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
                .FirstOrDefaultAsync(b => b.ISBN == isbn && b.IsActive);
            return book == null ? null : MapBookToDTO(book);
        }

        public async Task<BookDTO?> GetBookByAccessionNumberAsync(string accessionNumber)
        {
            var book = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
                .FirstOrDefaultAsync(b => b.AccessionNumber == accessionNumber && b.IsActive);
            return book == null ? null : MapBookToDTO(book);
        }
        public Task<BookDTO> CreateBookAsync(CreateBookDTO createDto, Guid createdByUserId) => throw new NotImplementedException();
        public Task<BookDTO?> UpdateBookAsync(Guid id, CreateBookDTO updateDto) => throw new NotImplementedException();
        public Task<bool> DeleteBookAsync(Guid id) => throw new NotImplementedException();
        public Task<IEnumerable<BookDTO>> SearchBooksAsync(string searchTerm) => throw new NotImplementedException();
        public Task<IEnumerable<BookDTO>> GetBooksByCategoryAsync(Guid categoryId) => throw new NotImplementedException();
        public Task<IEnumerable<BookDTO>> GetBooksByAuthorAsync(Guid authorId) => throw new NotImplementedException();
        public Task<IEnumerable<BookDTO>> GetBooksByPublisherAsync(Guid publisherId) => throw new NotImplementedException();
        public async Task<IEnumerable<BookDTO>> GetAvailableBooksAsync()
        {
            var books = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
                .Where(b => b.IsActive && b.IsAvailable)
                .OrderBy(b => b.Title)
                .ToListAsync();
            return books.Select(MapBookToDTO);
        }

        public async Task<IEnumerable<BookDTO>> GetPopularBooksAsync(int count = 10)
        {
            var books = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.TotalIssues)
                .Take(count)
                .ToListAsync();
            return books.Select(MapBookToDTO);
        }

        public async Task<IEnumerable<LibraryMemberDTO>> GetAllLibraryMembersAsync()
        {
            var members = await _context.LibraryMembers
                .Include(m => m.User)
                .Include(m => m.BookIssues)
                .Where(m => m.IsActive)
                .OrderBy(m => m.MembershipNumber)
                .ToListAsync();
            return members.Select(MapLibraryMemberToDTO);
        }

        public async Task<LibraryMemberDTO?> GetLibraryMemberByIdAsync(Guid id)
        {
            var member = await _context.LibraryMembers
                .Include(m => m.User)
                .Include(m => m.BookIssues)
                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);
            return member == null ? null : MapLibraryMemberToDTO(member);
        }

        public async Task<LibraryMemberDTO?> GetLibraryMemberByUserIdAsync(Guid userId)
        {
            var member = await _context.LibraryMembers
                .Include(m => m.User)
                .Include(m => m.BookIssues)
                .FirstOrDefaultAsync(m => m.UserId == userId && m.IsActive);
            return member == null ? null : MapLibraryMemberToDTO(member);
        }

        public async Task<LibraryMemberDTO?> GetLibraryMemberByMembershipNumberAsync(string membershipNumber)
        {
            var member = await _context.LibraryMembers
                .Include(m => m.User)
                .Include(m => m.BookIssues)
                .FirstOrDefaultAsync(m => m.MembershipNumber == membershipNumber && m.IsActive);
            return member == null ? null : MapLibraryMemberToDTO(member);
        }
        public Task<LibraryMemberDTO> CreateLibraryMemberAsync(CreateLibraryMemberDTO createDto, Guid createdByUserId) => throw new NotImplementedException();
        public Task<LibraryMemberDTO?> UpdateLibraryMemberAsync(Guid id, CreateLibraryMemberDTO updateDto) => throw new NotImplementedException();
        public Task<bool> DeleteLibraryMemberAsync(Guid id) => throw new NotImplementedException();
        public Task<bool> SuspendLibraryMemberAsync(Guid id, string reason, DateTime? suspensionEndDate = null) => throw new NotImplementedException();
        public Task<bool> ReactivateLibraryMemberAsync(Guid id) => throw new NotImplementedException();
        public async Task<IEnumerable<BookIssueDTO>> GetAllBookIssuesAsync()
        {
            var issues = await _context.BookIssues
                .Include(i => i.Book)
                .Include(i => i.LibraryMember).ThenInclude(m => m.User)
                .Include(i => i.IssuedBy)
                .Where(i => i.IsActive)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();
            return issues.Select(MapBookIssueToDTO);
        }

        /// <summary>
        /// Flat row returned by dbo.sp_BookIssues_GetPaged: raw BookIssue columns plus the joined
        /// Book / LibraryMember / member User / issuer User columns needed to rebuild the entity graph.
        /// </summary>
        private sealed class BookIssuePagedRow
        {
            public Guid Id { get; set; }
            public Guid BookId { get; set; }
            public Guid LibraryMemberId { get; set; }
            public Guid AcademicYearId { get; set; }
            public string IssueNumber { get; set; } = string.Empty;
            public DateTime IssueDate { get; set; }
            public DateTime DueDate { get; set; }
            public DateTime? ReturnDate { get; set; }
            public bool IsReturned { get; set; }
            public bool IsActive { get; set; }
            public int RenewalCount { get; set; }
            public bool HasFine { get; set; }
            public decimal FineAmount { get; set; }
            public decimal PaidFineAmount { get; set; }
            public decimal PendingFineAmount { get; set; }
            public string? FineReason { get; set; }
            public string IssueCondition { get; set; } = string.Empty;
            public string? ReturnCondition { get; set; }
            public bool IsLost { get; set; }
            public bool IsDamaged { get; set; }
            public Guid IssuedByUserId { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public string BookTitle { get; set; } = string.Empty;
            public string BookISBN { get; set; } = string.Empty;
            public Guid MemberUserId { get; set; }
            public string MembershipNumber { get; set; } = string.Empty;
            public bool MemberIsActive { get; set; }
            public bool MemberIsSuspended { get; set; }
            public int MemberMaxBooksAllowed { get; set; }
            public int MemberMaxIssueDays { get; set; }
            public int MemberMaxRenewalCount { get; set; }
            public int MemberCurrentBooksIssued { get; set; }
            public decimal MemberPendingFineAmount { get; set; }
            public string MemberFirstName { get; set; } = string.Empty;
            public string MemberLastName { get; set; } = string.Empty;
            public string IssuedByFirstName { get; set; } = string.Empty;
            public string IssuedByLastName { get; set; } = string.Empty;
        }

        public async Task<PagedResult<BookIssueDTO>> GetBookIssuesPagedAsync(
            int pageNumber, int pageSize, string? searchTerm = null, Guid? libraryMemberId = null, bool? isReturned = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchTerm", string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim());
            parameters.Add("@LibraryMemberId", libraryMemberId);
            parameters.Add("@IsReturned", isReturned);
            // Preserve the EF academic-year global query filter (BookIssue + LibraryMember).
            parameters.Add("@AcademicYearId", _context.CurrentAcademicYearId);

            var (rows, totalCount) = await _sqlDataAccess.QueryPagedStoredProcAsync<BookIssuePagedRow>(
                "dbo.sp_BookIssues_GetPaged", parameters);

            // Rebuild the entity graph so the entity's computed properties (IsOverdue, Status,
            // CanBeRenewed, NeedsAttention, ...) and the existing MapBookIssueToDTO are reused as-is.
            var items = rows.Select(r => MapBookIssueToDTO(new BookIssue
            {
                Id = r.Id,
                BookId = r.BookId,
                LibraryMemberId = r.LibraryMemberId,
                AcademicYearId = r.AcademicYearId,
                IssueNumber = r.IssueNumber,
                IssueDate = r.IssueDate,
                DueDate = r.DueDate,
                ReturnDate = r.ReturnDate,
                IsReturned = r.IsReturned,
                IsActive = r.IsActive,
                RenewalCount = r.RenewalCount,
                HasFine = r.HasFine,
                FineAmount = r.FineAmount,
                PaidFineAmount = r.PaidFineAmount,
                PendingFineAmount = r.PendingFineAmount,
                FineReason = r.FineReason,
                IssueCondition = r.IssueCondition,
                ReturnCondition = r.ReturnCondition,
                IsLost = r.IsLost,
                IsDamaged = r.IsDamaged,
                IssuedByUserId = r.IssuedByUserId,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                Book = new Book { Id = r.BookId, Title = r.BookTitle, ISBN = r.BookISBN },
                LibraryMember = new LibraryMember
                {
                    Id = r.LibraryMemberId,
                    UserId = r.MemberUserId,
                    MembershipNumber = r.MembershipNumber,
                    IsActive = r.MemberIsActive,
                    IsSuspended = r.MemberIsSuspended,
                    MaxBooksAllowed = r.MemberMaxBooksAllowed,
                    MaxIssueDays = r.MemberMaxIssueDays,
                    MaxRenewalCount = r.MemberMaxRenewalCount,
                    CurrentBooksIssued = r.MemberCurrentBooksIssued,
                    PendingFineAmount = r.MemberPendingFineAmount,
                    User = new User { Id = r.MemberUserId, FirstName = r.MemberFirstName, LastName = r.MemberLastName }
                },
                IssuedBy = new User { Id = r.IssuedByUserId, FirstName = r.IssuedByFirstName, LastName = r.IssuedByLastName }
            })).ToList();

            return new PagedResult<BookIssueDTO>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            };
        }

        public async Task<BookIssueDTO?> GetBookIssueByIdAsync(Guid id)
        {
            var issue = await _context.BookIssues
                .Include(i => i.Book)
                .Include(i => i.LibraryMember).ThenInclude(m => m.User)
                .Include(i => i.IssuedBy)
                .FirstOrDefaultAsync(i => i.Id == id && i.IsActive);
            return issue == null ? null : MapBookIssueToDTO(issue);
        }
        public Task<IEnumerable<BookIssueDTO>> GetBookIssuesByMemberAsync(Guid memberId) => throw new NotImplementedException();
        public Task<IEnumerable<BookIssueDTO>> GetBookIssuesByBookAsync(Guid bookId) => throw new NotImplementedException();
        public Task<IEnumerable<BookIssueDTO>> GetActiveBookIssuesAsync() => throw new NotImplementedException();
        public Task<IEnumerable<BookIssueDTO>> GetOverdueBookIssuesAsync() => throw new NotImplementedException();
        public Task<BookIssueDTO> IssueBookAsync(CreateBookIssueDTO createDto, Guid issuedByUserId) => throw new NotImplementedException();
        public Task<BookIssueDTO?> ReturnBookAsync(Guid issueId, string? returnCondition = null, string? returnNotes = null, Guid? returnedByUserId = null) => throw new NotImplementedException();
        public Task<BookIssueDTO?> RenewBookAsync(Guid issueId, int? additionalDays = null) => throw new NotImplementedException();
        public Task<bool> ReportLostBookAsync(Guid issueId, string description, decimal? replacementCost = null) => throw new NotImplementedException();
        public Task<bool> ReportDamagedBookAsync(Guid issueId, string description, decimal? repairCost = null) => throw new NotImplementedException();
        public async Task<IEnumerable<BookReservationDTO>> GetAllBookReservationsAsync()
        {
            var reservations = await _context.BookReservations
                .Include(r => r.Book)
                .Include(r => r.LibraryMember).ThenInclude(m => m.User)
                .Include(r => r.FulfilledBy)
                .Where(r => r.Status != "Deleted")
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();
            return reservations.Select(MapBookReservationToDTO);
        }

        public async Task<BookReservationDTO?> GetBookReservationByIdAsync(Guid id)
        {
            var reservation = await _context.BookReservations
                .Include(r => r.Book)
                .Include(r => r.LibraryMember).ThenInclude(m => m.User)
                .Include(r => r.FulfilledBy)
                .FirstOrDefaultAsync(r => r.Id == id);
            return reservation == null ? null : MapBookReservationToDTO(reservation);
        }
        public Task<IEnumerable<BookReservationDTO>> GetBookReservationsByMemberAsync(Guid memberId) => throw new NotImplementedException();
        public Task<IEnumerable<BookReservationDTO>> GetBookReservationsByBookAsync(Guid bookId) => throw new NotImplementedException();
        public Task<IEnumerable<BookReservationDTO>> GetActiveBookReservationsAsync() => throw new NotImplementedException();
        public Task<BookReservationDTO> CreateBookReservationAsync(CreateBookReservationDTO createDto, Guid createdByUserId) => throw new NotImplementedException();
        public Task<bool> CancelBookReservationAsync(Guid id, string? reason = null, Guid? cancelledByUserId = null) => throw new NotImplementedException();
        public Task<BookIssueDTO?> FulfillBookReservationAsync(Guid reservationId, Guid fulfilledByUserId) => throw new NotImplementedException();
        public async Task<IEnumerable<LibraryFineDTO>> GetAllLibraryFinesAsync()
        {
            var fines = await _context.LibraryFines
                .Include(f => f.LibraryMember).ThenInclude(m => m.User)
                .Include(f => f.BookIssue).ThenInclude(i => i.Book)
                .OrderByDescending(f => f.FineDate)
                .ToListAsync();
            return fines.Select(MapLibraryFineToDTO);
        }

        public async Task<LibraryFineDTO?> GetLibraryFineByIdAsync(Guid id)
        {
            var fine = await _context.LibraryFines
                .Include(f => f.LibraryMember).ThenInclude(m => m.User)
                .Include(f => f.BookIssue).ThenInclude(i => i.Book)
                .FirstOrDefaultAsync(f => f.Id == id);
            return fine == null ? null : MapLibraryFineToDTO(fine);
        }
        public Task<IEnumerable<LibraryFineDTO>> GetLibraryFinesByMemberAsync(Guid memberId) => throw new NotImplementedException();
        public Task<IEnumerable<LibraryFineDTO>> GetPendingLibraryFinesAsync() => throw new NotImplementedException();
        public Task<bool> PayLibraryFineAsync(Guid fineId, decimal amount, string paymentMethod, string? paymentReference = null, Guid? receivedByUserId = null) => throw new NotImplementedException();
        public Task<bool> WaiveLibraryFineAsync(Guid fineId, string reason, Guid waivedByUserId) => throw new NotImplementedException();
        public Task<bool> CalculateOverdueFinesAsync() => throw new NotImplementedException();
        public Task<bool> CanIssueBookAsync(Guid bookId, Guid memberId) => throw new NotImplementedException();
        public Task<bool> CanReserveBookAsync(Guid bookId, Guid memberId) => throw new NotImplementedException();
        public Task<bool> IsISBNUniqueAsync(string isbn, Guid? excludeBookId = null) => throw new NotImplementedException();
        public Task<bool> IsAccessionNumberUniqueAsync(string accessionNumber, Guid? excludeBookId = null) => throw new NotImplementedException();
        public Task<bool> IsMembershipNumberUniqueAsync(string membershipNumber, Guid? excludeMemberId = null) => throw new NotImplementedException();
    }
}
