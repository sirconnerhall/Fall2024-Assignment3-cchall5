using System;
using System.ComponentModel.DataAnnotations;

namespace Fall2024_Assignment3_cchall5.Models
{
	public class Movie
	{
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [Url]
        public string ImdbLink { get; set; }

        [Required]
        [StringLength(50)]
        public string Genre { get; set; }

        [Required]
        public int ReleaseYear { get; set; }

        // photo is not required
        public byte[]? MoviePhoto { get; set; }

        public List<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    }
}

