using System;
namespace Fall2024_Assignment3_cchall5.Models
{
    public class ActorViewModel
    {
        public Actor Actor { get; set; }
        public List<CommentSentiment> TweetsWithSentiments { get; set; }
        public double OverallSentiment { get; set; }
    }
}

