﻿using System;
using POESKillTree.Views;

namespace POESKillTree.SkillTreeFiles
{
    // Application entry class.
    class Bootstrap : MarshalByRefObject
    {
        // Entry point method.
        [STAThread]
        public static void Main(string[] arguments)
        {
            App app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
