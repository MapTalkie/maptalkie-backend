using System.Collections.Generic;

namespace MapTalkie.Utils
{
    public class ListResponse<T>
    {
        public ListResponse(List<T> items)
        {
            Items = items;
        }

        public List<T> Items { get; set; }
    }

    public static class ListResponse
    {
        public static ListResponse<T> Of<T>(List<T> list)
        {
            return new ListResponse<T>(list);
        }
    }
}