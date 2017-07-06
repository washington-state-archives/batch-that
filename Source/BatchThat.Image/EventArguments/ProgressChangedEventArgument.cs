using System.Drawing;

namespace BatchThat.Image.EventArguments
{
    public class ProgressChangedEventArgument
    {
        public int Current { get; set; }
        public int Total { get; set; }
        public ChangedEventMessage Message { get; set; }
        public Bitmap Image { get; set; }
    }
}
