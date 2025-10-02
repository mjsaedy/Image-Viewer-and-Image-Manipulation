using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;
using Microsoft.VisualBasic;

static class Program {

    [STAThread]
    static void Main(string[] args) {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}

public class MainForm : Form {

    public Viewer viewer;
    MenuStrip MainMenu;
    ToolStripMenuItem mnuFile,
                      mnuFileOpen,
                      mnuFileRevert,
                      mnuFileSaveAs,
                      mnuFileQuality,
                      mnuFileExit,
                      //mnuImageFit,
                      //mnuImageActualSize,
                      mnuImageResize,
                      mnuImageMirror,
                      mnuImageGamma,
                      mnuImageBrightness,
                      mnuImageContrast,
                      mnuImageSaturation,
                      mnuImageAdjust;
    
    private long _jpegQuality = 85; // default JPEG quality
    private string _fileName = null;
    private int _dlgX = 0, _dlgY = 0;
    private bool _fitToWindow = true;

    public Bitmap originalImage; // store original for live updates and to revert


    public MainForm() {
        Text = "Image Viewer Pan & Zoom";
        Width = 1000;
        Height = 600;
        StartPosition = FormStartPosition.CenterScreen;
        this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

        mnuFile = new ToolStripMenuItem("File");
        //---------------------------------------
        mnuFileOpen = new ToolStripMenuItem("Open...", null, OnOpenClicked);
        mnuFileRevert = new ToolStripMenuItem("Revert", null, OnRevertClicked);
        mnuFileSaveAs = new ToolStripMenuItem("Save As...", null, (s, e) => SaveAsImage());
        mnuFileQuality = new ToolStripMenuItem("Set JPEG Quality", null, (s, e) => SetJpegQuality());
        mnuFileExit = new ToolStripMenuItem("Exit", null, (s, e) => Close());
        mnuFile.DropDownItems.Add(mnuFileOpen);
        mnuFile.DropDownItems.Add(mnuFileSaveAs);
        mnuFile.DropDownItems.Add(mnuFileRevert);
        mnuFile.DropDownItems.Add(new ToolStripSeparator());
        mnuFile.DropDownItems.Add(mnuFileQuality);
        mnuFile.DropDownItems.Add(new ToolStripSeparator());
        mnuFile.DropDownItems.Add(mnuFileExit);

        
        var mnuImage = new ToolStripMenuItem("Image");
        //---------------------------------------
        //mnuImageFit = new ToolStripMenuItem("Fit to window (F)", null, (s, e) => viewer.FitImage());
        //mnuImageActualSize = new ToolStripMenuItem("Actual size (R)", null, (s, e) => viewer.ResetView());
        mnuImageResize = new ToolStripMenuItem("Resize", null, (s, e) => { viewer.ScaleImage();
                                                                           _fitToWindow = !_fitToWindow;
                                                                          });
        mnuImageMirror = new ToolStripMenuItem("Mirror", null, (s, e) => viewer.MirrorImage());
        mnuImageGamma = new ToolStripMenuItem("Gamma", null, (s, e) => viewer.ApplyGamma());
        mnuImageBrightness = new ToolStripMenuItem("Brightness", null,
                                                (s, e) => viewer.AdjustBrightness());
        mnuImageContrast = new ToolStripMenuItem("Contrast", null,
                                                (s, e) => viewer.AdjustContrast());
        mnuImageSaturation = new ToolStripMenuItem("Saturation", null,
                                                  (s, e) => viewer.AdjustSaturation());
        mnuImageAdjust = new ToolStripMenuItem("Adjust Image...", null, (s, e) => OpenAdjustDialog());

        //mnuImage.DropDownItems.Add(mnuImageFit);
        //mnuImage.DropDownItems.Add(mnuImageActualSize);
        //mnuImage.DropDownItems.Add(new ToolStripSeparator());
        mnuImage.DropDownItems.Add(mnuImageResize);
        mnuImage.DropDownItems.Add(mnuImageMirror);
        mnuImage.DropDownItems.Add(mnuImageGamma);
        mnuImage.DropDownItems.Add(mnuImageBrightness);
        mnuImage.DropDownItems.Add(mnuImageContrast);
        mnuImage.DropDownItems.Add(mnuImageSaturation);
        mnuImage.DropDownItems.Add(new ToolStripSeparator());
        mnuImage.DropDownItems.Add(mnuImageAdjust);

        MainMenu = new MenuStrip();
        MainMenu.Items.Add(mnuFile);
        MainMenu.Items.Add(mnuImage);

        this.MainMenuStrip = MainMenu;
        this.Controls.Add(MainMenu);


        viewer = new Viewer(this);
        viewer.Dock = DockStyle.Fill;
        Controls.Add(viewer);

        MainMenu.BringToFront();

        AllowDrop = true;
        DragEnter += MainForm_DragEnter;
        DragDrop += MainForm_DragDrop;

        KeyPreview = true;
        KeyDown += MainForm_KeyDown;
        
        this.SizeChanged  += MainForm_SizeChanged;
        this.Shown += MainForm_Shown;
        this.FormClosing += (s, e) => SaveSettings();
        
        LoadSettings();
    }

