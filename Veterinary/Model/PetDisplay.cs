using System;

namespace Veterinary.Model
{
    // This class is used only to display pet info on the home page
    public class PetDisplay
    {
        public int PetID { get; set; }
        public int OwnerID { get; set; }
        public string PetName { get; set; }
        public string Species { get; set; }
        public string? Breed { get; set; }
        public decimal? Weight { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
