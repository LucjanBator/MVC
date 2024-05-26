using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MvcLibrary.Models;
using MvcLibrary.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;
using OfficeOpenXml;
using CsvHelper;
using CsvHelper.Configuration;
namespace MvcLibrary.Controllers;
using System.Globalization;
public struct ReaderDisplayElement {
    public int ReaderId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int BorrowedBooksCount { get; set; }
    public int BooksCount { get; set; }
}

public struct BorrowingData {
    public int BorrowingId { get; set; }
    public int BookCopyId { get; set; }
    public int BookId { get; set; }
    public int ReaderId { get; set; }
    public string ReaderName { get; set; }
    public string ReaderSurname { get; set; }
    public DateTime BorrowingDate { get; set; }
    public DateTime? ReturnDate { get; set; }
}

public struct BookDisplayElement {
    public int BookId { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string Publisher { get; set; }
    public string Isbn { get; set; }
    public DateTime? PublicationDate { get; set; }
    public string BookCategory { get; set; }
    public int BookCategoryId { get; set; }
}

[Route("[controller]/")]
public class LibraryController : Controller {
    private readonly LibraryDBContext _db;

    public LibraryController(LibraryDBContext contextUser) {
        _db = contextUser;
    }

    private void SetViewDataFromSession() {
        if (HttpContext.Session.GetString("username") == null) {
            ViewData["Username"] = "";
            ViewData["IsAdmin"] = "";

            return;
        }

        ViewData["Username"] = HttpContext.Session.GetString("username");
        ViewData["IsAdmin"] = HttpContext.Session.GetString("isadmin");
    }

    public List<Pilkarze> ReadCsvFile(string filePath, char separator)
    {
        List<Pilkarze> data = new List<Pilkarze>();

        using (StreamReader reader = new StreamReader(filePath))
        {
            string headerLine = reader.ReadLine();
            string[] headerValues = headerLine.Split(separator);

            // Utwórz nowy obiekt Pilkarze z wartościami nagłówka
            var headerPilkarz = new Pilkarze
            {
                i = headerValues[0],
                n = headerValues[1],
                w = headerValues[2],
                r = headerValues[3],
                rr = headerValues[4]
            };

            // Dodaj obiekt nagłówka do listy
            data.Add(headerPilkarz);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(separator);

                var pilkarz = new Pilkarze
                {
                    imie = values[0],
                    nazwisko = values[1],
                    wiek = int.Parse(values[2]),
                    Rok_dolaczenia_do_klubu = int.Parse(values[3]),
                    Rok_zakonczenia_pracy_w_klubie = int.Parse(values[4])
                };

                data.Add(pilkarz);
            }
        }

        return data;
    }
    [Route("Sponsorzy/")]
    public IActionResult Sponsorzy()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Excel", "Sponsorzy.csv");
        List<Sponsorzy> data = new List<Sponsorzy>();

        if (System.IO.File.Exists(path))
        {
            data = ReadCsvFileS(path, ';');
        }

