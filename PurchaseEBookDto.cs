using System.ComponentModel.DataAnnotations;

namespace ebooks_dotnet7_api
{
    public class PurchaseEBookDto
    {
        public required int Id { get; set; }
        public required int Amount { get; set; }

        [Range(1, int.MaxValue)]
        public required int Price { get; set; }
    }
}