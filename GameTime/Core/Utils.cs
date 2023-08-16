using Microsoft.Toolkit.Uwp.Notifications;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTime.Core
{
    public class Utils
    {
        public static string TimeFormat(TimeSpan t)
        {
            int hours = t.Days * 24 + t.Hours;
            string format;

            if(hours < 99)
            {
                format = "{0:D2}:{1:D2}:{2:D2}";
            }
            else
            {
                if(hours < 999)
                {
                    format = "{0:D3}:{1:D2}:{2:D2}";
                }
                else
                {
                    format = "{0:D4}:{1:D2}:{2:D2}";
                }
            }

            string result = string.Format(
                format,
                (t.Days * 24) + t.Hours,
                t.Minutes,
                t.Seconds);
            
            return result;
        }

        public static void NotificacionSistemaShow(string titulo, string mensaje)
        {
            new ToastContentBuilder()
                .AddText(titulo)
                .AddText(mensaje)
                .Show();
        }
    }
}
