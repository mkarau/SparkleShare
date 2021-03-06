//   SparkleShare, a collaboration and sharing tool.
//   Copyright (C) 2010  Hylke Bons <hylkebons@gmail.com>
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General Public License for more details.
//
//   You should have received a copy of the GNU General Public License
//   along with this program. If not, see <http://www.gnu.org/licenses/>.


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

#if __MonoCS__
using Mono.Unix;
//using Mono.Unix.Native;
#endif
using SparkleLib;
using SparkleLib.Options;

namespace SparkleShare {

    // This is SparkleShare!
    public class Program {

        public static SparkleController Controller;
        public static SparkleUI UI;


        // Short alias for the translations
        public static string _ (string s)
        {
#if __MonoCS__
            return Catalog.GetString (s);
#else
            return Strings.T(s);
#endif
        }

#if !__MonoCS__
        public static void TranslateWinForm (System.Windows.Forms.Form form)
        {
            form.Text = Program._ (form.Text);

            foreach (var label in form.Controls.All ().OfType<System.Windows.Forms.Label> ()) {
                label.Text = Program._ (label.Text);
            }
            foreach (var button in form.Controls.All ().OfType<System.Windows.Forms.Button> ()) {
                button.Text = Program._ (button.Text);
            }
        }
#endif

#if !__MonoCS__
        [STAThread]
#endif
        public static void Main (string [] args)
        {
            SetUiCulture();

            // Parse the command line options
            bool show_help       = false;
            OptionSet option_set = new OptionSet () {
                { "v|version", _("Print version information"), v => { PrintVersion (); } },
                { "h|help", _("Show this help text"), v => show_help = v != null }
            };

            try {
                option_set.Parse (args);

            } catch (OptionException e) {
                Console.Write ("SparkleShare: ");
                Console.WriteLine (e.Message);
                Console.WriteLine ("Try `sparkleshare --help' for more information.");
            }

            if (show_help)
                ShowHelp (option_set);


            // Initialize the controller this way so that
            // there aren't any exceptions in the OS specific UI's
            Controller = new SparkleController ();
            Controller.Initialize ();
        
            if (Controller != null) {
                UI = new SparkleUI ();
                UI.Run ();
            }

#if !__MonoCS__
            // For now we must do GC.Collect to free some internal handles, otherwise
            // in debug mode you can got assertion message.
            GC.Collect (GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers ();
            CefSharp.CEF.Shutdown ();    // Shutdown CEF.
#endif
        }

        public static void SetUiCulture()
        {
            //var culture = CultureInfo.GetCultureInfo ("en"); // FIXME: test only
            //System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
        }


        // Prints the help output
        public static void ShowHelp (OptionSet option_set)
        {
            Console.WriteLine (" ");
            Console.WriteLine (_("SparkleShare, a collaboration and sharing tool."));
            Console.WriteLine (_("Copyright (C) 2010 Hylke Bons"));
            Console.WriteLine (" ");
            Console.WriteLine (_("This program comes with ABSOLUTELY NO WARRANTY."));
            Console.WriteLine (" ");
            Console.WriteLine (_("This is free software, and you are welcome to redistribute it "));
            Console.WriteLine (_("under certain conditions. Please read the GNU GPLv3 for details."));
            Console.WriteLine (" ");
            Console.WriteLine (_("SparkleShare automatically syncs Git repositories in "));
            Console.WriteLine (_("the ~/SparkleShare folder with their remote origins."));
            Console.WriteLine (" ");
            Console.WriteLine (_("Usage: sparkleshare [start|stop|restart] [OPTION]..."));
            Console.WriteLine (_("Sync SparkleShare folder with remote repositories."));
            Console.WriteLine (" ");
            Console.WriteLine (_("Arguments:"));

            option_set.WriteOptionDescriptions (Console.Out);
            Environment.Exit (0);
        }


        // Prints the version information
        public static void PrintVersion ()
        {
            Console.WriteLine (_("SparkleShare " + Defines.VERSION));
            Environment.Exit (0);
        }

#if __MonoCS__
        // Strange magic needed by SetProcessName ()
        [DllImport ("libc")]
        private static extern int prctl (int option, byte [] arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5);
#endif
        
        // Sets the Unix process name to 'sparkleshare' instead of 'mono'
        private static void SetProcessName (string name)
        {
#if __MonoCS__
            try {
                if (prctl (15, Encoding.ASCII.GetBytes (name + "\0"), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero) != 0)
                    throw new ApplicationException ("Error setting process name: " +
                                                    Mono.Unix.Native.Stdlib.GetLastError ());

            } catch (EntryPointNotFoundException) {
                Console.WriteLine ("SetProcessName: Entry point not found");
            }
#endif
        }
    }
}
