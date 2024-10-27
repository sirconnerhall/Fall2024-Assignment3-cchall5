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
    public class ActorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly OpenAI.Chat.ChatClient _client;

        public ActorController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            var apiKey = configuration["OpenAI:Secret"];
            var apiEndpoint = configuration["OpenAI:Endpoint"];
            AzureOpenAIClient chat = new(new Uri(apiEndpoint), new System.ClientModel.ApiKeyCredential(apiKey));
            _client = chat.GetChatClient("gpt-35-turbo");
        }

        // GET: Actor
        public async Task<IActionResult> Index()
        {
            return View(await _context.Actor.ToListAsync());
        }

        // GET: Actor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            // generate tweets about actor
            var comments = new List<String>();
            string resultText = "";
            try
            {
                var actorName = actor.Name;
                var completion = await _client.CompleteChatAsync($"Generate twenty tweets about the actor {actorName}. They should be of different styles." +
                    $"All of them should be less than 140 characters each. Separate each tweet with the '|' character. All of them should be passionate. " +
                    $"End the final tweet normally, not with the '|'. Ensure there is a fair mix of positive, negative, and neutral tweets.");
                resultText = completion.Value.Content[0].Text;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            // sentiment analysis
            var analyzer = new SentimentIntensityAnalyzer();
            double overallSentiment = analyzer.PolarityScores(resultText).Compound;
            comments = resultText.Split('|').Select(tweet => tweet.Trim()).ToList();
            var commentSentiments = comments.Select(comment => new CommentSentiment
            {
                Comment = comment,
                Sentiment = analyzer.PolarityScores(comment).Compound
            }).ToList();

            // determine movies for this actor
            var movies = await _context.MovieActor
                .Include(ma => ma.Movie)
                .Where(ma => ma.ActorId == actor.Id)
                .Select(ma => ma.Movie)
                .ToListAsync();

            // make view model
            var viewModel = new ActorViewModel
            {
                Actor = actor,
                TweetsWithSentiments = commentSentiments,
                OverallSentiment = overallSentiment,
                Movies = movies
            };

            return View(viewModel);
        }

        // GET: Actor/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Actor/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Gender,Age,ImdbLink")] Actor actor, IFormFile? photo)
        {
            if (ModelState.IsValid)
            {
                if (photo != null && photo.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    photo.CopyTo(memoryStream);
                    actor.ActorPhoto = memoryStream.ToArray();
                }
                _context.Add(actor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // GET: Actor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor.FindAsync(id);
            if (actor == null)
            {
                return NotFound();
            }
            return View(actor);
        }

        // POST: Actor/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Gender,Age,ImdbLink,ActorPhoto")] Actor actor, IFormFile? photo)
        {
            if (id != actor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingActor = await _context.Actor.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);

                    if (existingActor == null)
                    {
                        return NotFound();
                    }
                    if (photo != null && photo.Length > 0)
                    {
                        using var memoryStream = new MemoryStream();
                        await photo.CopyToAsync(memoryStream);
                        actor.ActorPhoto = memoryStream.ToArray();
                    }
                    else
                    {
                        actor.ActorPhoto = existingActor.ActorPhoto;
                    }
                    _context.Update(actor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActorExists(actor.Id))
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
            return View(actor);
        }

        // GET: Actor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        // POST: Actor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actor.FindAsync(id);
            if (actor != null)
            {
                _context.Actor.Remove(actor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActorExists(int id)
        {
            return _context.Actor.Any(e => e.Id == id);
        }
    }
}
