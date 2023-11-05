using System.ComponentModel.DataAnnotations;

namespace Google_Event.Models
{
    public class EventModel
    {
        [Required]
        public string Summary { get; set; }
        public string Description { get; set; }
        [Required]
        public DateTime StartDateTime { get; set; }
        [Required]
        public DateTime EndDateTime { get; set; }
    }
}
