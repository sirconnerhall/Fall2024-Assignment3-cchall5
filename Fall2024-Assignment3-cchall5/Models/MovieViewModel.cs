﻿using System;
namespace Fall2024_Assignment3_cchall5.Models
{
	public class MovieViewModel
	{
        public Movie Movie { get; set; }
        public List<CommentSentiment> ReviewsWithSentiments { get; set; }
    }
}
