using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using SVD;

namespace ImageSVD
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region Fields

        private Bitmap _bmpOriginal;
        private Bitmap _bmpU;
        private Bitmap _bmpV;

        private double[,] _a;
        private SingularValueDecomposition _svd;

        #endregion

        #region EventHandlers

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Execute(openFileDialog.FileName);

                    radioButton_CheckedChanged(null, null);

                    buttonSave.Enabled = true;
                }
                catch (Exception ex)
                {
                    //buttonSave.Enabled = false;
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void buttonOpenFolder_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var directoryName = Path.GetDirectoryName(openFileDialog.FileName);
                    var resultDirectoryName = Path.Combine(directoryName, "SVD");

                    if (Directory.Exists(resultDirectoryName))
                    {
                        Directory.Delete(resultDirectoryName, true);
                    }

                    Directory.CreateDirectory(resultDirectoryName);

                    foreach (var fileInfo in new DirectoryInfo(directoryName).GetFiles())
                    {
                        Execute(fileInfo.FullName);

                        var fileName = Path.GetFileNameWithoutExtension(fileInfo.Name);

                        SaveMatrix(/*A*/_svd.A, Path.Combine(resultDirectoryName, fileName + "_A.txt"));
                        SaveMatrix(_svd.U, Path.Combine(resultDirectoryName, fileName + "_U.txt"));
                        SaveMatrix(_svd.S, Path.Combine(resultDirectoryName, fileName + "_S.txt"));
                        SaveMatrix(_svd.V, Path.Combine(resultDirectoryName, fileName + "_V.txt"));

                        _bmpOriginal.Save(Path.Combine(resultDirectoryName, fileName + "_Original.bmp"), ImageFormat.Bmp);
                        _bmpU.Save(Path.Combine(resultDirectoryName, fileName + "_U.bmp"), ImageFormat.Bmp);
                        _bmpV.Save(Path.Combine(resultDirectoryName, fileName + "_V.bmp"), ImageFormat.Bmp);
                    }

                    radioButton_CheckedChanged(null, null);
                }
                catch (Exception ex)
                {
                    //buttonSave.Enabled = false;
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                var directoryName = Path.GetDirectoryName(saveFileDialog.FileName);

                SaveMatrix(/*A*/_svd.A, Path.Combine(directoryName, "A.txt"));
                SaveMatrix(_svd.U, Path.Combine(directoryName, "U.txt"));
                SaveMatrix(_svd.S, Path.Combine(directoryName, "S.txt"));
                SaveMatrix(_svd.V, Path.Combine(directoryName, "V.txt"));

                var syntez = new Syntez(_svd.A, _svd.U, _svd.V);
                SaveMatrix(syntez.AU1, Path.Combine(directoryName, "au1.txt"));
                SaveMatrix(syntez.AU2, Path.Combine(directoryName, "au2.txt"));
                SaveMatrix(syntez.AV1, Path.Combine(directoryName, "av1.txt"));
                SaveMatrix(syntez.AV2, Path.Combine(directoryName, "av2.txt"));

                _bmpOriginal.Save(saveFileDialog.FileName, ImageFormat.Bmp);
                _bmpU.Save(Path.Combine(directoryName, "U.bmp"), ImageFormat.Bmp);
                _bmpV.Save(Path.Combine(directoryName, "V.bmp"), ImageFormat.Bmp);

                _bmpU = CreateBitmap(syntez.AU1);
                _bmpU.Save(Path.Combine(directoryName, "AU1.bmp"), ImageFormat.Bmp);
                _bmpU = CreateBitmap(syntez.AU2);
                _bmpU.Save(Path.Combine(directoryName, "AU2.bmp"), ImageFormat.Bmp);

                _bmpV = CreateBitmap(syntez.AV1);
                _bmpV.Save(Path.Combine(directoryName, "AV1.bmp"), ImageFormat.Bmp);
                _bmpV = CreateBitmap(syntez.AV2);
                _bmpV.Save(Path.Combine(directoryName, "AV2.bmp"), ImageFormat.Bmp);
            }
        }

        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (radioButtonOriginal.Checked)
                {
                    pictureBox1.Image = _bmpOriginal;
                }
                else if (radioButtonU.Checked)
                {
                    pictureBox1.Image = _bmpU;
                }
                else if (radioButtonV.Checked)
                {
                    pictureBox1.Image = _bmpV;
                }
                else
                {
                    throw new Exception("RadioButton error!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                _bmpU = CreateBitmap(_svd.U);
                _bmpV = CreateBitmap(_svd.V);

                radioButton_CheckedChanged(null, null);
            }
            catch
            { }
        }

        #endregion

        #region Methods

        private void Execute(string fileName)
        {
            _bmpOriginal = new Bitmap(fileName);

            var bmpArray = new double[_bmpOriginal.Width, _bmpOriginal.Height];

            for (var iCount = 0; iCount < _bmpOriginal.Width; iCount++)
            {
                for (var jCount = 0; jCount < _bmpOriginal.Height; jCount++)
                {
                    var pixelColor = _bmpOriginal.GetPixel(iCount, jCount);
                    var grayscalePixelColor = pixelColor.R * 0.3 + pixelColor.G * 0.59 + pixelColor.B * 0.11;
                    _bmpOriginal.SetPixel(iCount, jCount, Color.FromArgb(255, (int)grayscalePixelColor, (int)grayscalePixelColor, (int)grayscalePixelColor));

                    bmpArray[iCount, jCount] = grayscalePixelColor;
                }
            }

            _a = bmpArray;

            _svd = new SingularValueDecomposition(_a);

            _bmpU = CreateBitmap(_svd.U);
            _bmpV = CreateBitmap(_svd.V);
        }

        private void SaveMatrix(double[,] matrix, string fileName)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                for (int iCount = 0; iCount < matrix.GetLength(0); iCount++)
                {
                    for (int jCount = 0; jCount < matrix.GetLength(1); jCount++)
                    {
                        //if (jCount == 0)
                        //{
                        //    int l;

                        //    if (iCount == 0)
                        //    {
                        //        for (int i = 0; i <= matrix.GetLength(1); i++)
                        //        {
                        //            sw.Write(i.ToString());

                        //            l = i.ToString().Length;

                        //            for (int n = l; n < 25; n++)
                        //                sw.Write(" ");
                        //        }

                        //        sw.WriteLine();
                        //    }

                        //    sw.Write((iCount + 1).ToString());

                        //    l = (iCount + 1).ToString().Length;

                        //    for (int i = l; i < 25; i++)
                        //        sw.Write(" ");
                        //}

                        var value = matrix[iCount, jCount] == 0 ? "0.0" : matrix[iCount, jCount].ToString();

                        sw.Write(/*matrix[iCount, jCount].ToString()*/ value.Replace(',', '.'));

                        int len = /*matrix[iCount, jCount].ToString()*/value.Length;

                        for (int i = len; i < 25; i++)
                            sw.Write(" ");
                    }

                    sw.WriteLine();
                }
            }
        }

        private void SaveMatrix(double[] matrix, string fileName)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                for (int iCount = 0; iCount < matrix.Length; iCount++)
                {
                    var value = matrix[iCount] == 0 ? "0.0" : matrix[iCount].ToString();

                    sw.Write(/*matrix[iCount, jCount].ToString()*/ value.Replace(',', '.'));

                    int len = /*matrix[iCount, jCount].ToString()*/value.Length;

                    for (int i = len; i < 25; i++)
                        sw.Write(" ");
                }

                sw.WriteLine();
            }
        }

        private Bitmap CreateBitmap(double[,] matrix)
        {
            var bmp = new Bitmap(matrix.GetLength(1), matrix.GetLength(0));

            var delta = (int)Math.Pow(10, trackBar1.Value);

            for (var iCount = 0; iCount < matrix.GetLength(1); iCount++)
            {
                for (var jCount = 0; jCount < matrix.GetLength(0); jCount++)
                {
                    bmp.SetPixel(iCount, jCount, Color.FromArgb(255,
                                                                (int)Math.Abs(matrix[jCount, iCount] * delta % 256),
                                                                (int)Math.Abs(matrix[jCount, iCount] * delta % 256),
                                                                (int)Math.Abs(matrix[jCount, iCount] * delta % 256)));
                }
            }

            return bmp;
        }

        #endregion
    }
}