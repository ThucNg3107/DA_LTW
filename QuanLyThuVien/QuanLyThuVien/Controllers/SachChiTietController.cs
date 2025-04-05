using Microsoft.AspNetCore.Mvc;
using QuanLyThuVien.Models;
using QuanLyThuVien.Repositories;

public class SachChiTietController : Controller
{
    private readonly IBookRepository _bookRepository;
    private readonly ICategoryRepository _categoryRepository;

    public SachChiTietController(IBookRepository bookRepository, ICategoryRepository categoryRepository)
    {
        _bookRepository = bookRepository;
        _categoryRepository = categoryRepository;
    }

    // Action Index để hiển thị danh sách sách và hỗ trợ tìm kiếm
    public async Task<IActionResult> Index(int? categoryId)
    {
        // Lấy tất cả sách từ repository
        var allBooks = await _bookRepository.GetAllAsync();
        var books = allBooks.AsQueryable();

        // Lọc theo category nếu được chọn
        if (categoryId.HasValue)
        {
            books = books.Where(b => b.CategoryId == categoryId.Value);
            ViewData["CategoryId"] = categoryId;
        }

        // Lấy danh sách thể loại để hiển thị trên dropdown tìm kiếm
        var categories = await _categoryRepository.GetAllAsync();
        ViewBag.Categories = categories;

        // Trả về danh sách sách dưới dạng danh sách (List)
        return View(books.ToList());
    }

    // Action để xem chi tiết sách
    public async Task<IActionResult> Details(int id)
    {
        var book = await _bookRepository.GetByIdAsync(id);

        if (book == null)
        {
            return NotFound();
        }

        return View(book);
    }

    // Action để mượn sách
    [HttpPost]
    public async Task<IActionResult> RentBook(int bookId)
    {
        var book = await _bookRepository.GetByIdAsync(bookId);

        if (book == null)
        {
            TempData["Error"] = "Không tìm thấy sách.";
            return RedirectToAction(nameof(Index));
        }

        if (book.BookStatus != Book.Status.Available)
        {
            TempData["Error"] = "Sách này không có sẵn để mượn.";
            return RedirectToAction(nameof(Index));
        }

        // Cập nhật trạng thái sách
        book.BookStatus = Book.Status.Borrowed;
        await _bookRepository.UpdateAsync(book);

        TempData["Success"] = "Bạn đã mượn sách thành công!";
        return RedirectToAction(nameof(Index));
    }
}