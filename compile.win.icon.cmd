cls

del *.exe

csc ^
/t:winexe ^
/r:System.Drawing.dll,System.Windows.Forms.dll,Microsoft.VisualBasic.dll ^
/win32icon:image.ico ^
/out:ImageViewer.exe ^
ImageViewerForm.cs ImageViewerControl.cs AdjustDialog.cs

@rem pause
