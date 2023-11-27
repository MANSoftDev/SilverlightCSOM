
using System;
namespace SilverlightCSOM
{
    public class ListItemHelper
    {
        public override string ToString()
        {
            return Title;
        }

        public Guid ID { get; set; }
        public string Title { get; set; }
        public string Zone { get; set; }
        public int ItemIndex { get; set; }
    }
}
