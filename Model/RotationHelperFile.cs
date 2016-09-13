using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace RotationHelper.Model
{
    [Serializable]
    public class RotationHelperFile
    {
        #region Constructors

        public RotationHelperFile()
        {
            Rotations = new ObservableCollection<Rotation>();
        }

        #endregion

        #region Properties

        public ObservableCollection<Rotation> Rotations { get; }

        public string Title { get; set; }

        #endregion

        #region Methods

        public static RotationHelperFile Deserialize(string path)
        {
            try
            {
                RotationHelperFile wagFile;

                using (Stream stream = File.Open(path, FileMode.Open))
                {
                    var bin = new XmlSerializer(typeof(RotationHelperFile));

                    wagFile = (RotationHelperFile)bin.Deserialize(stream);
                }

                return wagFile;
            }
            catch
            {
                MessageBox.Show("Unable to load the selected file");
                return null;
            }
        }

        public void Serialize(string path)
        {
            try
            {
                using (var stream = File.Open(path, FileMode.Create))
                {
                    Serialize(stream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                MessageBox.Show("Unable to save rotation file");
            }
        }

        private void Serialize(Stream stream)
        {
            var bin = new XmlSerializer(typeof(RotationHelperFile));
            bin.Serialize(stream, this);
        }

        #endregion
    }
}