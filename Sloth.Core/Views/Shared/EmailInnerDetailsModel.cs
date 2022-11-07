
namespace Sloth.Core.Views.Shared
{
    public class EmailInnerDetailsModel
    {
        public EmailInnerDetailsModel(string heading, string text)
        {
            Heading = heading;
            Text = text;
        }

        public string Heading { get; set; }
        public string Text { get; set; }
    }
}
