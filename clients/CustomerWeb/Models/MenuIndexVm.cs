using TableOrdering.Contracts;

namespace CustomerWeb.Models
{
    public class MenuIndexVm
    {
        public string TableCode { get; set; }
        public Guid? CurrentCategoryId { get; set; }
        public IReadOnlyList<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
        public IReadOnlyList<MenuItemDto> MenuItems { get; set; } = new List<MenuItemDto>();
        public CartDto? CurrentCart { get; set; }
    }
}
