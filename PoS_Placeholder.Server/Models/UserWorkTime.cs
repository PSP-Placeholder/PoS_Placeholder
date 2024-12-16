using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PoS_Placeholder.Server.Models
{
    public class UserWorkTime
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Day { get; set; }

        [Required]
        public TimeOnly StartTime { get; set; }

        [Required]
        public TimeOnly EndTime { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        public User user { get; set; }
        public TimeOnly? BreakStart { get; set; }

        public TimeOnly? BreakEnd { get; set; }
    }
}
