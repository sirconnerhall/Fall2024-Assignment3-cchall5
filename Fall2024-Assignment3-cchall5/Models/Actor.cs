using System;
using System.ComponentModel.DataAnnotations;

namespace Fall2024_Assignment3_cchall5.Models
{

    // contains all fields for each actor
	public class Actor
	{
        
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        public required string Gender { get; set; }

        [Required]
        public required int Age { get; set; }

        [Required]
        [Url]
        public required string ImdbLink { get; set; }

        // photo is not required
        public byte[]? ActorPhoto { get; set; }

        public List<MovieActor> MovieActors { get; set; } = new List<MovieActor>();

    }

}

