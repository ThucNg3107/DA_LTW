using Microsoft.AspNetCore.Authorization; // Dùng để xử lý quyền truy cập
using Microsoft.AspNetCore.Mvc; // Dùng cho các Action và Controller
using Microsoft.AspNetCore.Mvc.Rendering; // Dùng cho SelectListItem (dùng cho dropdown list)
using QuanLyThuVien.Areas.Admin.Models; // Các model liên quan đến Admin
using QuanLyThuVien.Models; // Các model chung
using QuanLyThuVien.Repositories; // Các interface repository
using System.Linq; // Dùng để truy vấn dữ liệu
using System.Threading.Tasks; // Dùng cho xử lý các tác vụ bất đồng bộ

namespace QuanLyThuVien.Areas.Admin.Controllers // Định nghĩa controller trong khu vực Admin
{
    [Area("Admin")] // Đánh dấu controller này thuộc khu vực "Admin"
    [Authorize(Roles = SD.Role_Admin)] // Chỉ người có quyền Admin mới có thể truy cập
    public class LoanController : Controller // Controller quản lý việc mượn sách
    {
        private readonly ILoanRepository _loanRepository; // Repository quản lý việc mượn sách
        private readonly IBookRepository _bookRepository; // Repository quản lý sách
        private readonly ILibraryCardRepository _libraryCardRepository; // Repository quản lý thẻ thư viện

        // Constructor: Tiêm các repository vào controller
        public LoanController(ILoanRepository loanRepository, IBookRepository bookRepository, ILibraryCardRepository libraryCardRepository)
        {
            _loanRepository = loanRepository; // Gán repository quản lý mượn sách
            _bookRepository = bookRepository; // Gán repository quản lý sách
            _libraryCardRepository = libraryCardRepository; // Gán repository quản lý thẻ thư viện
        }

        // Hiển thị tất cả các khoản mượn sách
        public async Task<IActionResult> Index()
        {
            var loans = await _loanRepository.GetAllAsync(); // Lấy tất cả các khoản mượn từ DB
            return View(loans); // Trả về view với danh sách các khoản mượn
        }

        // Hiển thị chi tiết một khoản mượn sách theo ID
        public async Task<IActionResult> Details(int id)
        {
            var loan = await _loanRepository.GetByIdAsync(id); // Lấy thông tin khoản mượn theo ID
            if (loan == null)
            {
                return NotFound(); // Nếu không tìm thấy, trả về lỗi 404
            }
            return View(loan); // Trả về view với thông tin chi tiết khoản mượn
        }

        // Hiển thị form tạo mới khoản mượn
        public async Task<IActionResult> Create()
        {
            var books = await _bookRepository.GetAllAsync(); // Lấy tất cả sách
            var libraryCards = await _libraryCardRepository.GetAllAsync(); // Lấy tất cả thẻ thư viện

            // Tạo danh sách sách cho dropdown list
            ViewBag.Books = books.Select(b => new SelectListItem
            {
                Value = b.BookId.ToString(),
                Text = $"{b.Name} - {b.Author}" // Hiển thị tên và tác giả
            }).ToList();

            // Tạo danh sách thẻ thư viện cho dropdown list
            ViewBag.LibraryCards = libraryCards.Select(c => new SelectListItem
            {
                Value = c.CardId.ToString(),
                Text = $"{c.FullName} - {c.StudentID}" // Hiển thị tên và mã sinh viên
            }).ToList();

            return View(); // Trả về view tạo mới
        }