        return View(data);
    }
    public List<Sponsorzy> ReadCsvFileS(string filePath, char separator)
    {
        List<Sponsorzy> data = new List<Sponsorzy>();

        using (StreamReader reader = new StreamReader(filePath))
        {
            string headerLine = reader.ReadLine();
            string[] headerValues = headerLine.Split(separator);

            // Utwórz nowy obiekt Pilkarze z wartościami nagłówka
            var headerSponsor = new Sponsorzy
            {
                n = headerValues[0]
            };

            // Dodaj obiekt nagłówka do listy
            data.Add(headerSponsor);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(separator);

                var sponsor = new Sponsorzy
                {
                    Nazwa = values[0]
                };

                data.Add(sponsor);
            }
        }

        return data;
    }
    [Route("Pilkarze/")]
    public IActionResult Pilkarze()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Excel", "Piłkarze.csv");
        List<Pilkarze> data = new List<Pilkarze>();

        if (System.IO.File.Exists(path))
        {
            data = ReadCsvFile(path, ';');
        }

        return View(data);
    }

    public List<Trenerzy> ReadCsvFileT(string filePath, char separator)
    {
        List<Trenerzy> data = new List<Trenerzy>();

        using (StreamReader reader = new StreamReader(filePath))
        {
            string headerLine = reader.ReadLine();
            string[] headerValues = headerLine.Split(separator);

            // Utwórz nowy obiekt Pilkarze z wartościami nagłówka
            var headerTrenerzy = new Trenerzy
            {
                i = headerValues[0],
                n = headerValues[1],
                w = headerValues[2],
                r = headerValues[3],
                rr = headerValues[4]
            };

            // Dodaj obiekt nagłówka do listy
            data.Add(headerTrenerzy);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(separator);

                var Trener = new Trenerzy
                {
                    imie = values[0],
                    nazwisko = values[1],
                    wiek = int.Parse(values[2]),
                    Rok_dolaczenia_do_klubu = int.Parse(values[3]),
                    Rok_zakonczenia_pracy_w_klubie = int.Parse(values[4])
                };

                data.Add(Trener);
            }
        }

        return data;
    }

    [Route("Trenerzy/")]
    public IActionResult Trenerzy()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Excel", "Trenerzy.csv");
        List<Trenerzy> data = new List<Trenerzy>();

        if (System.IO.File.Exists(path))
        {
            data = ReadCsvFileT(path, ';');
        }

        return View(data);
    }

    [Route("editbook/{bookId:int}")]
    public IActionResult EditBook(int bookId) {
        if (HttpContext.Session.GetString("username") == null || HttpContext.Session.GetString("username") == "") {
            return RedirectToAction("Login", "User");
        }

        SetViewDataFromSession();

        BookModel ?book = (
            from bookModel in _db.Books where bookModel.BookId == bookId select bookModel
        ).FirstOrDefault();

        if (book is null) {
            return RedirectToAction("ListBooks");
        }

        BookCategoryModel []bookCategories = _db.BookCategories.ToArray();

        ViewData["BookCategories"] = bookCategories;

        ViewData["book-id"] = book.BookId;
        ViewData["title"] = book.Title;
        ViewData["author"] = book.Author;
        ViewData["description"] = book.Description;
        ViewData["publisher"] = book.Publisher;
        ViewData["isbn"] = book.Isbn;
        ViewData["publication-date"] = book.PublicationDate?.ToString("yyyy-MM-dd");
        ViewData["book-category-id"] = book.BookCategoryId;

        BookCopyModel[] bookCopies = (
            from bookCopy in _db.BookCopies where bookCopy.BookId == bookId select bookCopy
        ).ToArray();

        ViewData["BookCopies"] = bookCopies;

        return View();
    }

    [Route("editbook/{bookId:int}")]
    [HttpPost]
    public IActionResult EditBook(int bookId, IFormCollection form) {
        if (HttpContext.Session.GetString("username") == null || HttpContext.Session.GetString("username") == "") {
            return RedirectToAction("Login", "User");
        }

        SetViewDataFromSession();

        if(form["addnewcopy"] == "Add") {
            _db.BookCopies.Add(new BookCopyModel {
                BookId = bookId
            });
            
            _db.SaveChanges();

            ViewData["EditBookMessage"] = "Book copy added successfully";

            return RedirectToAction("EditBook", bookId);
        }

        BookModel ?book = (
            from bookModel in _db.Books where bookModel.BookId == bookId select bookModel
        ).FirstOrDefault();

        if (book is null) {
            return RedirectToAction("ListBooks");
        }

        List<String> fields = new List<String> {
            "title",
            "author",
            "description",
            "publisher",
            "isbn",
            "publication-date",
            "book-category-id"
        };

        foreach (String field in fields) {
            if (((String ?) form[field]) is null || (String ?) form[field] == "") {
                ViewData["EditBookMessage"] = "All fields are required";

                return View();
            }
        }

        try {
            List<BookCategoryModel> category = (
                from bookCategory in _db.BookCategories where bookCategory.BookCategoryId == Int32.Parse((String ?) form["book-category-id"] ?? "") select bookCategory
            ).ToList();

            if (category.Count() == 0) {
                ViewData["EditBookMessage"] = "Invalid category";

                return View();
            }

            book.Title = ((String ?) form["title"]) ?? "";
            book.Author = ((String ?) form["author"]) ?? "";
            book.Publisher = ((String ?) form["publisher"]) ?? "";
            book.Description = ((String ?) form["description"]) ?? "";
            book.Isbn = ((String ?) form["isbn"]) ?? "";
            book.PublicationDate = DateTime.Parse((String ?) form["publication-date"] ?? "");
            book.BookCategoryId = Int32.Parse((String ?) form["book-category-id"] ?? "");

            System.Console.WriteLine(form["book-category-id"]);

            _db.Books.Update(book);

            _db.SaveChanges();
        } catch (Exception) {
            ViewData["EditBookMessage"] = "Invalid field format";

            return View();
        }

        ViewData["EditBookMessage"] = "Book edited successfully";

        return RedirectToAction("EditBook", bookId);
    }

    [Route("editbookcopy/{bookCopyId:int}")]
    public IActionResult EditBookCopy(int bookCopyId) {
        if (HttpContext.Session.GetString("username") == null || HttpContext.Session.GetString("username") == "") {
            return RedirectToAction("Login", "User");
        }

        SetViewDataFromSession();

        BookCopyModel ?bookCopy = (
            from bookCopyModel in _db.BookCopies where bookCopyModel.BookCopyId == bookCopyId select bookCopyModel
        ).FirstOrDefault();

        if (bookCopy is null) {
            return RedirectToAction("ListBooks");
        }

        BookModel ?book = (
            from bookModel in _db.Books where bookModel.BookId == bookCopy.BookId select bookModel
        ).FirstOrDefault();

        if (book is null) {
            return RedirectToAction("ListBooks");
        }

        String ?categoryName = (
            from bookCategory in _db.BookCategories where bookCategory.BookCategoryId == book.BookCategoryId select bookCategory.Name
        ).FirstOrDefault();

        DateTime currentDate = DateTime.Now;

        String []fieldNamesToDraw = {
            "book-copy-id",
            "book-id",
            "book-title",
            "book-author",
            "book-publisher",
            "book-isbn",
            "book-publication-date",
            "book-category-name",
            "current-date"
        };

        BorrowingData[] borrowingData = (
            from borrowing in _db.Borrowings join
                reader in _db.Readers on borrowing.ReaderId equals reader.ReaderId join
                bookCopyEl in _db.BookCopies on borrowing.BookCopyId equals bookCopyEl.BookCopyId
                where bookCopyEl.BookCopyId == bookCopyId
                select new BorrowingData {
                    BorrowingId = borrowing.BorrowingId,
                    ReaderId = reader.ReaderId,
                    ReaderName = reader.FirstName,
                    ReaderSurname = reader.LastName,
                    BookCopyId = bookCopyEl.BookCopyId,
                    BorrowingDate = borrowing.BorrowDate,
                    ReturnDate = borrowing.ReturnDate
                }
        ).ToArray();

        bool isBorrowed = borrowingData.Any(borrowing => borrowing.ReturnDate is null);

        ViewData["field-names-to-draw"] = fieldNamesToDraw;

        ViewData["book-copy-id"] = bookCopy.BookCopyId;
        ViewData["book-id"] = book.BookId;
        ViewData["book-title"] = book.Title;
        ViewData["book-author"] = book.Author;
        ViewData["book-publisher"] = book.Publisher;
        ViewData["book-isbn"] = book.Isbn;
        ViewData["book-publication-date"] = book.PublicationDate?.ToString("yyyy-MM-dd");
        ViewData["book-category-id"] = book.BookCategoryId;
        ViewData["book-category-name"] = categoryName;
        ViewData["current-date"] = currentDate.ToString("yyyy-MM-dd");

        ViewData["borrowingdata"] = borrowingData;

        ViewData["is-borrowed"] = isBorrowed;

        return View();
    }

    [Route("editbookcopy/{bookCopyId:int}")]
    [HttpPost]
    public IActionResult EditBookCopy(int bookCopyId, IFormCollection form) {
        if (HttpContext.Session.GetString("username") == null || HttpContext.Session.GetString("username") == "") {
            return RedirectToAction("Login", "User");
        }

        System.Console.WriteLine("edit book copy");
        System.Console.WriteLine(bookCopyId);
        System.Console.WriteLine("=========");

        SetViewDataFromSession();

        bool isCorrectId = (
            from bookCopyModel in _db.BookCopies where bookCopyModel.BookCopyId == bookCopyId select bookCopyModel
        ).Count() != 0;

        if (!isCorrectId) {
            return RedirectToAction("ListBooks");
        }

        bool canBeBorrowed = (
            from borrowing in _db.Borrowings where borrowing.BookCopyId == bookCopyId && borrowing.ReturnDate == null select borrowing
        ).Count() == 0;

        Console.WriteLine("return: " + form["return"]);

        if ((String ?) form["borrow"] == "Borrow") {
            String ?readerid = (String ?) form["readerid"];

            if (readerid is null || readerid == "") {
                ViewData["EditBookCopyMessageForm"] = "Reader ID is required";

                return RedirectToAction("EditBookCopy", bookCopyId);
            }

            if (!canBeBorrowed) {
                ViewData["EditBookCopyMessageForm"] = "Book copy cannot be borrowed";

                return RedirectToAction("EditBookCopy", bookCopyId);
            }

            try {
                int readerIdInt = Int32.Parse(readerid);

                bool isValidId = (
                    from reader in _db.Readers where reader.ReaderId == readerIdInt select reader
                ).Count() > 0;

                if (!isValidId) {
                    ViewData["EditBookCopyMessageForm"] = "Invalid reader ID";

                    return RedirectToAction("EditBookCopy", bookCopyId);
                }

                BorrowingModel borrowing = new BorrowingModel {
                    ReaderId = readerIdInt,
                    BookCopyId = bookCopyId,
                    BorrowDate = DateTime.Now,
                    ReturnDate = null
                };

                _db.Borrowings.Add(borrowing);

                _db.SaveChanges();
            } catch (Exception) {
                ViewData["EditBookCopyMessageForm"] = "Invalid field format";

                return RedirectToAction("EditBookCopy", bookCopyId);
            }
        } else if ((String ?) form["return"] == "Return") {
            System.Console.WriteLine("Return");

            try {
                BorrowingModel ?borrowing = (
                    from borrowingModel in _db.Borrowings where borrowingModel.BookCopyId == bookCopyId && borrowingModel.ReturnDate == null
                    select borrowingModel
                ).FirstOrDefault();

                if (borrowing is null) {
                    ViewData["EditBookCopyMessageForm"] = "Book copy cannot be returned";

                    return RedirectToAction("EditBookCopy", bookCopyId);
                }

                borrowing.ReturnDate = DateTime.Now;

                _db.Borrowings.Update(borrowing);

                _db.SaveChanges();
            } catch (Exception) {
                ViewData["EditBookCopyMessageForm"] = "Invalid field format";

                return RedirectToAction("EditBookCopy", bookCopyId);
            }
        }

        return RedirectToAction("EditBookCopy", bookCopyId);
    }

    [Route("addreader/")]
    public IActionResult AddReader() {
        if (HttpContext.Session.GetString("username") == null || HttpContext.Session.GetString("username") == "") {
            return RedirectToAction("Login", "User");
        }

        SetViewDataFromSession();

        return View();
    }

    [Route("addreader/")]
    [HttpPost]
    public IActionResult AddReader(IFormCollection form) {
        if (HttpContext.Session.GetString("username") == null || HttpContext.Session.GetString("username") == "") {
            return RedirectToAction("Login", "User");
        }

        SetViewDataFromSession();

        List<String> fields = new List<String> {
            "name",
            "surname"
        };

        foreach (String field in fields) {
            if (((String ?) form[field]) is null || (String ?) form[field] == "") {
                ViewData["AddReaderMessage"] = "All fields are required";

                return View();
            }
        }

        _db.Add(new ReaderModel {
            FirstName = ((String ?) form["name"]) ?? "",
            LastName = ((String ?) form["surname"]) ?? ""
        });

        _db.SaveChanges();

        ViewData["AddReaderMessage"] = "Reader added successfully";

        return RedirectToAction("AddReader"); 
    }

    public List<Puchary> ReadCsvFileTP(string filePath, char separator)
    {
        List<Puchary> data = new List<Puchary>();

        using (StreamReader reader = new StreamReader(filePath))
        {
            string headerLine = reader.ReadLine();
            string[] headerValues = headerLine.Split(separator);

            // Utwórz nowy obiekt Pilkarze z wartościami nagłówka
            var headerPuchary = new Puchary
            {
                n = headerValues[0],
                m = headerValues[1],
                r = headerValues[2]
            };

            // Dodaj obiekt nagłówka do listy
            data.Add(headerPuchary);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(separator);

                var Puchary = new Puchary
                {
                    Nazwa_Turnieju = values[0],
                    Miejsce = values[1],
                    Rok_zdobycia = int.Parse(values[2])
                };

                data.Add(Puchary);
            }
        }

        return data;
    }

    [Route("Puchary/")]
    public IActionResult Puchary()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Excel", "Puchary.csv");
        List<Puchary> data = new List<Puchary>();

        if (System.IO.File.Exists(path))
        {
            data = ReadCsvFileTP(path, ';');
        }

        return View(data);
    }

    [HttpGet("{*url}", Order = 999)]
    public IActionResult CatchAll() {
        SetViewDataFromSession();

        return RedirectToAction("Index", "Home");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}