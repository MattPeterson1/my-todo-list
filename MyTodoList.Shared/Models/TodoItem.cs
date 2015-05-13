using System;

#if !WINDOWS_PHONE_APP
using System.ComponentModel.DataAnnotations;
#endif


namespace MyTodoList.Shared.Models
{
    public class TodoItem
    {
#if !WINDOWS_PHONE_APP
        [Key]
#endif
        public string Id { get; set; }
        public string OwnerId { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string Text { get; set; }
    }
}
