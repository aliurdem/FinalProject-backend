using EdirneTravel.Models.Dtos.Base;

namespace EdirneTravel.Models.Dtos
{
    public class TravelRoutePlaceDto : BaseDto
    {
        public int PlaceId { get; set; }
        public int Sequence { get; set; }

        // Place detayları
        public string Name { get; set; }
        public string Description { get; set; }
        public byte[] ImageData { get; set; }
        public int CategoryId { get; set; }

    }
}
