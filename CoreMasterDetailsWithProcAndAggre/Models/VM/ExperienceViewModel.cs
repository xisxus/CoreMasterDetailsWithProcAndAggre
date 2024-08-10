using System.ComponentModel.DataAnnotations;

namespace CoreMasterDetailsWithProcAndAggre.Models.VM
{
    public class ExperienceViewModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public int Duration { get; set; }
    }
}