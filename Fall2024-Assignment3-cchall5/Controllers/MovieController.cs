using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Fall2024_Assignment3_cchall5.Data;
using Fall2024_Assignment3_cchall5.Models;
using Azure.AI.OpenAI;
using VaderSharp2;

namespace Fall2024_Assignment3_cchall5.Controllers
{
    public class MovieController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly OpenAI.Chat.ChatClient _client;

        public MovieController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            var apiKey = configuration["OpenAI:Secret"];
            var apiEndpoint = configuration["OpenAI:Endpoint"];
            AzureOpenAIClient chat = new(new Uri(apiEndpoint), new System.ClientModel.ApiKeyCredential(apiKey));
            _client = chat.GetChatClient("gpt-35-turbo");
        }

        // GET: Movie
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movie.ToListAsync());
        }

        // GET: Movie/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            // generate tweets about actor
            var comments = new List<String>();
            try
            {
                var movieName = movie.Title;
                var completion = await _client.CompleteChatAsync($"Generate ten reviews about the movie {movieName}. They should be of different styles." +
                    $"All of them should be between 200-500 characters each. Separate each review with the '|' character. All of them should be passionate. " +
                    $"End the final review normally, not with the '|'. Ensure there is a fair mix of positive, negative, and neutral reviews.");
                var resultText = completion.Value.Content[0].Text;
                comments = resultText.Split('|').Select(tweet => tweet.Trim()).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            // sentiment analysis
            var analyzer = new SentimentIntensityAnalyzer();
            var commentSentiments = comments.Select(comment => new CommentSentiment
            {
                Comment = comment,
                Sentiment = analyzer.PolarityScores(comment).Compound
            }).ToList();

            // make view model
            var viewModel = new MovieViewModel
            {
                Movie = movie,
                ReviewsWithSentiments = commentSentiments
            };

            return View(viewModel);
        }

        // GET: Movie/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movie/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,ImdbLink,Genre,ReleaseYear")] Movie movie, IFormFile? photo)
        {
            if (ModelState.IsValid)
            {
                if (photo != null && photo.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    photo.CopyTo(memoryStream);
                    movie.MoviePhoto = memoryStream.ToArray();
                }

                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movie/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movie/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ImdbLink,Genre,ReleaseYear,MoviePhoto")] Movie movie, IFormFile? photo)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingMovie = await _context.Movie.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);

                    if (existingMovie == null)
                    {
                        return NotFound();
                    }
                    if (photo != null && photo.Length > 0)
                    {
                        using var memoryStream = new MemoryStream();
                        await photo.CopyToAsync(memoryStream);
                        movie.MoviePhoto = memoryStream.ToArray();
                    }
                    else
                    {
                        movie.MoviePhoto = existingMovie.MoviePhoto;
                    }
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movie/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movie/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movie.FindAsync(id);
            if (movie != null)
            {
                _context.Movie.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.Id == id);
        }
    }
}
