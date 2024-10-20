using System;
using System.ComponentModel.DataAnnotations;

namespace Fall2024_Assignment3_cchall5.Models
{

    // contains all fields for each actor
	public class Actor
	{
        
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public int Age { get; set; }

        [Required]
        [Url]
        public string ImdbLink { get; set; }

        // photo is not required
        public byte[]? ActorPhoto { get; set; }

        public List<MovieActor> MovieActors { get; set; }

    }

}