    private void MainForm_SizeChanged (object sender, EventArgs e)  {
        if (_fitToWindow)
            viewer.FitImage();
    }


    private void MainForm_Shown(object sender, EventArgs e)  {
        string argFile = getArgument();
        if (argFile != null) {
            viewer.LoadImage(argFile);
            originalImage = new Bitmap(viewer.image);
            _fileName = argFile;
        }
    }

    void OnOpenClicked(object sender, EventArgs e) {
        using (var dlg = new OpenFileDialog()) {
            dlg.Filter = "Images|*.bmp;*.png;*.jpg;*.jpeg;*.gif;*.tif;*.tiff|All files|*.*";
            dlg.Title = "Open";
            if (dlg.ShowDialog(this) == DialogResult.OK) {
                viewer.LoadImage(dlg.FileName);
                originalImage = new Bitmap(viewer.image);
                _fileName = dlg.FileName;
            }
        }
    }

    void OnRevertClicked(object sender, EventArgs e) {
        if (_fileName == null)
            return;
        // Reload from file
        viewer.image.Dispose();
        viewer.LoadImage(_fileName);
        viewer.Invalidate();
    }

    void MainForm_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0 && IsImageFile(files[0]))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }
    }

    void MainForm_DragDrop(object sender, DragEventArgs e) {
        var files = (string[])e.Data.GetData(DataFormats.FileDrop);
        if (files != null && files.Length > 0 && IsImageFile(files[0])) {
            viewer.LoadImage(files[0]);
            originalImage = new Bitmap(viewer.image);
            _fileName = files[0];
        }
    }

    bool IsImageFile(string path) {
        string ext = Path.GetExtension(path).ToLowerInvariant();
        return ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp" ||
               ext == ".gif" || ext == ".tif" || ext == ".tiff";
    }

    void MainForm_KeyDown(object sender, KeyEventArgs e) {
        if (e.KeyCode == Keys.Escape)
            Close();

        /*
        if (e.KeyCode == Keys.R)
            viewer.ResetView();
        */

        else if (e.KeyCode == Keys.F) {
            _fitToWindow = !_fitToWindow;
            if (_fitToWindow)
                viewer.FitImage();
            else
                viewer.ResetView();

        }
        else if (e.KeyCode == Keys.O && e.Control)
            OnOpenClicked(null, EventArgs.Empty);
    }

    private void SetJpegQuality() {
        string input = Microsoft.VisualBasic.Interaction.InputBox(
            "Enter JPEG quality (1–100):",
            "Set JPEG Quality",
            _jpegQuality.ToString()
        );
        if (int.TryParse(input, out int q) && q >= 1 && q <= 100) {
            _jpegQuality = q;
        } else if (!string.IsNullOrEmpty(input)) {
            MessageBox.Show("Invalid quality value. Please enter a number between 1 and 100.");
        }
    }

    private void SaveAsImage() {
        if (viewer.image == null)
            return;
        using (SaveFileDialog dlg = new SaveFileDialog()) {
            dlg.Filter = "JPEG Image|*.jpg;*.jpeg|PNG Image|*.png|Bitmap Image|*.bmp|GIF Image|*.gif";
            dlg.Title = "Save Image As";
            if (string.IsNullOrEmpty(_fileName)) {
                dlg.FileName = "";
            } else {
                dlg.InitialDirectory = Path.GetDirectoryName(_fileName);
                dlg.FileName = AppendSuffixToFileName(Path.GetFileName(_fileName), " - copy");
            }
            if (dlg.ShowDialog() == DialogResult.OK) {
                var ext = System.IO.Path.GetExtension(dlg.FileName).ToLowerInvariant();
                System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Jpeg;
                if (ext == ".jpg" || ext == ".jpeg") {
                    format = System.Drawing.Imaging.ImageFormat.Jpeg;
                }
                else if (ext == ".png")
                    format = System.Drawing.Imaging.ImageFormat.Png;
                else if (ext == ".bmp")
                    format = System.Drawing.Imaging.ImageFormat.Bmp;
                else if (ext == ".gif")
                    format = System.Drawing.Imaging.ImageFormat.Gif;

                //viewer.image.Save(dlg.FileName, format);
                if (format == System.Drawing.Imaging.ImageFormat.Jpeg) {
                    var encoder = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()
                        .FirstOrDefault(c => c.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);
                    if (encoder != null) {
                        var encParams = new System.Drawing.Imaging.EncoderParameters(1);
                        encParams.Param[0] = new System.Drawing.Imaging.EncoderParameter(
                            System.Drawing.Imaging.Encoder.Quality, _jpegQuality);
                        viewer.image.Save(dlg.FileName, encoder, encParams);
                    } else {
                        viewer.image.Save(dlg.FileName, format); // fallback
                    }
                }
                else {
                    viewer.image.Save(dlg.FileName, format);
                }
            }
        }
    }

    private string AppendSuffixToFileName(string filePath, string suffix) {
        string dir = Path.GetDirectoryName(filePath);
        string name = Path.GetFileNameWithoutExtension(filePath);
        string ext = Path.GetExtension(filePath);
        return Path.Combine(dir, name + suffix + ext);
    }


    #region Setting
    /*
    private string GetSettingsFile() {
        string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyImageViewer");
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, "settings.xml");
    }
    */

    private string GetSettingsFile() {
        string folder = AppDomain.CurrentDomain.BaseDirectory;
        return Path.Combine(folder, "settings.xml");
    }

    private void SaveSettings() {
        var s = new ProgramSettings {
            X = this.Location.X,
            Y = this.Location.Y,
            Width = this.Size.Width,
            Height = this.Size.Height,
            WindowState = this.WindowState,
            DialogX = _dlgX,
            DialogY = _dlgY,
            JpegQuality = _jpegQuality
        };

        try {
            var xs = new XmlSerializer(typeof(ProgramSettings));
            using var fs = new FileStream(GetSettingsFile(), FileMode.Create);
            xs.Serialize(fs, s);
        }
        catch {
            //MessageBox.Show("Error while saving settings");
        }
    }

    private void LoadSettings() {
        string file = GetSettingsFile();
        if (!File.Exists(file))
            return;

        try {
            var xs = new XmlSerializer(typeof(ProgramSettings));
            using var fs = new FileStream(file, FileMode.Open);
            var s = (ProgramSettings)xs.Deserialize(fs);
            this.StartPosition = FormStartPosition.Manual;
            this.WindowState = s.WindowState;
            if (this.WindowState == FormWindowState.Normal) {
                this.Size = new Size(s.Width, s.Height);
                this.Location = new Point(s.X, s.Y);
            }
            _jpegQuality = s.JpegQuality;
            _dlgX = s.DialogX;
            _dlgY = s.DialogY;
        } catch {
            //MessageBox.Show("Error while loading settings");
        }
    }
    #endregion

    private void OpenAdjustDialog() {
        if (viewer.image == null)
            return;

        using (var dlg = new ImageAdjustDialog(0f, 0f, 1f, 1f)) {
            // Store original image for live updates
            using Bitmap backup = new Bitmap(originalImage);

            dlg.ValuesChanged += (b, c, s, g) => {
                viewer.ApplyAdjustmentsLive(backup, b, c, s, g);
            };

            dlg.Left = _dlgX;
            dlg.Top = _dlgY;
            
            var result = dlg.ShowDialog();

            if (result == DialogResult.OK) {
                // Commit changes
                originalImage.Dispose();
                originalImage = new Bitmap(viewer.image);
            } else {
                // Restore original image if canceled
                viewer.image.Dispose();
                viewer.image = new Bitmap(originalImage);
                viewer.Invalidate();
            }
            
            _dlgX = dlg.Left;
            _dlgY = dlg.Top;
            
        }
    }

    private string getArgument() {
        string[] args = Environment.GetCommandLineArgs();
        // Skip first argument (executable path)
        if (args.Length > 1 && File.Exists(args[1])) {
            return args[1];
        } else {
            return null;
        }
    }

}

public class ProgramSettings {
    // Main window
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public FormWindowState WindowState { get; set; }

    // Image adjustment dialog
    public int DialogX { get; set; }
    public int DialogY { get; set; }
    
    // Persised values
    public long JpegQuality { get; set; }
    public string LastOpenDirectory { get; set; } //todo
}
