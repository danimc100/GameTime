using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameTime.Core
{
    using System.Reflection;

    public static class ListViewExtensions
    {
        public static void SetDoubleBuffered(this ListView listView, bool value)
        {
            listView.GetType()
                .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(listView, value);
        }
    }
}
