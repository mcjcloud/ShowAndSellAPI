using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShowAndSellAPI.Models.Database;
using ShowAndSellAPI.Models;
using ShowAndSellAPI.Models.Http;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ShowAndSellAPI.Controllers
{
    [Route("api/[controller]")]
    public class BookmarksController : Controller
    {
        // Properties
        private readonly SSDbContext _context;

        // CONSTRUCTOR
        public BookmarksController(SSDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetBookmarks([FromQuery]string userId, [FromQuery]string password)
        {
            return _context.GetBookmarkedItems(userId, password);
        }

        [HttpPost]
        public IActionResult CreateBookmark([FromBody]AddBookmarkRequest request)
        {
            return _context.CreateBookmark(request.UserId, request.ItemId);
        }

        [HttpDelete("{bookmarkId}")]
        public IActionResult DeleteBookmark(string bookmarkId)
        {
            return _context.DeleteBookmark(bookmarkId);
        }
    }
}