        // Xử lý khi gửi form tạo mới khoản mượn
        [HttpPost]
        [ValidateAntiForgeryToken] // Chống các cuộc tấn công CSRF
        public async Task<IActionResult> Create(Loan loan)
        {
            if (!ModelState.IsValid) // Kiểm tra tính hợp lệ của dữ liệu form
            {
                var books = await _bookRepository.GetAllAsync(); // Lấy lại danh sách sách nếu dữ liệu không hợp lệ
                var libraryCards = await _libraryCardRepository.GetAllAsync(); // Lấy lại danh sách thẻ thư viện

                ViewBag.Books = books.Select(b => new SelectListItem
                {
                    Value = b.BookId.ToString(),
                    Text = $"{b.Name} - {b.Author}"
                }).ToList();

                ViewBag.LibraryCards = libraryCards.Select(c => new SelectListItem
                {
                    Value = c.CardId.ToString(),
                    Text = $"{c.FullName} - {c.StudentID}"
                }).ToList();

                return View(loan); // Trả về form tạo mới với dữ liệu không hợp lệ
            }

            await _loanRepository.AddAsync(loan); // Thêm khoản mượn mới vào DB
            return RedirectToAction(nameof(Index)); // Quay lại trang danh sách các khoản mượn
        }

        // Hiển thị form chỉnh sửa một khoản mượn
        public async Task<IActionResult> Edit(int id)
        {
            var loan = await _loanRepository.GetByIdAsync(id); // Lấy thông tin khoản mượn theo ID
            if (loan == null)
            {
                return NotFound(); // Nếu không tìm thấy, trả về lỗi 404
            }

            // Lấy lại danh sách sách và thẻ thư viện cho dropdown list
            var books = await _bookRepository.GetAllAsync();
            var libraryCards = await _libraryCardRepository.GetAllAsync();

            ViewBag.Books = books.Select(b => new SelectListItem
            {
                Value = b.BookId.ToString(),
                Text = $"{b.Name} - {b.Author}"
            }).ToList();

            ViewBag.LibraryCards = libraryCards.Select(c => new SelectListItem
            {
                Value = c.CardId.ToString(),
                Text = $"{c.FullName} - {c.StudentID}"
            }).ToList();

            return View(loan); // Trả về form chỉnh sửa với dữ liệu của khoản mượn
        }

        // Xử lý khi gửi form chỉnh sửa khoản mượn
        [HttpPost]
        [ValidateAntiForgeryToken] // Chống các cuộc tấn công CSRF
        public async Task<IActionResult> Edit(int id, Loan loan)
        {
            if (id != loan.LoanId) // Kiểm tra ID của khoản mượn có hợp lệ không
            {
                return BadRequest(); // Nếu không hợp lệ, trả về lỗi 400
            }

            if (!ModelState.IsValid) // Kiểm tra tính hợp lệ của dữ liệu form
            {
                var books = await _bookRepository.GetAllAsync(); // Lấy lại danh sách sách
                var libraryCards = await _libraryCardRepository.GetAllAsync(); // Lấy lại danh sách thẻ thư viện

                ViewBag.Books = books.Select(b => new SelectListItem
                {
                    Value = b.BookId.ToString(),
                    Text = $"{b.Name} - {b.Author}"
                }).ToList();

                ViewBag.LibraryCards = libraryCards.Select(c => new SelectListItem
                {
                    Value = c.CardId.ToString(),
                    Text = $"{c.FullName} - {c.StudentID}"
                }).ToList();

                return View(loan); // Trả về form chỉnh sửa nếu dữ liệu không hợp lệ
            }

            await _loanRepository.UpdateAsync(loan); // Cập nhật thông tin khoản mượn trong DB
            return RedirectToAction(nameof(Index)); // Quay lại trang danh sách
        }

        // Hiển thị form xác nhận xóa khoản mượn
        public async Task<IActionResult> Delete(int id)
        {
            var loan = await _loanRepository.GetByIdAsync(id); // Lấy thông tin khoản mượn theo ID
            if (loan == null)
            {
                return NotFound(); // Nếu không tìm thấy, trả về lỗi 404
            }
            return View(loan); // Trả về form xác nhận xóa
        }

        // Xử lý khi người dùng xác nhận xóa khoản mượn
        [HttpPost, ActionName("Delete")] // Xác nhận xóa bằng phương thức POST
        [ValidateAntiForgeryToken] // Chống các cuộc tấn công CSRF
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _loanRepository.DeleteAsync(id); // Xóa khoản mượn khỏi DB
            return RedirectToAction(nameof(Index)); // Quay lại trang danh sách
        }
    }
}
